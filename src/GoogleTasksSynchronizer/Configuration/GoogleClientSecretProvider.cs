using System;
using System.IO;
using System.Text;
using Google.Apis.Auth.OAuth2;

namespace GoogleTasksSynchronizer.Configuration
{
    public class GoogleClientSecretProvider : IGoogleClientSecretProvider
    {
        public ClientSecrets GetGoogleClientSecrets()
        {
            //TODO: Consider switching this to use Configuration
            var googleClientSecretJson = Environment.GetEnvironmentVariable("GoogleClientSecret");

            using var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(googleClientSecretJson));

            return GoogleClientSecrets.Load(memoryStream).Secrets;
        }
    }
}