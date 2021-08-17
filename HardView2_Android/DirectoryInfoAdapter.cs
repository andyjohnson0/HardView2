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

using System.IO;


namespace HardView2
{
    public class DirectoryInfoViewHolder : RecyclerView.ViewHolder
    {
        public DirectoryInfoViewHolder(View itemView, Action<int> listener) : base(itemView)
        {
            itemView.Click += (sender, e) => listener(base.LayoutPosition);
        }


        public TextView Label
        {
            get
            {
                return ((ViewGroup)ItemView).FindViewById<TextView>(Resource.Id.directoryPicker_item_label) as TextView;
            }
        }


        public DirectoryInfo Directory { get; set; }
    }



    public class DirectoryInfoAdapter : RecyclerView.Adapter
    {
        public DirectoryInfoAdapter(
            Context context, 
            DirectoryInfo directoryInfo)
        {
            this.context = context;
            this.directoryInfo = directoryInfo;
        }


        private Context context;
        private DirectoryInfo directoryInfo;


        public event EventHandler<DirectoryInfo> ItemClick;


        private void OnClick(int position)
        {
            if (ItemClick != null)
                ItemClick(this, GetDirectoryForPosition(position));
        }


        public override int ItemCount
        {
            get { return directoryInfo.GetDirectories().Length + (directoryInfo.Parent != null ? 1 : 0); }
        }


        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var ll = LayoutInflater.From(this.context).Inflate(Resource.Layout.directoryPicker_item, parent, false);
            return new DirectoryInfoViewHolder(ll, OnClick);
        }


        private DirectoryInfo GetDirectoryForPosition(int position)
        {
            if (directoryInfo.Parent != null)
            {
                if (position == 0)
                {
                    return directoryInfo.Parent;
                }
                else
                {
                    return directoryInfo.GetDirectories()[position - 1];
                }
            }
            else
            {
                return directoryInfo.GetDirectories()[position];
            }
        }


        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as DirectoryInfoViewHolder;
            vh.Directory = GetDirectoryForPosition(position);
            vh.Label.Text = (vh.Directory.FullName == directoryInfo.Parent?.FullName) ? ".." : vh.Directory.Name;
        }
    }
}