﻿using Google.Apis.Auth.OAuth2;

namespace GoogleTasksSynchronizer.Configuration
{
    public interface IGoogleClientSecretProvider
    {
        Task<ClientSecrets> GetGoogleClientSecrets();
    }
}
