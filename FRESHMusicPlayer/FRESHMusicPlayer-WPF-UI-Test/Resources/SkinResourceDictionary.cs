using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FRESHMusicPlayer
{
    
    public class SkinResourceDictionary : ResourceDictionary
    {

        private Uri _darkSource;

        private Uri _lightSource;

        public Uri DarkSource
        {
            get { return _darkSource; }
            set
            {
                _darkSource = value;
                UpdateSource();
            }
        }
        public Uri LightSource
        {
            get { return _lightSource; }
            set
            {
                _lightSource = value;
                UpdateSource();
            }
        }

        public void UpdateSource()
        {
            var val = App.CurrentSkin == Skin.Dark ? DarkSource : LightSource;
            if (val != null && base.Source != val)
                base.Source = val;
        }
    }
}
