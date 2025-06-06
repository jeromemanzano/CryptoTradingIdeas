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

    private void GenerateSource(SourceProductionContext context, Compilation compilation)
    {
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
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "SG0002",
                    "Registered Service",
                    $"Registered scoped service: {type.ToDisplayString()}",
                    "Generation",
                    DiagnosticSeverity.Info,
                    true),
                Location.None));
        }

        // Register singleton services
        foreach (var type in singletonTypes)
        {
            sourceBuilder.AppendLine($"        services.AddSingleton<{type.ToDisplayString()}>();");
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "SG0002",
                    "Registered Service",
                    $"Registered singleton service: {type.ToDisplayString()}",
                    "Generation",
                    DiagnosticSeverity.Info,
                    true),
                Location.None));
        }

        // Register transient services
        foreach (var type in transientTypes)
        {
            sourceBuilder.AppendLine($"        services.AddTransient<{type.ToDisplayString()}>();");
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "SG0002",
                    "Registered Service",
                    $"Registered transient service: {type.ToDisplayString()}",
                    "Generation",
                    DiagnosticSeverity.Info,
                    true),
                Location.None));
        }

        sourceBuilder.AppendLine(@"
        return services;
    }
}");

        context.AddSource("ServiceRegistration.g.cs", sourceBuilder.ToString());
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
            ProcessType(nestedType, scopedDependencySymbol, singletonDependencySymbol, transientDependencySymbol,
                scopedTypes, singletonTypes, transientTypes);
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