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
using Android.Util;

namespace HardView2
{
    public class ImageView : View
    {
        // Programmatic inflation
        public ImageView(Context context) : base(context)
        {
        }


        // Declarative inflation
        public ImageView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }




        public event EventHandler<Android.Graphics.Canvas> DrawEvent;  // Use "Event" suffiX to avoid hiding DraW()

        protected override void OnDraw(Android.Graphics.Canvas canvas)
        {
            if (this.DrawEvent != null)
                this.DrawEvent(this, canvas);
        }
    }
}
