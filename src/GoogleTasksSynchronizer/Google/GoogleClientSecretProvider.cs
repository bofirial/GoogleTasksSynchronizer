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
            string googleClientSecretJson = Environment.GetEnvironmentVariable("GoogleClientSecret");

            Stream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(googleClientSecretJson));

            return GoogleClientSecrets.Load(memoryStream).Secrets;
        }
    }
}