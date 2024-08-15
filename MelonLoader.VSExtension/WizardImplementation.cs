using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
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
                WizardDialog inputForm = new WizardDialog();
                inputForm.ShowDialog();

                //replacementsDictionary.Add("$custommessage$", inputForm.GamePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        // These methods are only called for item templates, not for project templates.
        public bool ShouldAddProjectItem(string filePath) => true;
        public void ProjectItemFinishedGenerating(ProjectItem projectItem) { }
    }
}
