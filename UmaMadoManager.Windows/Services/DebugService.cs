using System;
using UmaMadoManager.Core.Services;

namespace UmaMadoManager.Windows.Services
{
    public class DebugService : IDebugService
    {
        private bool IsAllocated = false;
        public bool AllocConsole()
        {
            if (IsAllocated) {
                return true;
            };

            var allocated = Native.Win32API.AllocConsole() == 1;
            IsAllocated = allocated;
            return allocated;
        }
    }
}
