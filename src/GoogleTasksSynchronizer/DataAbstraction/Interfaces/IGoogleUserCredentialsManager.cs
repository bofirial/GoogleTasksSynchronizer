using GoogleTasksSynchronizer.DataAbstraction.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.DataAbstraction.Interfaces
{
    public interface IGoogleUserCredentialsManager
    {
        Task<GoogleUserCredentials> SelectAsync();

        Task UpdateAsync(GoogleUserCredentials googleUserCredentials);
    }
}
