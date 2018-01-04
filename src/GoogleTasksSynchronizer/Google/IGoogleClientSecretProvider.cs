using System;
using System.Collections.Generic;
using System.Text;
using Google.Apis.Auth.OAuth2;

namespace GoogleTasksSynchronizer.Google
{
    public interface IGoogleClientSecretProvider
    {
        ClientSecrets GetGoogleClientSecrets();
    }
}
