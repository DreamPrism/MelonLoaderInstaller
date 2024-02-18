// See https://aka.ms/new-console-template for more information

using System.Reflection;
using AssetRipper.Primitives;
using MelonLoaderInstaller.ConsolePatcher;
using MelonLoaderInstaller.Core;

Console.Title = "MelonLoader Installer";
Console.WriteLine("MelonLoader Installer running...");

var packageName = "com.winterspringgames.survivaljourney";
var packageTempPath = Path.Combine("temp", packageName);
var lemonDataPath = Path.Combine(packageTempPath, "dependencies.zip");
var il2cppEtcPath = Path.Combine(packageTempPath, "il2cpp_etc.zip");
var unityDepsPath = Path.Combine(packageTempPath, "unity.zip");

Directory.CreateDirectory(packageTempPath);


var assembly = Assembly.GetExecutingAssembly();
using var deps = assembly.GetManifestResourceStream("MelonLoaderInstaller.ConsolePatcher.Resources.dependencies.zip");
using var il2cpp_etc = assembly.GetManifestResourceStream("MelonLoaderInstaller.ConsolePatcher.Resources.il2cpp_etc.zip");
using var unity_zip = assembly.GetManifestResourceStream("MelonLoaderInstaller.ConsolePatcher.Resources.unity.zip");
using var lemonData = File.Create(lemonDataPath);
using var il2cpp = File.Create(il2cppEtcPath);
using var unity = File.Create(unityDepsPath);
deps?.CopyTo(lemonData);
il2cpp_etc?.CopyTo(il2cpp);
unity_zip?.CopyTo(unity);
lemonData.Close();
il2cpp.Close();
unity.Close();

var patcher = new Patcher(
    new PatchArguments
    {
        IsSplit = false,
        TempDirectory = "temp",
        OutputApkDirectory = "output",
        TargetApkPath = "input.apk",
        PackageName = packageName,
        LemonDataPath = lemonDataPath,
        Il2CppEtcPath = il2cppEtcPath,
        UnityDependenciesPath = unityDepsPath,
        UnityVersion = new UnityVersion(2019, 4, 38)
    }, new ConsoleLogger()
);
patcher.Run();