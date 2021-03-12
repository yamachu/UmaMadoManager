using System;
using System.Collections.Generic;
using UmaMadoManager.Core.Models;

namespace UmaMadoManager.Core.Services
{
    public interface IScreenManager
    {
        IEnumerable<Screen> GetScreens();
    }
}
