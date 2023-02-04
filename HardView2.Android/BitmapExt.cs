using System;

using Android.Graphics;



namespace uk.andyjohnson.HardView2
{
    /// <summary>
    /// Extensions to Android.Graphics.Bitmap class.
    /// </summary>
    public static class BitmapExt
    {
        /// <summary>
        /// Resize a bitmap to fit within max width or height, preserving aspect ratio.
        /// </summary>
        /// <param name="maxWidthPx">Maximum width in pixels</param>
        /// <param name="maxHeightPx">Maximum height in pixels</param>
        /// <returns>Resized bitmap</returns>
        public static Bitmap Resize(
            this Bitmap self,
            int maxWidthPx,
            int maxHeightPx)
        {
            var ratioX = (double)maxWidthPx / self.Width;
            var ratioY = (double)maxHeightPx / self.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidthPx = (int)(self.Width * ratio);
            var newHeightPx = (int)(self.Height * ratio);

            return Bitmap.CreateScaledBitmap(self, newWidthPx, newHeightPx, false);
        }
    }
}
