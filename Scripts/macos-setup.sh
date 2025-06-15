#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
NC='\033[0m' # No Color

echo -e "${GREEN}Starting .NET 9 installation...${NC}"

# Detect architecture
ARCH=$(uname -m)
if [ "$ARCH" = "arm64" ]; then
    echo "Detected Apple Silicon (ARM64) architecture"
    ARCH_FLAG="--architecture arm64"
else
    echo "Detected Intel (x64) architecture"
    ARCH_FLAG="--architecture x64"
fi

# Download .NET 9 SDK
echo "Downloading .NET 9 SDK..."
curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --version 9.0.300 $ARCH_FLAG

# Add .NET to PATH if not already present
if [[ ":$PATH:" != *":$HOME/.dotnet:"* ]]; then
    echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.zshrc
    source ~/.zshrc
fi

# Verify installation
echo -e "${GREEN}Verifying .NET installation...${NC}"
dotnet --version

echo -e "${GREEN}.NET 9 installation completed!${NC}" 