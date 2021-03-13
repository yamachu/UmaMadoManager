using System;
using System.Threading.Tasks;

namespace UmaMadoManager.Core.Services
{
    public interface IVersionRepository
    {
        Task<string> GetLatestVersion();
    }
}
