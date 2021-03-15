using System;
using UmaMadoManager.Core.Models;
using UmaMadoManager.Core.Services;
using UmaMadoManager.Core.Extensions;

namespace UmaMadoManager.Windows.Services
{
    public class SettingService : ISettingService
    {
        private Settings settings;
        public void Init()
        {
            if (settings != null) {
                return;
            }
            settings = new Settings().Also(s => {
                s.Reload();
                if (!s.IsUpgrated) {
                    s.Upgrade();
                    s.IsUpgrated = true;
                    s.Save();
                }
            });
        }

        public Settings Instance()
        {
            return settings;
        }

        public void Save()
        {
            settings.Save();
        }
    }
}
