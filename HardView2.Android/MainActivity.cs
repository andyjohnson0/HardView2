using System;
using System.Linq;

using Android;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Runtime;
using Android.Views;
using Android.Graphics;
using Android.Content.PM;
using Android.Content;
using Android.Widget;
using Android.Views.Animations;

using AndroidX.Core.App;
using AndroidX.Core;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Android.Provider;
using System.Runtime.Remoting.Contexts;


namespace uk.andyjohnson.HardView2
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, Android.Views.View.IOnTouchListener
    {
        public MainActivity()
        {            
        }


        // File types
        private readonly string[] imageFileTypes = new string[] { ".jpg", ".jpeg", ".png" };

        // Browsing state.
        // Note that we store the current directory separately from the current file because, even though the current directory
        // should be derivable from the current file's ParentFile property, this file->dir mapping is not reliable in practice.
        private DocumentCollection currentDirFiles = null;
        private int currentFilePosition = DocumentCollection.NoPosition;

        //
        private GestureDecoder gestureDecoder;

        // Request codes
        private const int reqActPickDocTree = 1;
        private const int reqActImageBrowse = 2;
        private const int reqActImageProps = 3;

        //
        public const string CurrentDirUriKey = "CurrentDirUri";
        public const string CurrentFilePositionKey = "CurrentFilePosition";


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // 
            SetContentView(Resource.Layout.activity_main);
            this.EnterImmersiveMode();

            // Initialise imageview
            var imageView = this.FindViewById<ImageView>(Resource.Id.imageView);
            imageView.Alpha = 1F;
            imageView.SetOnTouchListener(this);

            // Initialise menu bar
            var v = FindViewById(Resource.Id.infoLayout).Visibility = ViewStates.Invisible;
            v = FindViewById(Resource.Id.menuLayout).Visibility = ViewStates.Invisible;
            FindViewById<ImageButton>(Resource.Id.folderBrowseBtn).Click += folderBrowseBtn_Click;
            FindViewById<ImageButton>(Resource.Id.imageBrowseBtn).Click += imageBrowseBtn_Click;
            FindViewById<ImageButton>(Resource.Id.toggleInfoBtn).Click += toggleInfoBtn_Click;
            FindViewById<ImageButton>(Resource.Id.moreInfoBtn).Click += moreInfoBtn_Click;
            FindViewById<ImageButton>(Resource.Id.aboutBtn).Click += aboutBtn_Click;

            // Restore state
            var restoreFromBundle = (savedInstanceState != null) ? savedInstanceState : this.Intent?.Extras;
            if (restoreFromBundle != null)
            {
                currentFilePosition = restoreFromBundle.GetInt(CurrentFilePositionKey, DocumentCollection.NoPosition);
                var uriStr = restoreFromBundle.GetString(CurrentDirUriKey);
                if (uriStr != null)
                    currentDirFiles = DocumentCollection.Create(this, Android.Net.Uri.Parse(uriStr), this.imageFileTypes);
            }
            else
            {
                SetMenuBarVisibility(true);
            }
        }





        protected override void OnResume()
        {
            base.OnResume();

            this.EnterImmersiveMode();

            gestureDecoder = new GestureDecoder(this, this.Window, this.FindViewById<ImageView>(Resource.Id.imageView));
            gestureDecoder.Gestured += OnGestured;
        }


        protected override void OnSaveInstanceState(Bundle outState)
        {
            if (currentDirFiles != null)
                outState.PutString(CurrentDirUriKey, currentDirFiles.DirectoryUri.ToString());
            if (currentFilePosition != DocumentCollection.NoPosition)
                outState.PutInt(CurrentFilePositionKey, currentFilePosition);

            base.OnSaveInstanceState(outState);
        }


        protected override void OnRestoreInstanceState(Bundle savedState)
        {
            base.OnRestoreInstanceState(savedState);

            currentFilePosition = savedState.GetInt(CurrentFilePositionKey, DocumentCollection.NoPosition);
            var uriStr = savedState.GetString(CurrentDirUriKey);
            if (uriStr != null)
                currentDirFiles = DocumentCollection.Create(this, Android.Net.Uri.Parse(uriStr), this.imageFileTypes);
        }


        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent resultData)
        {
            base.OnActivityResult(requestCode, resultCode, resultData);

            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case reqActPickDocTree:
                        GrantUriPermission(this.PackageName, resultData.Data, ActivityFlags.GrantPersistableUriPermission | ActivityFlags.GrantReadUriPermission);
                        ContentResolver.TakePersistableUriPermission(resultData.Data, resultData.Flags & ActivityFlags.GrantReadUriPermission);
                        SetCurrentDirectory(resultData.Data);
                        DrawCurrentImage();
                        break;
                    case reqActImageBrowse:
                        var imagePosition = resultData.GetIntExtra(ImageBrowseActivity.BrowseFilePositionKey, DocumentCollection.NoPosition);
                        if (imagePosition != DocumentCollection.NoPosition)
                        {
                            SetCurrentFile(imagePosition);
                        }
                        break;
                    default:
                        base.OnActivityResult(requestCode, resultCode, resultData);
                        break;
                }
            }
        }


        private void OnGestured(object sender, GestureDecoder.Gesture gesture)
        {
            switch (gesture)
            {
                case GestureDecoder.Gesture.Reset:
                    this.gestureDecoder.Reset();
                    return;
                case GestureDecoder.Gesture.ShowMenuBar:
                    SetMenuBarVisibility(true);
                    return;
            }

            if (currentDirFiles == null)
                return;
            switch (gesture)
            {
                case GestureDecoder.Gesture.First:
                    var firstFilePosition = currentDirFiles.FirstPosition();
                    if (firstFilePosition != DocumentCollection.NoPosition)
                    {
                        SetCurrentFile(firstFilePosition);
                        Toast.MakeText(this, Resource.String.MovedToFirstPrompt, ToastLength.Short).Show();
                    }
                    return;
                case GestureDecoder.Gesture.Last:
                    var lastFilePosition = currentDirFiles.LastPosition();
                    if (lastFilePosition != DocumentCollection.NoPosition)
                    {
                        SetCurrentFile(lastFilePosition);
                        Toast.MakeText(this, Resource.String.MovedToLastPrompt, ToastLength.Short).Show();
                    }
                    return;
                case GestureDecoder.Gesture.Random:
                    var newFilePosition = currentDirFiles.RandomPosition(currentFilePosition);
                    if (newFilePosition != DocumentCollection.NoPosition)
                        SetCurrentFile(newFilePosition);
                    return;
            }

            if (currentFilePosition == DocumentCollection.NoPosition)
                return;
            switch (gesture)
            {
                case GestureDecoder.Gesture.Previous:
                    var prevFilePosition = currentDirFiles.PreviousPosition(currentFilePosition);
                    if (prevFilePosition != DocumentCollection.NoPosition)
                        SetCurrentFile(prevFilePosition);
                    else
                        Toast.MakeText(this, Resource.String.PreviousFileNotFoundPrompt, ToastLength.Short).Show();
                    return;
                case GestureDecoder.Gesture.Next:
                    var nextFilePosition = currentDirFiles.NextPosition(currentFilePosition);
                    if (nextFilePosition != DocumentCollection.NoPosition)
                        SetCurrentFile(nextFilePosition);
                    else
                        Toast.MakeText(this, Resource.String.NextFileNotFoundPrompt, ToastLength.Short).Show();
                    return;
            }
        }


        private void OpenSystemDocumentTreeBrowser()
        {
            var openDocTreeIntent = new Intent(Intent.ActionOpenDocumentTree);
            //openDocTreeIntent.AddCategory(Intent.CategoryOpenable);
            if (currentDirFiles != null)
                openDocTreeIntent.PutExtra(DocumentsContract.ExtraInitialUri, currentDirFiles.DirectoryUri.ToString());
            openDocTreeIntent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantPersistableUriPermission | ActivityFlags.GrantPrefixUriPermission);
            StartActivityForResult(openDocTreeIntent, reqActPickDocTree);
        }



        #region IOnTouchListener

        public bool OnTouch(View v, MotionEvent e)
        {
            SetMenuBarVisibility(false);

            // Forward the touch event to the gesture decoder. This will fire a GestureDecoded event when necessary.
            return gestureDecoder.OnTouchEvent(e);
        }

        #endregion IOnTouchListener



        #region Drawing

        private void SetCurrentDirectory(Android.Net.Uri newCurrentDirUri)
        {
            if (newCurrentDirUri == null)
                throw new ArgumentNullException(nameof(newCurrentDirUri));

            var oldCurrentDirFiles = currentDirFiles;
            var oldCurrentFilePosition = currentFilePosition;

            try
            {
                if (newCurrentDirUri != null)
                {
                    currentDirFiles = DocumentCollection.Create(this, newCurrentDirUri, this.imageFileTypes);
                    currentFilePosition = currentDirFiles.FirstPosition();
                    if (currentDirFiles.Length == 0 || currentFilePosition == DocumentCollection.NoPosition)
                    {
                        Toast.MakeText(this, Resource.String.NoImagesPrompt, ToastLength.Short).Show();
                        return;
                    }
                    DrawCurrentImage();
                }
            }
            catch(Exception)
            {
                var prompt = Resources.GetString(Resource.String.DirectoryInaccessiblePrompt);
                Toast.MakeText(this, prompt, ToastLength.Long).Show();

                currentDirFiles = oldCurrentDirFiles;
                currentFilePosition = oldCurrentFilePosition;
            }
        }


        private void SetCurrentFile(int newCurrentFilePosition)
        {
            currentFilePosition = newCurrentFilePosition;
            DrawCurrentImage();
        }



        private const long imageTransitionDuration = 125L;

        private AlphaAnimation imageFadeOut = new AlphaAnimation(1f, 0f)  // fully opaque -> fully transparent
        {
            Interpolator = new AccelerateInterpolator(),
            Duration = imageTransitionDuration
        };
        private AlphaAnimation imageFadeIn = new AlphaAnimation(0f, 1f)  // fully transparent -> fully opaque
        {
            Interpolator = new AccelerateInterpolator(),
            Duration = imageTransitionDuration
        };


        private void DrawCurrentImage()
        {
            if (currentFilePosition == DocumentCollection.NoPosition)
                return;

            var imageDocFileUri = currentDirFiles.UriAt(this, currentFilePosition);
            using (var img = BitmapFactory.DecodeStream(this.ContentResolver.OpenInputStream(imageDocFileUri)))
            {
                var imgView = this.FindViewById<ImageView>(Resource.Id.imageView);
                var displayImg = img.Resize(imgView.Width, imgView.Height);
                imageFadeOut.AnimationEnd += (s, e) =>
                {
                    // At the end of the fade-out, swap to the new image and start the fade-in.
                    this.gestureDecoder.Reset();
                    imgView.SetImageBitmap(displayImg);
                    imgView.StartAnimation(imageFadeIn);
                };
                imgView.StartAnimation(imageFadeOut);

                var documentInfo = DocumentInfo.Create(this, imageDocFileUri);
                this.FindViewById<TextView>(Resource.Id.infoTitle).Text = documentInfo.DisplayName;
                this.FindViewById<TextView>(Resource.Id.infoDetail).Text = $"{img.Width}x{img.Height}";
            }
        }

        #endregion Drawing


        #region Menu

        // Note that layout and visibility changes to the menu bar will be animated by the parent view.

        private void SetMenuBarVisibility(bool visible)
        {
            this.FindViewById(Resource.Id.menuLayout).Visibility = visible ? ViewStates.Visible : ViewStates.Invisible;
        }


        public void folderBrowseBtn_Click(object sender, EventArgs e)
        {
            OpenSystemDocumentTreeBrowser();
        }


        public void imageBrowseBtn_Click(object sender, EventArgs e)
        {
            if (this.currentDirFiles?.Length > 0)
            {
                var intent = new Intent(this, typeof(ImageBrowseActivity));
                intent.PutExtra(ImageBrowseActivity.BrowseDirTreeUriKey, this.currentDirFiles.DirectoryUri.ToString());
                intent.PutExtra(ImageBrowseActivity.BrowseFilePositionKey, this.currentFilePosition);
                intent.PutExtra(ImageBrowseActivity.ImageFileTypesKey, this.imageFileTypes);
                StartActivityForResult(intent, reqActImageBrowse);
            }
        }


        public void toggleInfoBtn_Click(object sender, EventArgs e)
        {
            if (this.currentFilePosition != DocumentCollection.NoPosition)
            {
                var infoLayout = this.FindViewById(Resource.Id.infoLayout);
                infoLayout.Visibility = (infoLayout.Visibility == ViewStates.Visible) ? ViewStates.Invisible : ViewStates.Visible;
            }
        }


        public void moreInfoBtn_Click(object sender, EventArgs e)
        {
            if (this.currentDirFiles != null && this.currentFilePosition != DocumentCollection.NoPosition)
            {
                var docInfo = DocumentInfo.Create(this, currentDirFiles.UriAt(this, currentFilePosition));

                var intent = new Intent(this, typeof(ImagePropertiesActivity));
                intent.PutExtra(ImagePropertiesActivity.ImageFileUriKey, currentDirFiles.UriAt(this, currentFilePosition).ToString());
                StartActivityForResult(intent, reqActImageProps);
            }
        }


        public void aboutBtn_Click(object sender, EventArgs e)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var pkgInfo = this.PackageManager.GetPackageInfo(this.PackageName, PackageInfoFlags.MetaData);
#pragma warning restore CS0618 // Type or member is obsolete
            var adb = new Android.App.AlertDialog.Builder(this, Resource.Style.AppDialogTheme);
            adb.SetCancelable(true);
            adb.SetIcon(Resource.Drawable.ic_appIcon);
            adb.SetTitle(Resource.String.AboutPrompt);
            adb.SetMessage(Resources.GetString(Resource.String.AboutMessage, pkgInfo.VersionName));
            adb.Create().Show();
        }

        #endregion Menu
    }
}
