using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace HardView2
{
    public static class ActivityExt
    {
        public static void EnterFullScreen(this AndroidX.AppCompat.App.AppCompatActivity activity)
        {
            var flags = SystemUiFlags.Immersive | SystemUiFlags.LayoutStable | SystemUiFlags.LayoutHideNavigation | SystemUiFlags.LayoutFullscreen |
            SystemUiFlags.HideNavigation | SystemUiFlags.Fullscreen;
            activity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(int)flags;
        }
    }
}