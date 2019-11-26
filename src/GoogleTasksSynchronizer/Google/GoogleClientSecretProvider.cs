using System;
using System.IO;
using System.Text;
using Google.Apis.Auth.OAuth2;

namespace GoogleTasksSynchronizer.Google
{
    public class GoogleClientSecretProvider : IGoogleClientSecretProvider
    {
        public ClientSecrets GetGoogleClientSecrets()
        {
            var googleClientSecretJson = Environment.GetEnvironmentVariable("GoogleClientSecret");

            var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(googleClientSecretJson));

            return GoogleClientSecrets.Load(memoryStream).Secrets;
        }
    }
}