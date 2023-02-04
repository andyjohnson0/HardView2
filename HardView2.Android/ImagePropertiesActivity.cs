using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Android.App;
using Android.Graphics;
using Android.Media;
using Android.OS;

using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;


namespace uk.andyjohnson.HardView2
{
    /// <summary>
    /// Browse images image properties using a recycler view.
    /// </summary>
    [Activity(Label = "@string/ImageProperties", Theme = "@style/AppDialogTheme")]
    public class ImagePropertiesActivity : AppCompatActivity
    {
        public const string ImageFileUriKey = "ImageFileUri";  // in: image file uri


        private Android.Net.Uri imageFileUri;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_imageProperties);

            // Restore state
            var restoreFromBundle = (savedInstanceState != null) ? savedInstanceState : this.Intent?.Extras;
            if (restoreFromBundle != null)
            {
                imageFileUri = Android.Net.Uri.Parse(restoreFromBundle.GetString(ImageFileUriKey));
            }

            var propsRecView = FindViewById<RecyclerView>(Resource.Id.imagePropsRecView);
            propsRecView.SetLayoutManager(new GridLayoutManager(this, 2));
            propsRecView.SetAdapter(new ImagePropertiesAdapter(this, GetImageProperties()));
        }


        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(ImageFileUriKey, imageFileUri.ToString());

            base.OnSaveInstanceState(outState);
        }


        protected override void OnRestoreInstanceState(Bundle savedState)
        {
            base.OnRestoreInstanceState(savedState);

            imageFileUri = Android.Net.Uri.Parse(savedState.GetString(ImageFileUriKey));
        }


        /// <summary>
        /// Build the image properties key/value list
        /// </summary>
        /// <returns></returns>
        private IList<KeyValuePair<string, string>> GetImageProperties()
        {
            var props = new List<KeyValuePair<string, string>>();

            // File properties
            var docInfo = DocumentInfo.Create(this, imageFileUri);
            props.Add("File name", docInfo.DisplayName);
            props.Add("File length", $"{docInfo.DisplayLength} ({docInfo.Length} bytes)");

            // Image properties
            using(var imageFileStm = this.ContentResolver.OpenInputStream(imageFileUri))
            {
                var options = new BitmapFactory.Options() { InJustDecodeBounds = true };
                BitmapFactory.DecodeStream(imageFileStm, null, options);
                props.Add("Resolution", $"{options.OutWidth}x{options.OutHeight}");
            }

            // EXIF properties
            using(var imageFileStm = this.ContentResolver.OpenInputStream(imageFileUri))
            {
                var exif = new ExifInterface(imageFileStm);

                var ts = exif.GetDateTimeTaken();
                if (ts.HasValue)
                    props.Add("Taken", ts.Value.ToString("dd/MM/yyyy HH:mm:ss"));
                var st = exif.GetDeviceDescription();
                if (st != null)
                    props.Add("Device", st);
                var sh = exif.GetIsoSpeed();
                if (sh.HasValue)
                    props.Add("Speed", $"{sh.Value} ISO");
                var db = exif.GetShutterSpeed();
                if (db.HasValue)
                    props.Add("Shutter", $"{ToFractionStr(db.Value)} seconds");
                db = exif.GetFStop();
                if (db.HasValue)
                    props.Add("F-Stop", db.Value.ToString());
                db = exif.GetFocalLength();
                if (db.HasValue)
                    props.Add("Focal length", $"{Math.Ceiling(db.Value)} mm");
                sh = exif.GetMeteringMode();
                if (sh.HasValue)
                    props.Add("Metering mode", MeteringModeTostring(sh.Value));
                sh = exif.GetFlashMode();
                if (sh.HasValue)
                    props.Add("Flash mode", FlashModeTostring(sh.Value));
                sh = exif.GetWhiteBalance();
                if (sh.HasValue)
                    props.Add("White balance", WhiteBalanceToString(sh.Value));
            }

            return props;
        }


        private static string MeteringModeTostring(ushort mode)
        {
            switch (mode)
            {
                case 0:
                    return "Unknown";
                case 1:
                    return "Average";
                case 2:
                    return "Center Weighted Average";
                case 3:
                    return "Spot";
                case 4:
                    return "Multi Spot";
                case 5:
                    return "Pattern";
                case 6:
                    return "Partial";
                case 255:
                    return "Other";
                default:
                    return "Unknown";
            }
        }


        private static string FlashModeTostring(ushort mode)
        {
            switch (mode)
            {
                case 0x0: return "No Flash";
                case 0x1: return "Fired";
                case 0x5: return "Fired, return not detected";
                case 0x7: return "Fired, return detected";
                case 0x8: return "On, did not fire";
                case 0x9: return "On, fired";
                case 0xd: return "On, return not detected";
                case 0xf: return "On, return detected";
                case 0x10: return "Off, did not fire";
                case 0x14: return "Off, did not fire, return not detected";
                case 0x18: return "Auto, did not fire";
                case 0x19: return "Auto, fired";
                case 0x1d: return "Auto, fired, return not detected";
                case 0x1f: return "Auto, fired, return detected";
                case 0x20: return "No flash function";
                case 0x30: return "Off, no flash function";
                case 0x41: return "Fired, red-eye reduction";
                case 0x45: return "Fired, red-eye reduction, return not detected";
                case 0x47: return "Fired, red-eye reduction, return detected";
                case 0x49: return "On, red-eye reduction";
                case 0x4d: return "On, red-eye reduction, return not detected";
                case 0x4f: return "On, red-eye reduction, return detected";
                case 0x50: return "Off, red-eye reduction";
                case 0x58: return "Auto, did not fire, red-eye reduction";
                case 0x59: return "Auto, fired, red-eye reduction";
                case 0x5d: return "Auto, fired, red-eye reduction, return not detected";
                case 0x5f: return "Auto, fired, red-eye reduction, return detected";
                default: return "Unknown";
            }
        }


        private static string WhiteBalanceToString(ushort mode)
        {
            switch (mode)
            {
                case 0: return "Auto";
                case 1: return "Manual";
                default: return "Unknown";
            }
        }


        // Convert a double to a numerator/denominator string
        // Copied from https://stackoverflow.com/a/41434490
        private static string ToFractionStr(double x)
        {
            if (x < 0)
            {
                return "-" + ToFractionStr(-x);
            }
            double tolerance = 1.0E-3;
            double h1 = 1; double h2 = 0;
            double k1 = 0; double k2 = 1;
            double b = x;
            do
            {
                double a = Math.Floor(b);
                double aux = h1; h1 = a * h1 + h2; h2 = aux;
                aux = k1; k1 = a * k1 + k2; k2 = aux;
                b = 1 / (b - a);
            } while (Math.Abs(x - h1 / k1) > x * tolerance);

            return h1 + "/" + k1;
        }
    }
}