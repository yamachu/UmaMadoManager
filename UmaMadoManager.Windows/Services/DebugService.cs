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

            var allocated = PInvoke.Kernel32.AllocConsole();
            IsAllocated = allocated;
            return allocated;
        }
    }
}
