using AssetsTools.NET.Extra;
using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityVersion = AssetRipper.Primitives.UnityVersion;
using System.Reflection;

namespace MelonLoader.WizardExtension
{
    // copied from MelonLoader
    internal static class UnityDataParser
    {
        internal static void Run(GameInfo info)
        {
            AssetsManager assetsManager = new();
            ReadGameInfo(assetsManager, info);
            assetsManager.UnloadAll();

            if (string.IsNullOrEmpty(info.GameDeveloper)
                || string.IsNullOrEmpty(info.GameName))
                ReadGameInfoFallback(info);

            if (info.EngineVersion == UnityVersion.MinVersion)
            {
                try { info.EngineVersion = ReadVersionFallback(info); }
                catch { }
            }

            if (string.IsNullOrEmpty(info.GameDeveloper))
                info.GameDeveloper = null;
            if (string.IsNullOrEmpty(info.GameName))
                info.GameName = null;
        }

        private static void ReadGameInfo(AssetsManager assetsManager, GameInfo info)
        {
            AssetsFileInstance instance = null;
            try
            {
                string bundlePath = Path.Combine(info.DataPath, "globalgamemanagers");
                if (!File.Exists(bundlePath))
                    bundlePath = Path.Combine(info.DataPath, "mainData");

                if (!File.Exists(bundlePath))
                {
                    bundlePath = Path.Combine(info.DataPath, "data.unity3d");
                    if (!File.Exists(bundlePath))
                        return;

                    BundleFileInstance bundleFile = assetsManager.LoadBundleFile(bundlePath);
                    instance = assetsManager.LoadAssetsFileFromBundle(bundleFile, "globalgamemanagers");
                }
                else
                    instance = assetsManager.LoadAssetsFile(bundlePath, true);
                if (instance == null)
                    return;

                ClassPackageFile classPackage = null;
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MelonLoader.WizardExtension.Resources.classdata.tpk"))
                    classPackage = assetsManager.LoadClassPackage(stream);

                if (!instance.file.Metadata.TypeTreeEnabled)
                    assetsManager.LoadClassDatabaseFromPackage(instance.file.Metadata.UnityVersion);

                if (info.EngineVersion == UnityVersion.MinVersion)
                    info.EngineVersion = UnityVersion.Parse(instance.file.Metadata.UnityVersion);

                List<AssetFileInfo> assetFiles = instance.file.GetAssetsOfType(AssetClassID.PlayerSettings);
                if (assetFiles.Count > 0)
                {
                    AssetFileInfo playerSettings = assetFiles.First();

                    AssetTypeValueField playerSettings_baseField = assetsManager.GetBaseField(instance, playerSettings);
                    if (playerSettings_baseField != null)
                    {
                        AssetTypeValueField bundleVersion = playerSettings_baseField.Get("bundleVersion");
                        AssetTypeValueField companyName = playerSettings_baseField.Get("companyName");
                        if (companyName != null)
                            info.GameDeveloper = companyName.AsString;

                        AssetTypeValueField productName = playerSettings_baseField.Get("productName");
                        if (productName != null)
                            info.GameName = productName.AsString;
                    }
                }
            }
            catch { }

            instance?.file.Close();
        }

        private static void ReadGameInfoFallback(GameInfo info)
        {
            try
            {
                string appInfoFilePath = Path.Combine(info.DataPath, "app.info");
                if (!File.Exists(appInfoFilePath))
                    return;

                string[] filestr = File.ReadAllLines(appInfoFilePath);
                if ((filestr == null) || (filestr.Length < 2))
                    return;

                if (string.IsNullOrEmpty(info.GameDeveloper) && !string.IsNullOrEmpty(filestr[0]))
                    info.GameDeveloper = filestr[0];

                if (string.IsNullOrEmpty(info.GameName) && !string.IsNullOrEmpty(filestr[1]))
                    info.GameName = filestr[1];

            }
            catch { }
        }

        private static UnityVersion ReadVersionFallback(GameInfo info)
        {
            string unityPlayerPath = info.ExePath;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var unityVer = FileVersionInfo.GetVersionInfo(unityPlayerPath);
                return new UnityVersion((ushort)unityVer.FileMajorPart, (ushort)unityVer.FileMinorPart, (ushort)unityVer.FileBuildPart);
            }

            try
            {
                var globalgamemanagersPath = Path.Combine(info.DataPath, "globalgamemanagers");
                if (File.Exists(globalgamemanagersPath))
                    return GetVersionFromGlobalGameManagers(File.ReadAllBytes(globalgamemanagersPath));
            }
            catch { }

            try
            {
                var dataPath = Path.Combine(info.DataPath, "data.unity3d");
                if (File.Exists(dataPath))
                    return GetVersionFromDataUnity3D(File.OpenRead(dataPath));
            }
            catch { }

            return default;
        }

        private static UnityVersion GetVersionFromGlobalGameManagers(byte[] ggmBytes)
        {
            var verString = new StringBuilder();
            var idx = 0x14;
            while (ggmBytes[idx] != 0)
            {
                verString.Append(Convert.ToChar(ggmBytes[idx]));
                idx++;
            }

            Regex UnityVersionRegex = new(@"^[0-9]+\.[0-9]+\.[0-9]+[abcfx][0-9]+$", RegexOptions.Compiled);
            string unityVer = verString.ToString();
            if (!UnityVersionRegex.IsMatch(unityVer))
            {
                idx = 0x30;
                verString = new StringBuilder();
                while (ggmBytes[idx] != 0)
                {
                    verString.Append(Convert.ToChar(ggmBytes[idx]));
                    idx++;
                }

                unityVer = verString.ToString().Trim();
            }

            return UnityVersion.Parse(unityVer);
        }

        private static UnityVersion GetVersionFromDataUnity3D(Stream fileStream)
        {
            var verString = new StringBuilder();

            if (fileStream.CanSeek)
                fileStream.Seek(0x12, SeekOrigin.Begin);
            else
            {
                if (fileStream.Read(new byte[0x12], 0, 0x12) != 0x12)
                    throw new("Failed to seek to 0x12 in data.unity3d");
            }

            while (true)
            {
                var read = fileStream.ReadByte();
                if (read == 0)
                    break;
                verString.Append(Convert.ToChar(read));
            }

            return UnityVersion.Parse(verString.ToString().Trim());
        }
    }
}
