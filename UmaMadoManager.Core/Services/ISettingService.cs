using System;
using UmaMadoManager.Core.Models;

namespace UmaMadoManager.Core.Services
{
    public interface ISettingService
    {
        void Init();
        Settings Instance();
        void Save();
    }
}
