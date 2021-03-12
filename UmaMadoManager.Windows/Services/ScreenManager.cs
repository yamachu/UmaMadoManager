using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;
using UmaMadoManager.Core.Services;

namespace UmaMadoManager.Windows.Services
{
    public class ScreenManager : IScreenManager
    {

        public IEnumerable<Core.Models.Screen> GetScreens()
        {
            return Screen.AllScreens.Select(s =>
            {
                return new Core.Models.Screen
                {
                    Bounds = s.Bounds,
                    WorkingArea = s.WorkingArea,
                };
            });
        }
    }
}
