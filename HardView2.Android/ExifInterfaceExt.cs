using System;

using Android.Media;


namespace uk.andyjohnson.HardView2
{
    /// <summary>
    /// Extensions to Android.Media.ExifInterface class.
    /// </summary>
    public static class ExifInterfaceExt
    {
        /// <summary>
        /// Get image time taken.
        /// Checks TagDateTime, falling back on TagDatetimeOriginal
        /// </summary>
        /// <returns>DateTime containing local time that image was taken, or null if the time could not be determined.</returns>
        public static DateTime? GetDateTimeTaken(this ExifInterface self)
        {
            var propStr = self.GetPropertyItemString(ExifInterface.TagDatetime);
            if (propStr == null)
                propStr = propStr = self.GetPropertyItemString(ExifInterface.TagDatetimeOriginal);
            if (propStr != null)
            {
                if (DateTime.TryParseExact(propStr, "yyyy:MM:dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture,
                                           System.Globalization.DateTimeStyles.AssumeLocal, out var ts))
                {
                    return ts;
                }
            }

            return null;
        }


        /// <summary>
        /// Get device description.
        /// </summary>
        /// <returns>Device description (e.g. "motorola edge 20") or null if the device could not be determined.</returns>
        public static string GetDeviceDescription(this ExifInterface self)
        {
            var propMakeStr = self.GetPropertyItemString(ExifInterface.TagMake);
            var propModelStr = self.GetPropertyItemString(ExifInterface.TagModel);

            if (propMakeStr != null && propModelStr != null)
            {
                return !propModelStr.StartsWith(propMakeStr, StringComparison.InvariantCultureIgnoreCase) ? propMakeStr + " " + propModelStr : propModelStr;
            }
            else if (propMakeStr != null || propModelStr != null)
            {
                return propMakeStr != null ? propMakeStr : propModelStr;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Get the image capture speed in ISO units - 200, 400, 800 etc
        /// </summary>
        /// <returns>Speed in ISO units, or null if the speed could not be determined.</returns>
        public static ushort? GetIsoSpeed(this ExifInterface self)
        {
            return self.GetPropertyItemShort(ExifInterface.TagIsoSpeedRatings);
        }


        /// <summary>
        /// Get shutter speed
        /// </summary>
        /// <returns>Shutter speed as a fraction of a second, or null if the shutter speed could not be determined.</returns>
        public static double? GetShutterSpeed(this ExifInterface self)
        {
            var propVal = self.GetPropertyItemLong2(ExifInterface.TagShutterSpeedValue);
            if (propVal.HasValue)
                // convert from APEX to seconds.
                // See http://www.yqcomputer.com/36_2389_1.htm for details of this calculation.
                return (double?)(1D / Math.Pow(2D, (double)propVal.Value.Item1 / (double)propVal.Value.Item2));
            else
                return null;
        }


        /// <summary>
        /// Get the f-stop.
        /// </summary>
        /// <returns>F-stop value or null if the f-stop could not be determined.</returns>
        public static double? GetFStop(this ExifInterface self)
        {
            var propVal = self.GetPropertyItemLong2(ExifInterface.TagApertureValue);
            if (propVal.HasValue)
                return (double)propVal.Value.Item1 / (double)propVal.Value.Item2;
            else
                return null;
        }


        /// <summary>
        /// Get focal length
        /// </summary>
        /// <returns>Focal length in mm, or null if the focal length could not be determined.</returns>
        public static double? GetFocalLength(this ExifInterface self)
        {
            var propVal = self.GetPropertyItemLong2(ExifInterface.TagFocalLength);
            if (propVal.HasValue)
                return (double)propVal.Value.Item1 / (double)propVal.Value.Item2;
            else
                return null;
        }


        /// <summary>
        /// Get metering mode.
        /// </summary>
        /// <returns>Metering mode, or null if the metering mode could not be determined</returns>
        public static ushort? GetMeteringMode(this ExifInterface self)
        {
            return self.GetPropertyItemShort(ExifInterface.TagMeteringMode);
        }


        /// <summary>
        /// Get flash mode.
        /// </summary>
        /// <returns>Flash mode, or null if the flash mode could not be determined</returns>
        public static ushort? GetFlashMode(this ExifInterface self)
        {
            return self.GetPropertyItemShort(ExifInterface.TagFlash);
        }


        /// <summary>
        /// Get white balance.
        /// </summary>
        /// <returns>White balance, or null if the white balance could not be determined</returns>
        public static ushort? GetWhiteBalance(this ExifInterface self)
        {
            return self.GetPropertyItemShort(ExifInterface.TagWhiteBalance);
        }



        #region Implementation

        private static string GetPropertyItemString(this ExifInterface self, string propId)
        {
            try
            {
                var propBytes = self.GetAttributeBytes(propId);
                if (propBytes != null)
                    return System.Text.Encoding.ASCII.GetString(propBytes, 0, propBytes.Length - 1);
            }
            catch (ArgumentException) { }

            return null;
        }


        private static ushort? GetPropertyItemShort(this ExifInterface self, string propId)
        {
            try
            {
                var propBytes = self.GetAttributeBytes(propId);
                if (propBytes != null)
                    return BitConverter.ToUInt16(propBytes, 0);
            }
            catch (ArgumentException) { }

            return null;
        }


        public static (uint, uint)? GetPropertyItemLong2(this ExifInterface self, string propId)
        {
            try
            {
                var propBytes = self.GetAttributeBytes(propId);
                if (propBytes != null)
                    return (BitConverter.ToUInt32(propBytes, 0), BitConverter.ToUInt32(propBytes, 4));
            }
            catch (ArgumentException) { }

            return null;
        }

        #endregion Implementation
    }
}
