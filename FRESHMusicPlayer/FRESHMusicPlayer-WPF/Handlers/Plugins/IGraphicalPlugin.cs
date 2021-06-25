using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FRESHMusicPlayer.Handlers.Plugins
{
    public interface IGraphicalPlugin : IPlugin
    {
        bool HasControlsMenuItem { get; set; }
        string ControlsMenuItemName { get; set; }
        Page GetControlsMenuItem();
    }
}
