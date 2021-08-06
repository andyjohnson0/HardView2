using System;
using System.Linq;
using System.IO;

using Android;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Graphics;
using Android.Content.PM;
using Android.Content;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using static Android.Views.GestureDetector;
using static Android.Views.View;




namespace HardView2
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, IOnTouchListener, IOnGestureListener
    {
        private GestureDetector gestureDetector;
        private const int swipeThresholdVelocity = 200;

        private string[] imageFileTypes = new string[] { ".jpg", ".jpeg", ".png" };
        private FileInfo currentFile;
        private DirectoryInfo currentDirectory;
        private Bitmap currentImage = null;  // Cached image.
        private bool scaleToFit = true;
        private int zoom = 0;
        private System.Drawing.Size currentPan = new System.Drawing.Size(0, 0);   // Accumulated position afer drags
        private System.Drawing.Size currentDrag = new System.Drawing.Size(0, 0);  // Position delta during drag

        // Request codes
        private const int changeDirectoryRc = 1;
        private const int requestExternalStorage = 2;



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            DirectoryInfo di;
            if (savedInstanceState != null)
            {
                di = new DirectoryInfo(savedInstanceState.GetString("DirPath"));
            }
            else if (this.Intent?.Extras != null)
            {
                di = new DirectoryInfo(this.Intent.Extras.GetString("DirPath"));
            }
            else
            {
                // Initial directory.
#if RELEASE
                var dir = Android.OS.Environment.DirectoryPictures;
#else
                var dir = Android.OS.Environment.DirectoryDcim;
#endif
                di = new DirectoryInfo(Android.OS.Environment.GetExternalStoragePublicDirectory(dir).AbsolutePath);
            }

            SetContentView(Resource.Layout.activity_main);
            this.EnterImmersiveMode();

            gestureDetector = new GestureDetector(this);
            var imageView = this.FindViewById<ImageView>(Resource.Id.imageView);
            imageView.SetOnTouchListener(this);
            imageView.DrawEvent += OnDrawImage;

            // 
            SetCurrentDirectory(di);
            ShowPictures();
        }


        protected override void OnResume()
        {
            base.OnResume();

            var flags = SystemUiFlags.Immersive | SystemUiFlags.LayoutStable | SystemUiFlags.LayoutHideNavigation | SystemUiFlags.LayoutFullscreen |
            SystemUiFlags.HideNavigation | SystemUiFlags.Fullscreen;
            this.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(int)flags;
        }



        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            switch (requestCode)
            {
                case requestExternalStorage:
                    if ((grantResults.Length > 0) && grantResults[0] == Permission.Granted)
                    {
                        ShowPictures();
                    }
                    break;
                default:
                    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                    break;
            }
        }


        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case changeDirectoryRc:
                        SetCurrentDirectory(new DirectoryInfo(data.GetStringExtra("DirPath")));
                        break;
                    default:
                        base.OnActivityResult(requestCode, resultCode, data);
                        break;
                }
            }
        }


        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString("DirPath", currentDirectory.FullName);
            base.OnSaveInstanceState(outState);
        }


        protected override void OnRestoreInstanceState(Bundle savedState)
        {
            base.OnRestoreInstanceState(savedState);
            currentDirectory = new DirectoryInfo(savedState.GetString("DirPath"));
        }



        #region Drawing

        private void SetCurrentDirectory(DirectoryInfo di)
        {
            var cd = currentDirectory;
            var cf = currentFile;

            try
            {
                currentDirectory = di;
                currentFile = currentDirectory.First(imageFileTypes);
                RedrawCurrentImage(true);

                var fileCount = currentDirectory.GetFiles(imageFileTypes).Length;
                var prompt = string.Format(Resources.GetString(Resource.String.CurrentDirectoryPrompt),
                                           currentDirectory.FullName,
                                           fileCount,
                                           Resources.GetString(fileCount != 1 ? Resource.String.Images : Resource.String.Image));
                Toast.MakeText(this, prompt, ToastLength.Long).Show();
            }
            catch(Exception)
            {
                var prompt = string.Format(Resources.GetString(Resource.String.DirectoryInaccessiblePrompt),
                                                           currentDirectory.FullName);
                Toast.MakeText(this, prompt, ToastLength.Long).Show();

                currentDirectory = cd;
                currentFile = cf;
            }
        }


        private void ShowPictures()
        {
            // Check permissions.
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != Permission.Granted)
            {
                if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Camera))
                {
                    // Explain to the user why we want the permission.
                    var adb = new Android.App.AlertDialog.Builder(this);
                    adb.SetCancelable(true);
                    adb.SetTitle(Resource.String.ExternalStoragePermissionPrompt);
                    adb.SetMessage(Resource.String.ExternalStoragePermissionMsg);
                    adb.SetPositiveButton(Android.Resource.String.Ok, (sender, args) =>
                    {
                        ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.ReadExternalStorage }, requestExternalStorage);
                    });
                    adb.Create().Show();
                    return;
                }
                else
                {
                    // No need to explain. Just acquire the premission.
                    ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.ReadExternalStorage }, requestExternalStorage);
                    // System calls OnRequestPermissionsResult() with grant/deny.
                    return;
                }
            }

            //
            if (currentFile == null)
                currentFile = currentDirectory.First(imageFileTypes);
            RedrawCurrentImage(true);
        }


        private void RedrawCurrentImage(bool clearCached)
        {
            if (clearCached && (currentImage != null))
            {
                currentImage.Dispose();
                currentImage = null;
            }
            var imageView = this.FindViewById<ImageView>(Resource.Id.imageView);
            imageView.Invalidate();
        }


        private void OnDrawImage(object sender, Canvas canvas)
        {
            if ((currentFile != null) && currentFile.Exists)
            {
                Bitmap img;
                if (currentImage != null)
                {
                    img = currentImage;
                }
                else
                {
                    img = BitmapFactory.DecodeFile(currentFile.FullName);
                    currentImage = img;
                }
                using (var drawImg = scaleToFit ? ScaleImage(img, canvas.Width + zoom, canvas.Height + zoom) : img)
                {
                    var pt = new Point((canvas.Width / 2) - (drawImg.Width / 2) + currentPan.Width + currentDrag.Width,
                                       (canvas.Height / 2) - (drawImg.Height / 2) + currentPan.Height + currentDrag.Height);
                    canvas.DrawBitmap(drawImg, (float)pt.X, (float)pt.Y, (Paint)null);
                }

                //if (displayInfo)
                //{
                //    ShowInfo(img, e.Graphics);
                //}
            }
        }


        private static Bitmap ScaleImage(Bitmap image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            return Bitmap.CreateScaledBitmap(image, newWidth, newHeight, false);
        }

#endregion Drawing



#region Gestures

        private void OnSwipeRight()
        {
            if (currentFile != null)
            {
                currentFile = currentFile.Previous(imageFileTypes);
                RedrawCurrentImage(true);
            }
        }

        private void OnSwipeLeft()
        {
            if (currentFile != null)
            {
                currentFile = currentFile.Next(imageFileTypes);
                RedrawCurrentImage(true);
            }
        }

        private void OnSwipeUp()
        {
            // Show child directory choices
            var intent = new Intent(this, typeof(DirectoryPickerActivity));
            intent.PutExtra("DirPath", currentDirectory.FullName);
            StartActivityForResult(intent, changeDirectoryRc);
        }

        private void OnSwipeDown()
        {
            // Move to parent directory.
            if(currentDirectory?.Parent != null)
            {
                try
                {
                    SetCurrentDirectory(currentDirectory.Parent);
                }
                catch(Exception)
                {
                    // TODO: Failed to cahange directory
                }
            }
            else
            {
                // TODO
            }
        }

        #endregion Gestures


        #region IOnTouchListener

        public bool OnTouch(View v, MotionEvent e)
        {
            return gestureDetector.OnTouchEvent(e);
        }

#endregion IOnTouchListener


#region IOnGestureListener

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            bool result = false;
            float dY = e2.GetY() - e1.GetY();
            float dX = e2.GetX() - e1.GetX();
            if (Math.Abs(dX) > Math.Abs(dY))
            {
                if ((Math.Abs(dX) > swipeThresholdVelocity) && (Math.Abs(velocityX) > swipeThresholdVelocity))
                {
                    if (dX > 0)
                    {
                        OnSwipeRight();    
                    }
                    else
                    {
                        OnSwipeLeft();    
                    }
                    result = true;
                }
            }
            else if ((Math.Abs(dY) > swipeThresholdVelocity) && (Math.Abs(velocityY) > swipeThresholdVelocity))
            {
                if (dY > 0)
                {
                    OnSwipeDown();
                }
                else
                {
                    OnSwipeUp();
                }
                result = true;
            }

            return result;
        }


        private enum ScreenRegionType
        {
            TopLeft = 1,
            TopCentre,
            TopRight,
            MiddleLeft,
            CentreCentre,
            CentreRight,
            BottomLeft,
            BottomCentre,
            BottomRight,
        }

        private ScreenRegionType GetScreenRegion(MotionEvent e)
        {
            var w = (float)this.Window.DecorView.Width;
            var h = (float)this.Window.DecorView.Height;

            int t;
            if (e.GetX() < w * 0.25f)
                t = (int)ScreenRegionType.TopLeft;
            else if (e.GetX() > w * 0.75f)
                t = (int)ScreenRegionType.TopRight;
            else
                t = (int)ScreenRegionType.TopCentre;
            if (e.GetY() < h * 0.25f)
                t += 0;
            else if (e.GetY() > h * 0.75f)
                t += 6;
            else
                t += 3;

            return (ScreenRegionType)t;
        }



        public void OnLongPress(MotionEvent e)
        {
            switch(GetScreenRegion(e))
            {
                case ScreenRegionType.MiddleLeft:
                    currentFile = currentDirectory.First(imageFileTypes);
                    if (currentFile != null)
                    {
                        RedrawCurrentImage(true);
                        Toast.MakeText(this, Resource.String.MovedToFirstPrompt, ToastLength.Short).Show();
                    }
                    break;
                case ScreenRegionType.CentreRight:
                    currentFile = currentDirectory.Last(imageFileTypes);
                    if (currentFile != null)
                    {
                        RedrawCurrentImage(true);
                        Toast.MakeText(this, Resource.String.MovedToLastPrompt, ToastLength.Short).Show();
                    }
                    break;
                case ScreenRegionType.CentreCentre:
                    currentFile = currentDirectory.Random(imageFileTypes);
                    if (currentFile != null)
                    {
                        RedrawCurrentImage(true);
                    }
                    break;
            }
        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            switch(GetScreenRegion(e))
            {
                case ScreenRegionType.MiddleLeft:
                    if (currentFile != null)
                    {
                        currentFile = currentFile.Previous(imageFileTypes);
                        RedrawCurrentImage(true);
                    }
                    break;
                case ScreenRegionType.CentreRight:
                    if (currentFile != null)
                    {
                        currentFile = currentFile.Next(imageFileTypes);
                        RedrawCurrentImage(true);
                    }
                    break;
            }

            return true;
        }



        public bool OnDown(MotionEvent e)
        {
            return true;
        }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            return true;
        }

        public void OnShowPress(MotionEvent e)
        {
        }

        #endregion IOnGestureListener
    }
}
