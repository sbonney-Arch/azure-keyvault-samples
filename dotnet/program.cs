using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace KeyVaultSecretDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Replace with your tenant ID, client ID, client secret, and Key Vault URL
            var tenantId = "<your-tenant-id>";
            var clientId = "<your-client-id>";
            var clientSecret = "<your-client-secret>";
            var keyVaultUrl = "https://<key-vault-name>.privatelink.vaultcore.azure.net";

            // Create a client secret credential
            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            // Create HttpClientHandler with custom SSL validation
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                Console.WriteLine($"SSL Policy Errors: {sslPolicyErrors}");
                Console.WriteLine($"Certificate Subject: {cert.Subject}");
                Console.WriteLine($"Certificate Issuer: {cert.Issuer}");

                if (sslPolicyErrors == SslPolicyErrors.None)
                    return true;

                // Allow if certificate is trusted by the chain
                return chain.Build(cert);
            };

            // Create HttpClient using the handler
            var httpClient = new HttpClient(handler);

            // Create a custom HttpPipelineTransport
            var httpClientTransport = new HttpClientTransport(httpClient);

            // Create a SecretClient using the HttpClient and ClientSecretCredential
            var clientOptions = new SecretClientOptions
            {
                Transport = httpClientTransport,
                DisableChallengeResourceVerification = true, // Disable challenge resource verification for private endpoints
                Retry =
                {
                    Delay = TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 5,
                    Mode = RetryMode.Exponential
                }
            };
            var client = new SecretClient(new Uri(keyVaultUrl), clientSecretCredential, clientOptions);

            // Name and value of the secret to be created and retrieved
            var secretName = "MySecret";
            var secretValue = "This is a test secret";

            // Set expiration date to 2 years from now
            var expirationDate = DateTimeOffset.UtcNow.AddYears(2);

            try
            {
                // Create a new secret with expiration date
                KeyVaultSecret secret = new KeyVaultSecret(secretName, secretValue)
                {
                    Properties = { ExpiresOn = expirationDate }
                };
                await client.SetSecretAsync(secret);
                Console.WriteLine($"Secret '{secretName}' created successfully with expiration date {expirationDate}.");

                // Retrieve the secret
                KeyVaultSecret retrievedSecret = await client.GetSecretAsync(secretName);
                Console.WriteLine($"Retrieved Secret Value: {retrievedSecret.Value}");
                Console.WriteLine($"Secret Expiration Date: {retrievedSecret.Properties.ExpiresOn}");
            }
            catch (Azure.RequestFailedException ex)
            {
                Console.WriteLine($"Azure Request Failed: {ex.Message}");
                Console.WriteLine($"Status: {ex.Status}");
                Console.WriteLine($"Error Code: {ex.ErrorCode}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
        }
    }
}
