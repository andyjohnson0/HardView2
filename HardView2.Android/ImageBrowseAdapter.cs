using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Graphics;

using AndroidX.RecyclerView.Widget;
using Android.Media;
using Android.Util;

namespace uk.andyjohnson.HardView2
{
    /// <summary>
    /// Adapter for ImageBrowseActivity recycle view.
    /// </summary>
    public class ImageBrowseAdapter : RecyclerView.Adapter
    {
        /// <summary>
        /// Constructor. Initailise an ImageBrowseAdapter object.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="documentFiles">Document files to display</param>
        /// <param name="columnWidthDp">Desired column width</param>
        public ImageBrowseAdapter(Context context, DocumentCollection documentFiles, int columnWidthDp)
        {
            this.context = context;
            this.documentFiles = documentFiles;
            this.columnWidthPx = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, columnWidthDp, this.context.Resources.DisplayMetrics);
        }


        private readonly Context context;
        private readonly DocumentCollection documentFiles;
        private readonly int columnWidthPx;


        /// <summary>
        /// Click event
        /// </summary>
        public event EventHandler<ImageBrowseAdapterClickEventArgs> ItemClick;



        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var ll = LayoutInflater.From(this.context).Inflate(Resource.Layout.imageBrowse_item, parent, false);
            return new ImageBrowseAdapterViewHolder(ll, OnClick);
        }


        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var holder = viewHolder as ImageBrowseAdapterViewHolder;
            var imageFileUri = documentFiles.UriAt(context, position);

            // Get the image size without loading the image content.
            var options = new BitmapFactory.Options() { InJustDecodeBounds = true };
            BitmapFactory.DecodeStream(this.context.ContentResolver.OpenInputStream(imageFileUri), null, options);
            options.InJustDecodeBounds = false;

            // Load the image.
            if ((options.OutWidth > columnWidthPx) || (options.OutHeight > columnWidthPx))
            {
                // Bigger than column width (typical case) so need to scale it on loading to avoid dragging a potentially
                // very large image into memory.
                // BitmapFactory.DecodeStream() does scaling by a single integer pixel sampling value, so we calculate
                // this to give us the smallest image that is just larger than the column width in both width and height
                // while preserving it's aspect ratio.
                var sf = (float)Math.Min(options.OutWidth, options.OutHeight) / (float)columnWidthPx;
                options.InSampleSize = (int)sf;
            }
            using (var bmp = BitmapFactory.DecodeStream(this.context.ContentResolver.OpenInputStream(imageFileUri), null, options))
            {
                if (bmp.Width > columnWidthPx || bmp.Height > columnWidthPx)
                {
                    // If the loaded image is wider or higher than the column size then extract a square from its centre and display that.
                    var x = bmp.Width > columnWidthPx ? (bmp.Width - columnWidthPx) / 2 : 0;
                    var y = bmp.Height > columnWidthPx ? (bmp.Height - columnWidthPx) / 2 : 0;
                    var w = Math.Min(bmp.Width, columnWidthPx);
                    var h = Math.Min(bmp.Height, columnWidthPx);
                    holder.ImageView.SetImageBitmap(Bitmap.CreateBitmap(bmp, x, y, w, h));
                }
                else
                {
                    holder.ImageView.SetImageBitmap(bmp);
                }
            }
        }

        public override int ItemCount
        {
            get { return documentFiles != null ? documentFiles.Length : 0; }
        }


        private void OnClick(ImageBrowseAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }
    }


    /// <summary>
    /// View holder for ImageBrowseActivity recycle view.
    /// </summary>
    public class ImageBrowseAdapterViewHolder : RecyclerView.ViewHolder
    {
        public ImageBrowseAdapterViewHolder(
            View itemView,
            Action<ImageBrowseAdapterClickEventArgs> clickListener) : base(itemView)
        {
            itemView.Click += (sender, e) => clickListener(new ImageBrowseAdapterClickEventArgs { View = itemView, Position = this.BindingAdapterPosition });
        }


        public ImageView ImageView
        {
            get
            {
                return ((ViewGroup)ItemView).FindViewById<ImageView>(Resource.Id.imageBrowse_image);
            }
        }
    }


    public class ImageBrowseAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}