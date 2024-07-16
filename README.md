# Azure Key Vault Samples

This repository contains sample code for interacting with Azure Key Vault using .NET and PowerShell.

## .NET Sample

The .NET sample demonstrates how to create and retrieve secrets from Azure Key Vault.

### Running the .NET Sample

1. Install required NuGet packages:
     dotnet add package Azure.Identity__
     dotnet add package Azure.Security.KeyVault.Secrets__
3. Update `Program.cs` with your Azure Key Vault details.
4. Build and run the application:
   ```sh
   dotnet run

### Running the Powershell Sample

1. Install required modules
      Install-Module -Name Az -AllowClobber -Force__
2. Update 'Sample-AzKeyVault' with your Azure Key Vault details
3. Run the Powershell script
