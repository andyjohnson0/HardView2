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
        public DirectoryInfoViewHolder(View itemView) : base(itemView)
        {
        }


        public TextView Label
        {
            get
            {
                return ((ViewGroup)ItemView).GetChildAt(0) as TextView;
            }
        }
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


        public override int ItemCount
        {
            get { return directoryInfo.GetDirectories().Length + 1; }
        }


        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var ll = new LinearLayout(context);
            var layoutParams = new RecyclerView.LayoutParams(RecyclerView.LayoutParams.WrapContent, RecyclerView.LayoutParams.WrapContent);
            ll.LayoutParameters = layoutParams;
            var tv = new TextView(context);
            tv.LayoutParameters = new RecyclerView.LayoutParams(RecyclerView.LayoutParams.MatchParent, RecyclerView.LayoutParams.MatchParent);
            ll.AddView(tv);
            return new DirectoryInfoViewHolder(ll);
        }


        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as DirectoryInfoViewHolder;
            if (position == 0)
                vh.Label.Text = "..";
            else
                vh.Label.Text = directoryInfo.GetDirectories()[position - 1].Name;
        }
    }
}