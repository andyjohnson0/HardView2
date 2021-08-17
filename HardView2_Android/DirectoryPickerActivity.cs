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

using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;

using System.IO;

namespace HardView2
{
    [Activity(Label = "@string/SelectDirectoryPrompt", Theme = "@style/Theme.AppCompat.Light.Dialog.Alert")]
    public class DirectoryPickerActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (savedInstanceState != null)
            {
                currentDirectory = new DirectoryInfo(savedInstanceState.GetString("DirPath"));
            }
            else if (this.Intent?.Extras != null)
            {
                currentDirectory = new DirectoryInfo(this.Intent.Extras.GetString("DirPath"));
            }

            SetContentView(Resource.Layout.activity_directoryPicker);

            dirRecView = FindViewById<RecyclerView>(Resource.Id.dirRecView);
            layoutMgr = new LinearLayoutManager(this);
            dirRecView.SetLayoutManager(layoutMgr);
            var adapter = new DirectoryInfoAdapter(this, currentDirectory);
            adapter.ItemClick += OnItemClick;
            dirRecView.SetAdapter(adapter);
        }


        private DirectoryInfo currentDirectory;
        private RecyclerView dirRecView;
        private RecyclerView.LayoutManager layoutMgr;


        private void OnItemClick(object sender, DirectoryInfo di)
        {
            SetResult(Result.Ok, new Intent().PutExtra("DirPath", di.FullName));
            Finish();
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
    }
}