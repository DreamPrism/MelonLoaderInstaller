﻿using System.Collections.Generic;
using Android;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Activity.Result.Contract;
using AndroidX.AppCompat.App;
using MelonLoaderInstaller.App.Adapters;
using MelonLoaderInstaller.App.Models;
using MelonLoaderInstaller.App.Utilities;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace MelonLoaderInstaller.App.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.MelonLoaderInstaller", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, AdapterView.IOnItemClickListener
    {
        private List<UnityApplicationData> _availableApps;
        private Toast _unsupportedToast;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Logger.SetupMainInstance("melonloader");

            PackageWarnings.Run(this);

            _availableApps = UnityApplicationFinder.Find(this);
            ApplicationsAdapter adapter = new ApplicationsAdapter(this, _availableApps);

            ListView listView = FindViewById<ListView>(Resource.Id.application_list);
            listView.Adapter = adapter;
            listView.OnItemClickListener = this;

            FolderPermission.l = RegisterForActivityResult(new ActivityResultContracts.StartActivityForResult(), new FolderPermissionCallback());

            TryRequestPermissions();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (grantResults.Length > 0 && grantResults[0] == Android.Content.PM.Permission.Granted)
            {
                RequestInstallUnknownSources();
            }
            else
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder
                        .SetTitle("Permissions Issue")
                        .SetMessage("Lemon needs to be granted storage permissions to function!")
                        .SetPositiveButton("Setup", (o, di) => TryRequestPermissions())
                        .SetIcon(Android.Resource.Drawable.IcDialogAlert);

                AlertDialog alert = builder.Create();
                alert.SetCancelable(false);
                alert.Show();
            }
        }

        public void TryRequestPermissions()
        {
            bool canRead = CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Android.Content.PM.Permission.Granted;
            bool canWrite = CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Android.Content.PM.Permission.Granted;

            if (!canRead || !canWrite)
            {
                RequestPermissions(new string[]
                {
                    Manifest.Permission.ReadExternalStorage,
                    Manifest.Permission.WriteExternalStorage,
                }, 100);
            }

            // TODO: does this fix the funny bug?
            if (!Environment.IsExternalStorageManager)
            {
                var uri = Uri.Parse($"package:{Application.Context?.ApplicationInfo?.PackageName}");
                var permissionIntent = new Intent(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission, uri);
                StartActivity(permissionIntent);
            }
        }

        public void RequestInstallUnknownSources()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder
                    .SetTitle("Install Permission")
                    .SetMessage("Lemon needs permission to install apps from unknown sources to function!")
                    .SetPositiveButton("Setup", (o, di) => StartActivity(new Intent(Android.Provider.Settings.ActionManageUnknownAppSources, Uri.Parse("package:" + PackageName))))
                    .SetIcon(Android.Resource.Drawable.IcDialogAlert);

            AlertDialog alert = builder.Create();
            alert.SetCancelable(false);
            alert.Show();
        }

        public void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            UnityApplicationData app = _availableApps[position];

            if (!app.IsSupported)
            {
                _unsupportedToast ??= Toast.MakeText(this, "Unsupported application", ToastLength.Short);
                _unsupportedToast.Show();
                return;
            }

            Intent intent = new Intent();
            intent.SetClass(this, typeof(ViewApplication));
            intent.PutExtra("target.packageName", app.PackageName);
            StartActivity(intent);
        }
    }
}