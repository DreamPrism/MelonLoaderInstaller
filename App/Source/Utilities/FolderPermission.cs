﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Provider;
using AndroidX.Activity.Result;
using AndroidX.DocumentFile.Provider;
using Java.Lang;
using Xamarin.Essentials;
using File = System.IO.File;

namespace MelonLoaderInstaller.App.Utilities
{
    // adapted from https://github.com/ComputerElite/QuestAppVersionSwitcher/blob/a168f5c0a77d35fb9d00096d1e75352032ced756/QuestAppVersionSwitcher/FolderPermission.cs
    public static class FolderPermission
    {
        public static Activities.MainActivity CurrentContext;

        public static ActivityResultLauncher l = null;
        public static void OpenDirectory(string dirInExtenalStorage)
        {
            //if (Build.VERSION.SdkInt <= BuildVersionCodes.SV2 && !Directory.Exists(dirInExtenalStorage))
            //    Directory.CreateDirectory(dirInExtenalStorage);

            Intent intent = new Intent(Intent.ActionOpenDocumentTree).PutExtra(DocumentsContract.ExtraInitialUri, Uri.Parse(RemapPathForApi300OrAbove(dirInExtenalStorage)));
            l.Launch(intent);
        }

        public static string RemapPathForApi300OrAbove(string path)
        {
            string text = path;
            if (text.StartsWith("/sdcard"))
                text = text["/sdcard".Length..];

            if (text.StartsWith(Environment.ExternalStorageDirectory.AbsolutePath))
                text = path[Environment.ExternalStorageDirectory.AbsolutePath.Length..];

            if (text.Length < 1)
                text = "/";

            string documentId = "primary:" + text[1..];
            return DocumentsContract.BuildDocumentUri("com.android.externalstorage.documents", documentId).ToString();
        }

        public static DocumentFile GetAccessToFile(string dir)
        {
            string text = "/sdcard/Android/data";
            if (dir.Contains("/Android/obb/"))
                text = "/sdcard/Android/obb";

            string diff = dir.Replace(text, "");
            string[] dirs = diff.Split('/');
            DocumentFile docFile = DocumentFile.FromTreeUri(CurrentContext, Uri.Parse(RemapPathForApi300OrAbove(text).Replace("com.android.externalstorage.documents/document/", "com.android.externalstorage.documents/tree/")));
            foreach (string dirName in dirs)
            {
                if (string.IsNullOrWhiteSpace(dirName))
                    continue;

                if (docFile.FindFile(dirName) == null)
                    docFile.CreateDirectory(dirName);

                docFile = docFile.FindFile(dirName);
            }
            return docFile;
        }

        public static bool GotAccessTo(string dirInExtenalStorage)
        {
            if (!Directory.Exists(dirInExtenalStorage))
                return false;

            string b = RemapPathForApi300OrAbove(dirInExtenalStorage).Replace("com.android.externalstorage.documents/document/", "com.android.externalstorage.documents/tree/");
            foreach (UriPermission perm in Platform.AppContext.ContentResolver.PersistedUriPermissions)
            {
                if (perm.Uri.ToString() == b)
                    return true;
            }

            return false;
        }
    }

    public class FolderPermissionCallback : Object, IActivityResultCallback
    {
        public void OnActivityResult(Result resultCode, Intent data)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                if (data.Data != null)
                {
                    FolderPermission.CurrentContext.ContentResolver.TakePersistableUriPermission(
                        data.Data,
                        ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);

                    FolderPermission.CurrentContext.RequestFolderPermissions();
                }
            }
        }
        public void OnActivityResult(Object result)
        {
            if (result is ActivityResult activityResult && Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                if (activityResult.Data.Data != null)
                {
                    FolderPermission.CurrentContext.ContentResolver.TakePersistableUriPermission(
                        activityResult.Data.Data,
                        ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);

                    FolderPermission.CurrentContext.RequestFolderPermissions();
                }
            }
        }
    }
}