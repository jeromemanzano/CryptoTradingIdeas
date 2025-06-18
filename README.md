# CryptoTradingIdeas

## Overview

This app is my personal lab for testing out crypto trading ideas — a quick and lightweight way to bring concepts to life without diving into full-scale development. I’ve had plenty of trading ideas over time, mostly centered on taking advantage of market inefficiencies, but never had a solid way to explore them… until now. The goal is simple: validate ideas fast, figure out what has potential, and only then consider building a more polished version.

## Ideas
### 1. Triangular Arbitrage
Triangular arbitrage in is a strategy that takes advantage of price differences between three crypto trading pairs by converting one coin to another, then a third, and back to the original to secure a risk-free profit. It relies on inefficiencies across exchange rates.

## Technology Stack

- **Framework**: .NET 9
- **UI Framework**: Avalonia UI
- **Trading Library**: [CryptoClients.Net](https://github.com/JKorf/CryptoClients.Net)
- **Key Libraries**:
  - DynamicData - For reactive collections and caching
  - ReactiveUI - For reactive programming patterns
  - Serilog - For structured logging
  - Splat - For dependency injection and service location

## Prerequisites

- .NET 9 SDK
- macOS (currently tested only on macOS)

## Setup Instructions

### macOS Setup

1. Clone the repository
2. Navigate to the `Scripts` folder under the project directory
3. Run the setup script:
   ```bash
   ./macos-setup.sh
   ```

## Running the Application

### Self-Contained Executable
To create a self-contained executable:
```bash
dotnet publish CryptoTradingIdeas/CryptoTradingIdeas.csproj -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --self-contained true -c Release
```

The published executable will be located in the `CryptoTradingIdeas/Output/Publish/Release` directory.

Successful trades will be logged to `CryptoTradingIdeas/Output/Publish/Release/logs` directory.

## Disclaimer

This software is for educational and research purposes only. Use at your own risk. Cryptocurrency trading involves significant risk and may not be suitable for all investors. 
