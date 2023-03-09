using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;


namespace uk.andyjohnson.HardView2
{
    /// <summary>
    /// Browse images in a directory using a recycler view.
    /// </summary>
    [Activity(Label = "@string/ImageBrowse", Theme = "@style/AppTheme.NoActionBar" )]
    public class ImageBrowseActivity : AppCompatActivity
    {
        public const string BrowseDirTreeUriKey = "BrowseDirUri";          // in: directory url
        public const string BrowseFilePositionKey = "BrowseFilePosition";  // in/out: current position / selection position
        public const string ImageFileTypesKey = "ImageFileTypes";          // in: image files types (e.g. "*.jpg")

        private const int columnWidthDp = 150;

        private Android.Net.Uri browseDirUri;
        private string[] imageFileTypes;
        private DocumentCollection imageFiles;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_imageBrowse);
            this.EnterImmersiveMode();

            // Restore state
            var currentPos = DocumentCollection.NoPosition;
            var restoreFromBundle = (savedInstanceState != null) ? savedInstanceState : this.Intent?.Extras;
            if (restoreFromBundle != null)
            {
                this.browseDirUri = Android.Net.Uri.Parse(restoreFromBundle.GetString(BrowseDirTreeUriKey));
                this.imageFileTypes = restoreFromBundle.GetStringArray(ImageFileTypesKey);
                currentPos = restoreFromBundle.GetInt(BrowseFilePositionKey, DocumentCollection.NoPosition);
            }

            //
            FindViewById<ImageButton>(Resource.Id.imageBrowseBackBtn).Click += BackBtn_Click;

            //
            imageFiles = DocumentCollection.Create(this, browseDirUri, imageFileTypes);
            var numColumns = CalcColumnCount(this, columnWidthDp);
            var adapter = new ImageBrowseAdapter(this, imageFiles, columnWidthDp);
            adapter.ItemClick += ItemClick;
            var propsRecView = FindViewById<RecyclerView>(Resource.Id.imageBrowseRecView);
            propsRecView.SetLayoutManager(new GridLayoutManager(this, numColumns));
            propsRecView.SetAdapter(adapter);

            if (currentPos != DocumentCollection.NoPosition)
                propsRecView.ScrollToPosition(currentPos);
        }


        protected override void OnResume()
        {
            base.OnResume();
            this.EnterImmersiveMode();
        }


        protected override void OnSaveInstanceState(Bundle outState)
        {
            if (browseDirUri != null)
                outState.PutString(BrowseDirTreeUriKey, this.browseDirUri.ToString());
            if (imageFileTypes != null)
                outState.PutStringArray(ImageFileTypesKey, this.imageFileTypes);
            base.OnSaveInstanceState(outState);
        }


        protected override void OnRestoreInstanceState(Bundle savedState)
        {
            base.OnRestoreInstanceState(savedState);
            this.browseDirUri = Android.Net.Uri.Parse(savedState.GetString(BrowseDirTreeUriKey));
            this.imageFileTypes = savedState.GetStringArray(ImageFileTypesKey);
        }


        /// <summary>
        /// Calculate the number of columns of a given width that will fit across the display.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="columnWidthDp">Desired column width</param>
        /// <returns>Number of columns</returns>
        private static int CalcColumnCount(Context context, float columnWidthDp)
        {
            var metrics = context.Resources.DisplayMetrics;
            var displayWidthDp = metrics.WidthPixels / metrics.Density;
            return (int)(displayWidthDp / columnWidthDp + 0.5f);
        }


        private void ItemClick(object sender, ImageBrowseAdapterClickEventArgs e)
        {
            // Return the file index.
            var intent = new Intent();
            intent.PutExtra(BrowseFilePositionKey, e.Position);
            SetResult(Result.Ok, intent);
            Finish();
        }


        private void BackBtn_Click(object sender, EventArgs e)
        {
            SetResult(Result.Canceled);
            Finish();
        }
    }
}
