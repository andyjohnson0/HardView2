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

using AndroidX.RecyclerView.Widget;


namespace uk.andyjohnson.HardView2
{
    /// <summary>
    /// Adapter for ImagePropertiesActivity recycle view.
    /// </summary>
    public class ImagePropertiesAdapter : RecyclerView.Adapter
    {
        /// <summary>
        /// Constructor. Initialise a ImagePropertiesAdapter object.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="properties">List of property label/value pairs</param>
        /// <exception cref="ArgumentNullException">Argument must not be null</exception>
        public ImagePropertiesAdapter(
            Context context,
            IList<KeyValuePair<string,string>> properties)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));
            
            this.context = context;
            this.properties = properties;
        }


        private readonly Context context;
        private readonly IList<KeyValuePair<string, string>> properties;


        /// <summary>
        /// Get number of properties
        /// </summary>
        public override int ItemCount
        {
            get { return properties.Count * 2; }
        }


        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var ll = LayoutInflater.From(this.context).Inflate(Resource.Layout.imageProperties_item, parent, false);
            return new ImagePropertyViewHolder(ll);
        }


        private KeyValuePair<string, string> GetKvpForPosition(int position)
        {
            return properties[position];
        }


        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as ImagePropertyViewHolder;
            var kvp = GetKvpForPosition(position / 2);
            vh.Label.Text = (position % 2 == 0) ? kvp.Key : kvp.Value;
        }
    }


    /// <summary>
    /// View holder for ImagePropertiesActivity recycle view.
    /// </summary>
    public class ImagePropertyViewHolder : RecyclerView.ViewHolder
    {
        public ImagePropertyViewHolder(View itemView) : base(itemView)
        {
        }

        public TextView Label
        {
            get
            {
                return ((ViewGroup)ItemView).FindViewById<TextView>(Resource.Id.imageProperties_label);
            }
        }
    }
}