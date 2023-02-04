using System;

using Android.App;
using Android.Views;


namespace uk.andyjohnson.HardView2
{
    /// <summary>
    /// Extensions to Android.App.Activity class.
    /// </summary>
    public static class ActivityExt
    {
        /// <summary>
        /// Enter immersive mode, hiding title bar, task bar, etc.
        /// </summary>
        public static void EnterImmersiveMode(this Activity self)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            // See https://developer.android.com/training/system-ui/immersive
            var flags = SystemUiFlags.Immersive | SystemUiFlags.Fullscreen | SystemUiFlags.HideNavigation |
                        SystemUiFlags.LayoutFullscreen | SystemUiFlags.LayoutHideNavigation | SystemUiFlags.LayoutStable;
            self.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(int)flags;
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
