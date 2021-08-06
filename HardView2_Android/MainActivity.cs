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



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            //this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            var decorView = this.Window.DecorView;
            var flags = SystemUiFlags.Immersive | SystemUiFlags.LayoutStable | SystemUiFlags.LayoutHideNavigation | SystemUiFlags.LayoutFullscreen |
                        SystemUiFlags.HideNavigation | SystemUiFlags.Fullscreen;
            decorView.SystemUiVisibility = (StatusBarVisibility)(int)flags;

            gestureDetector = new GestureDetector(this);
            var imageView = this.FindViewById<ImageView>(Resource.Id.imageView);
            imageView.SetOnTouchListener(this);
            imageView.DrawEvent += OnDrawImage;

            ShowPictures();
        }


        private const int requestExternalStorage = 1;

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


            currentDirectory = new DirectoryInfo(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim /*DirectoryPictures*/).AbsolutePath + "/Camera");
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
            //var paint = new Paint()
            //{
            //    Color = Color.White,
            //    StrokeWidth = 3.0f
            //};
            //canvas.DrawLine(0f, 0f, (float)canvas.Width, (float)canvas.Height, paint);
            //canvas.DrawLine((float)canvas.Width, 0f, 0f, (float)canvas.Height, paint);

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

        }

        private void OnSwipeDown()
        {

        }

        #endregion Gestures


        #region IOnTouchListener

        public bool OnTouch(View v, MotionEvent e)
        {
            return gestureDetector.OnTouchEvent(e);
        }

        #endregion IOnTouchListener


        #region IOnGestureListener

        public bool OnDown(MotionEvent e)
        {
            return true;
        }

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

        public void OnLongPress(MotionEvent e)
        {
        }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            return true;
        }

        public void OnShowPress(MotionEvent e)
        {
        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            return true;
        }

        #endregion IOnGestureListener
    }
}
