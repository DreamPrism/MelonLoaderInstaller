using AssetsTools.NET.Extra;
using System;
using System.IO;
using System.IO.Compression;
using UnityVersion = AssetRipper.Primitives.UnityVersion;

namespace MelonLoaderInstaller.Core.PatchSteps
{
    internal class DetectUnityVersion : IPatchStep
    {
        public bool Run(Patcher patcher)
        {
            if (patcher._args.UnityVersion != null && patcher._args.UnityVersion != UnityVersion.MinVersion)
                return true;

            using var apkStream = new FileStream(patcher._info.OutputBaseApkPath, FileMode.Open);
            using var archive = new ZipArchive(apkStream, ZipArchiveMode.Read);

            var uAssetsManager = new AssetsManager();

            // Try to read directly from file
            try
            {
                var assetEntry = archive.GetEntry("bin/Data/globalgamemanagers");
                using var stream = assetEntry?.Open();

                var instance = uAssetsManager.LoadAssetsFile(stream, "/bin/Data/globalgamemanagers", true);
                patcher._args.UnityVersion = UnityVersion.Parse(instance.file.Metadata.UnityVersion);

                return true;
            }
            catch
            {
                // ignored
            }

            // If failed before, try to get the data from data.unity3d
            try
            {
                var assetEntry = archive.GetEntry("bin/Data/data.unity3d");
                using var stream = assetEntry?.Open();

                var bundle = uAssetsManager.LoadBundleFile(stream, "/bin/Data/data.unity3d");
                var instance = uAssetsManager.LoadAssetsFileFromBundle(bundle, "globalgamemanagers");
                patcher._args.UnityVersion = UnityVersion.Parse(instance.file.Metadata.UnityVersion);
            }
            catch (Exception ex)
            {
                patcher._logger.Log($"Failed to get Unity version, cannot patch.\n{ex}");
                patcher._args.UnityVersion = UnityVersion.MinVersion;
                return false;
            }

            return true;
        }
    }
}
