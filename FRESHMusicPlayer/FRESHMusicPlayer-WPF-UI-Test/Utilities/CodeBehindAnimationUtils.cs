using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace FRESHMusicPlayer.Utilities
{
    class CodeBehindAnimationUtils
    {
        public static Storyboard WidthAnimation(TimeSpan length, double To, double From)
        {
            Storyboard sb = new Storyboard();
            DoubleAnimation doubleAnimation = new DoubleAnimation(From, To, length);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Width"));
            return sb;
        }
    }
}
