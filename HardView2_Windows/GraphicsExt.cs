using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography.X509Certificates;

namespace uk.andyjohnson.HardView2
{
    public static class GraphicsExt
    {
        public static (SizeF, float) MeasureStrings(
            this Graphics grp,
            string[] strings,
            Font fnt,
            float lineSpacing)
        {
            var measuredSize = new SizeF(0F, 0F);
            var maxLineHeight = 0F;
            foreach (var str in strings)
            {
                var s = grp.MeasureString(str, fnt);
                measuredSize.Width = Math.Max(s.Width, measuredSize.Width);
                measuredSize.Height += s.Height;
                maxLineHeight = Math.Max(s.Height, maxLineHeight);
            }
            measuredSize.Height += Math.Max((strings.Length - 1) * lineSpacing, 0F);
            return (measuredSize, maxLineHeight);
        }
    }


    public static class ImageExt
    {
        public static DateTime? GetDateTimeTaken(this Image img)
        {
            var piStr = img.GetPropertyItemString(0x9003);
            if (piStr == null)
                piStr = img.GetPropertyItemString(0x0132);
            if (piStr != null)
            {
                if (DateTime.TryParseExact(piStr, "yyyy:MM:dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture,
                                           System.Globalization.DateTimeStyles.AssumeLocal, out var ts))
                {
                    return ts;
                }
            }

            return null;
        }


        public static string GetDeviceDescription(this Image img)
        {
            var piMakeStr = img.GetPropertyItemString(0x010F);
            var piModelStr = img.GetPropertyItemString(0x0110);
            if ((piMakeStr != null) && (piModelStr != null))
                return string.Format("{0} {1}", piMakeStr, piModelStr);
            else
                return null;
        }

        public static ushort? GetIsoSpeed(this Image img)
        {
            return img.GetPropertyItemShort(0x8827);
        }

        public static double? GetShutterSpeed(this Image img)
        {
            var x = img.GetPropertyItemLong2(0x9201);
            if (x.HasValue)
                // convert from APEX to seconds.
                // See http://www.yqcomputer.com/36_2389_1.htm for details of this calculation.
                return (double?)(1D / Math.Pow(2D, (double)x.Value.Item1 / (double)x.Value.Item2));
            else
                return null;
        }

        public static double? GetFStop(this Image img)
        {
            var x = img.GetPropertyItemLong2(0x829D);
            if (x.HasValue)
                return (double)x.Value.Item1 / (double)x.Value.Item2;
            else
                return null;
        }


        public static double? GetFocalLength(this Image img)
        {
            var x = img.GetPropertyItemLong2(0x920A);
            if (x.HasValue)
                return (double)x.Value.Item1 / (double)x.Value.Item2;
            else
                return null;
        }


        public static ushort? GetMeteringMode(this Image img)
        {
            return img.GetPropertyItemShort(0x9207);
        }


        public static ushort? GetFlashMode(this Image img)
        {
            return img.GetPropertyItemShort(0x9209);
        }


        public static ushort? GetWhiteBalance(this Image img)
        {
            return img.GetPropertyItemShort(0xA403);
        }



        public static string GetPropertyItemString(this Image img, int propId)
        {
            try
            {
                var pi = img.GetPropertyItem(propId);
                if (pi != null)
                    return Encoding.ASCII.GetString(pi.Value, 0, pi.Len - 1);
            }
            catch (ArgumentException) { }

            return null;
        }


        public static ushort? GetPropertyItemShort(this Image img, int propId)
        {
            try
            {
                var pi = img.GetPropertyItem(propId);
                if (pi != null)
                    return BitConverter.ToUInt16(pi.Value, 0);
            }
            catch (ArgumentException) { }

            return null;
        }


        public static (ulong, ulong)? GetPropertyItemLong2(this Image img, int propId)
        {
            try
            {
                var pi = img.GetPropertyItem(propId);
                if (pi != null)
                    return ( BitConverter.ToUInt32(pi.Value, 0), BitConverter.ToUInt32(pi.Value, 4) );
            }
            catch (ArgumentException) { }

            return null;
        }

    }
}
