using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace MelonLoader.VSExtension
{
    public class WizardImplementation : IWizard
    {
        private Dictionary<string, string> _replacements = [];

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(Project project)
        {
        }

        public void RunFinished()
        {
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            try
            {
                BeginWizard();
                foreach (var item in _replacements)
                    replacementsDictionary.Add(item.Key, item.Value);
            }
            catch (WizardCancelledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw new WizardCancelledException();
            }
        }

        private void BeginWizard()
        {
            using OpenFileDialog dialog = new();
            dialog.Title = "Select Game Executable";
            dialog.Multiselect = false;
            dialog.Filter = "Unity Executables (*.exe)|*.exe";

            if (dialog.ShowDialog() != DialogResult.OK)
                throw new WizardCancelledException();

            var info = TryParseGamePath(Path.GetDirectoryName(dialog.FileName));

            #region Framework

            string framework = "6.0";
            if (!info.IsMelon6Plus && info.IsIl2Cpp)
                framework = "472";

            if (!info.IsIl2Cpp)
            {
                if (info.EngineVersion >= new AssetRipper.Primitives.UnityVersion(2021, 2, 0))
                    framework = "standard2.1";
                else if (info.EngineVersion >= new AssetRipper.Primitives.UnityVersion(2018, 1, 0))
                    framework = "472";
                else if (info.EngineVersion >= new AssetRipper.Primitives.UnityVersion(2017, 1, 0))
                    framework = "35"; // possible for it to be 472, but this is a safer bet
                else
                    framework = "35";
            }

            #endregion

            #region Reference Generation

            string il2cppDllDir = info.IsMelon6Plus ? Path.Combine(info.Path, "MelonLoader", "Il2CppAssemblies") : Path.Combine(info.Path, "MelonLoader", "Managed");
            string dllDir = info.IsIl2Cpp ? il2cppDllDir : Path.Combine(info.DataPath, "Managed");

            StringBuilder referencesBuilder = new();
            List<string> files = [.. Directory.GetFiles(dllDir, "*.dll")];
            if (info.MelonVersion <= new Version(0, 5, 3))
                files.Add(Path.Combine(info.Path, "MelonLoader", "MelonLoader.dll"));
            else if (info.MelonVersion <= new Version(0, 5, 7))
            {
                files.Add(Path.Combine(info.Path, "MelonLoader", "MelonLoader.dll"));
                files.Add(Path.Combine(info.Path, "MelonLoader", "0Harmony.dll"));
            }
            else // ML 0.6+
            {
                files.Add(Path.Combine(info.Path, "MelonLoader", info.IsIl2Cpp ? "net6" : "net35", "MelonLoader.dll"));
                files.Add(Path.Combine(info.Path, "MelonLoader", info.IsIl2Cpp ? "net6" : "net35", "0Harmony.dll"));

                if (info.IsIl2Cpp)
                {
                    files.Add(Path.Combine(info.Path, "MelonLoader", "net6", "Il2CppInterop.Runtime.dll"));
                    files.Add(Path.Combine(info.Path, "MelonLoader", "net6", "Il2CppInterop.Common.dll"));
                }
            }

            foreach (string file in files)
            {
                referencesBuilder.AppendLine($"\t\t<Reference Include=\"{Path.GetFileNameWithoutExtension(file)}\">");
                referencesBuilder.AppendLine($"\t\t\t<HintPath>{file}</HintPath>");
                referencesBuilder.AppendLine($"\t\t</Reference>");
            }

            #endregion

            _replacements.Add("$GAME_DIR$", info.Path);
            _replacements.Add("$GAME_DEV$", info.GameDeveloper);
            _replacements.Add("$GAME_NAME$", info.GameName);
            _replacements.Add("$INIT_METHOD_NAME$", info.MelonVersion >= new Version(0, 5, 5) ? "OnInitializeMelon" : "OnApplicationStart");
            _replacements.Add("$AUTHOR$", Environment.UserName);
            _replacements.Add("$FRAMEWORK_VER$", framework);
            _replacements.Add("$PROJ_REFERENCES$", referencesBuilder.ToString());
        }

        private GameInfo TryParseGamePath(string dir)
        {
            GameInfo info = new();
            info.Path = dir;

            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            {
                ThrowError("Game path does not exist.");
                return info;
            }

            var files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);

            string exe = files.FirstOrDefault(f => f.EndsWith(".exe") && !f.Contains("UnityCrashHandler"));

            if (string.IsNullOrWhiteSpace(exe))
            {
                ThrowError("Path does not contain an EXE.");
                info.IsUnityGame = false;
                return info;
            }

            string dataDir = Path.Combine(Path.GetDirectoryName(exe), Path.GetFileNameWithoutExtension(exe) + "_Data");
            if (!Directory.Exists(dataDir))
            {
                ThrowError("Path does not contain a Data folder. It may not be a Unity game.");
                info.IsUnityGame = false;
                return info;
            }

            info.ExePath = exe;
            info.DataPath = dataDir;

            if (!files.Any(f => f.EndsWith("MelonLoader.dll")))
            {
                ThrowError("Game does not have MelonLoader installed.\nInstall and run MelonLoader once before using this wizard.");
                info.HasMelonInstalled = false;
                return info;
            }

            if (files.Any(f => f.EndsWith("global-metadata.dat")))
                info.IsIl2Cpp = true;

            FileVersionInfo fvi = null;

            try
            {
                // it's possible to have multiple MelonLoader.dlls due to 0.6+'s multi-framework stuff
                // though you should never have multiple of different versions so this should be fine
                fvi = FileVersionInfo.GetVersionInfo(files.First(f => f.EndsWith("MelonLoader.dll")));
            }
            catch
            {
                ThrowError("Failed to read MelonLoader DLL. It may be corrupt.");
                info.IsMelonValid = false;
                return info;
            }

            info.MelonVersion = Version.Parse(fvi.FileVersion);
            if (info.MelonVersion < new Version(0, 5, 0))
            {
                ThrowError("The installed MelonLoader version is too old. This wizard only supports MelonLoader 0.5+.");
                info.IsMelonValid = false;
                return info;
            }

            UnityDataParser.Run(info);
            return info;
        }

        private void ThrowError(string status)
        {
            var result = MessageBox.Show(status, "Error", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.Cancel)
                throw new WizardCancelledException();

            _replacements.Clear();
            BeginWizard();
        }

        // These methods are only called for item templates, not for project templates.
        public bool ShouldAddProjectItem(string filePath) => true;
        public void ProjectItemFinishedGenerating(ProjectItem projectItem) { }
    }
}
