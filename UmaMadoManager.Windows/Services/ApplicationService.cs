using System;
using System.Windows;
using UmaMadoManager.Core.Services;

namespace UmaMadoManager.Windows.Services
{
    public class ApplicationService : IApplicationService
    {
        public void Shutdown()
        {
            Application.Current.Shutdown();
        }
    }
}
