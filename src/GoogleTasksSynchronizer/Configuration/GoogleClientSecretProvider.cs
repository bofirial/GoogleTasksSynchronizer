using System.IO;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace GoogleTasksSynchronizer.Configuration
{
    public class GoogleClientSecretProvider : IGoogleClientSecretProvider
    {
        private readonly IOptions<GoogleClientSecretOptions> _googleClientSecretOptions;

        public GoogleClientSecretProvider(IOptions<GoogleClientSecretOptions> googleClientSecretOptions)
        {
            _googleClientSecretOptions = googleClientSecretOptions;
        }

        public ClientSecrets GetGoogleClientSecrets()
        {
            using var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(_googleClientSecretOptions.Value.GoogleClientSecret));

            return GoogleClientSecrets.Load(memoryStream).Secrets;
        }
    }
}