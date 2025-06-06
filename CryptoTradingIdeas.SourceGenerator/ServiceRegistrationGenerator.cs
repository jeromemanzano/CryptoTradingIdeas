using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using System.Text;

namespace CryptoTradingIdeas.SourceGenerator;

[Generator]
public class ServiceRegistrationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register the source generator
        context.RegisterSourceOutput(context.CompilationProvider, GenerateSource);
    }

    private static void GenerateSource(SourceProductionContext context, Compilation compilation)
    {
        // Get the assembly prefix from the current compilation's assembly name
        var assemblyPrefix = compilation.Assembly.Name.Split('.')[0];

        // Get all types that implement our dependency interfaces
        var scopedDependencySymbol = compilation.GetTypeByMetadataName("CryptoTradingIdeas.Core.Injection.IScopedDependency");
        var singletonDependencySymbol = compilation.GetTypeByMetadataName("CryptoTradingIdeas.Core.Injection.ISingletonDependency");
        var transientDependencySymbol = compilation.GetTypeByMetadataName("CryptoTradingIdeas.Core.Injection.ITransientDependency");

        if (scopedDependencySymbol == null || singletonDependencySymbol == null || transientDependencySymbol == null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "SG0001",
                    "Missing Interface",
                    "Could not find one or more dependency interfaces",
                    "Generation",
                    DiagnosticSeverity.Error,
                    true),
                Location.None));
            return;
        }

        var scopedTypes = new List<ITypeSymbol>();
        var singletonTypes = new List<ITypeSymbol>();
        var transientTypes = new List<ITypeSymbol>();

        // Get all referenced assemblies
        var assemblies = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default) { compilation.Assembly };
        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly)
            {
                assemblies.Add(assembly);
            }
        }

        // Process all assemblies
        foreach (var assembly in assemblies)
        {
            ProcessNamespace(
                assembly.GlobalNamespace,
                scopedDependencySymbol,
                singletonDependencySymbol,
                transientDependencySymbol,
                scopedTypes,
                singletonTypes,
                transientTypes);
        }

        // Generate the registration code
        var sourceBuilder = new StringBuilder(@"
using Microsoft.Extensions.DependencyInjection;

namespace CryptoTradingIdeas.Core.Injection;

public static class ServiceRegistration
{
    public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
    {
");

        // Register scoped services
        foreach (var type in scopedTypes)
        {
            sourceBuilder.AppendLine($"        services.AddScoped<{type.ToDisplayString()}>();");
            // Register all public interfaces
            foreach (var @interface in GetValidTypeSymbols(type.AllInterfaces, assemblyPrefix))
            {
                sourceBuilder.AppendLine($"        services.AddScoped<{@interface.ToDisplayString()}, {type.ToDisplayString()}>();");
            }
        }

        // Register singleton services
        foreach (var type in singletonTypes)
        {
            sourceBuilder.AppendLine($"        services.AddSingleton<{type.ToDisplayString()}>();");
            // Register all public interfaces
            foreach (var @interface in GetValidTypeSymbols(type.AllInterfaces, assemblyPrefix))
            {
                sourceBuilder.AppendLine($"        services.AddSingleton<{@interface.ToDisplayString()}, {type.ToDisplayString()}>();");
            }
        }

        // Register transient services
        foreach (var type in transientTypes)
        {
            sourceBuilder.AppendLine($"        services.AddTransient<{type.ToDisplayString()}>();");
            // Register all public interfaces
            foreach (var @interface in GetValidTypeSymbols(type.AllInterfaces, assemblyPrefix))
            {
                sourceBuilder.AppendLine($"        services.AddTransient<{@interface.ToDisplayString()}, {type.ToDisplayString()}>();");
            }
        }

        sourceBuilder.AppendLine(@"
        return services;
    }
}");

        context.AddSource("ServiceRegistration.g.cs", sourceBuilder.ToString());
    }

    private static IEnumerable<INamedTypeSymbol> GetValidTypeSymbols(
        ImmutableArray<INamedTypeSymbol> allInterfaces,
        string assemblyPrefix)
    {
        var injectionNamespace = $"{assemblyPrefix}.Core.Injection";

        return allInterfaces
            .Where(symbol =>
                symbol.DeclaredAccessibility == Accessibility.Public &&
                (symbol.ContainingNamespace?.ToDisplayString().StartsWith(assemblyPrefix) ?? false) &&
                symbol.ContainingNamespace?.ToDisplayString() != injectionNamespace);
    }

    private static void ProcessNamespace(
        INamespaceSymbol namespaceSymbol,
        INamedTypeSymbol scopedDependencySymbol,
        INamedTypeSymbol singletonDependencySymbol,
        INamedTypeSymbol transientDependencySymbol,
        List<ITypeSymbol> scopedTypes,
        List<ITypeSymbol> singletonTypes,
        List<ITypeSymbol> transientTypes)
    {
        // Process types in this namespace
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            ProcessType(
                type,
                scopedDependencySymbol,
                singletonDependencySymbol,
                transientDependencySymbol,
                scopedTypes,
                singletonTypes,
                transientTypes);
        }

        // Process nested namespaces
        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            ProcessNamespace(
                nestedNamespace,
                scopedDependencySymbol,
                singletonDependencySymbol,
                transientDependencySymbol,
                scopedTypes,
                singletonTypes,
                transientTypes);
        }
    }

    private static void ProcessType(
        INamedTypeSymbol type,
        INamedTypeSymbol scopedDependencySymbol,
        INamedTypeSymbol singletonDependencySymbol,
        INamedTypeSymbol transientDependencySymbol,
        List<ITypeSymbol> scopedTypes,
        List<ITypeSymbol> singletonTypes,
        List<ITypeSymbol> transientTypes)
    {
        // Skip interfaces and abstract classes
        if (type.TypeKind == TypeKind.Interface || type.IsAbstract)
            return;

        // Process nested types
        foreach (var nestedType in type.GetTypeMembers())
        {
            ProcessType(
                nestedType,
                scopedDependencySymbol,
                singletonDependencySymbol,
                transientDependencySymbol,
                scopedTypes,
                singletonTypes,
                transientTypes);
        }

        // Check if the type implements any of our interfaces
        if (ImplementsInterface(type, scopedDependencySymbol))
            scopedTypes.Add(type);
        else if (ImplementsInterface(type, singletonDependencySymbol))
            singletonTypes.Add(type);
        else if (ImplementsInterface(type, transientDependencySymbol))
            transientTypes.Add(type);
    }

    private static bool ImplementsInterface(ITypeSymbol type, INamedTypeSymbol interfaceSymbol)
    {
        return type.AllInterfaces.Any(namedTypeSymbol => SymbolEqualityComparer.Default.Equals(namedTypeSymbol, interfaceSymbol));
    }
} 