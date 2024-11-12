using System.Text;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace GoogleTasksSynchronizer.Configuration
{
    public class GoogleClientSecretProvider(IOptions<GoogleClientSecretOptions> googleClientSecretOptions) : IGoogleClientSecretProvider
    {
        public async Task<ClientSecrets> GetGoogleClientSecrets()
        {
            using var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(googleClientSecretOptions.Value.GoogleClientSecret));

            var googleClientSecrets = await GoogleClientSecrets.FromStreamAsync(memoryStream);

            return googleClientSecrets.Secrets;
        }
    }
}
