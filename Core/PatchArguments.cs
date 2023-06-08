﻿namespace MelonLoaderInstaller.Core
{
    /// <summary>
    /// Public-facing class for user set information
    /// </summary>
    public class PatchArguments
    {
        public string TargetApkPath;
        public string LibraryApkPath;

        public string OutputApkDirectory;

        public string TempDirectory;

        public string LemonDataPath;
        public string Il2CppEtcPath;
        public string UnityDependenciesPath;

        public AssetRipper.VersionUtilities.UnityVersion? UnityVersion;
        public string PackageName;

        public bool IsSplit;

        public PatchArguments() { }
    }
}