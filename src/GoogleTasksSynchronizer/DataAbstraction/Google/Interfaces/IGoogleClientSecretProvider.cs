using Google.Apis.Auth.OAuth2;

namespace GoogleTasksSynchronizer.DataAbstraction.Google
{
    public interface IGoogleClientSecretProvider
    {
        ClientSecrets GetGoogleClientSecrets();
    }
}
