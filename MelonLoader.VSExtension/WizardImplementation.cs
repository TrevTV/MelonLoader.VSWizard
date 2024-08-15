using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace MelonLoader.VSExtension
{
    public class WizardImplementation : IWizard
    {
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
                // TODO: remove the UI, it doesn't look good and we can get it all done with a single folder picker
                WizardDialog inputForm = new();
                inputForm.ShowDialog();

                var info = inputForm.GameInfo;

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
                    files.Add(Path.Combine(info.Path, "MelonLoader", info.IsIl2Cpp ? "net6" : "net35", "Il2CppInterop.Runtime.dll"));
                    files.Add(Path.Combine(info.Path, "MelonLoader", info.IsIl2Cpp ? "net6" : "net35", "Il2CppInterop.Common.dll"));
                }

                foreach (string file in files)
                {
                    referencesBuilder.AppendLine($"\t\t<Reference Include=\"{Path.GetFileNameWithoutExtension(file)}\">");
                    referencesBuilder.AppendLine($"\t\t\t<HintPath>{file}</HintPath>");
                    referencesBuilder.AppendLine($"\t\t</Reference>");
                }

                replacementsDictionary.Add("$GAME_DIR$", info.Path);
                replacementsDictionary.Add("$GAME_DEV$", info.GameDeveloper);
                replacementsDictionary.Add("$GAME_NAME$", info.GameName);
                replacementsDictionary.Add("$AUTHOR$", Environment.UserName);
                replacementsDictionary.Add("$FRAMEWORK_VER$", framework);
                replacementsDictionary.Add("$PROJ_REFERENCES$", referencesBuilder.ToString());

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

                throw new WizardCancelledException();
            }
        }

        // These methods are only called for item templates, not for project templates.
        public bool ShouldAddProjectItem(string filePath) => true;
        public void ProjectItemFinishedGenerating(ProjectItem projectItem) { }
    }
}
