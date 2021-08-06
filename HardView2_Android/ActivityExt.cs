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
        public static void EnterImmersiveMode(this AndroidX.AppCompat.App.AppCompatActivity activity)
        {
            // See https://developer.android.com/training/system-ui/immersive
            var flags = SystemUiFlags.Immersive | SystemUiFlags.Fullscreen | SystemUiFlags.HideNavigation |
                        SystemUiFlags.LayoutHideNavigation | SystemUiFlags.LayoutStable | SystemUiFlags.LayoutFullscreen;
            activity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(int)flags;
        }
    }
}