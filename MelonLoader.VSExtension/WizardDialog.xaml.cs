using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace MelonLoader.VSExtension
{
    /// <summary>
    /// Interaction logic for WizardDialog.xaml
    /// </summary>
    public partial class WizardDialog : Window
    {
        public GameInfo GameInfo => _gameInfo;
        private GameInfo _gameInfo;

        private bool _wasLastStatusReset = true;

        public WizardDialog()
        {
            InitializeComponent();
            _gameInfo = new GameInfo();
        }

        private void OnClickContinue(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnClickFolderPicker(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog();
            var result = openFileDlg.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                GamePathTextBox.Text = openFileDlg.SelectedPath;
            }
        }

        private void GamePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateStatus("", true);

            string dir = GamePathTextBox.Text;
            _gameInfo.Path = dir;

            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            {
                UpdateStatus("Game path does not exist.", true);
                return;
            }

            var files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
            
            // TODO: not perfect. LiS:BtS doesnt have the player dll
            if (!files.Any(f => f.EndsWith("UnityPlayer.dll")))
            {
                UpdateStatus("Path is not a Unity game.", true);
                _gameInfo.IsUnityGame = false;
                return;
            }

            if (!files.Any(f => f.EndsWith("MelonLoader.dll")))
            {
                UpdateStatus("Game does not have MelonLoader installed.\nInstall and run MelonLoader once before using this wizard.", true);
                _gameInfo.HasMelonInstalled = false;
                return;
            }

            if (files.Any(f => f.EndsWith("global-metadata.dat")))
                _gameInfo.IsIl2Cpp = true;

            UpdateStatus($"IsIl2Cpp = {_gameInfo.IsIl2Cpp}");

            FileVersionInfo fvi = null;

            try
            {
                // it's possible to have multiple MelonLoader.dlls due to 0.6+'s multi-framework stuff
                // though you should never have multiple of different versions so this should be fine
                fvi = FileVersionInfo.GetVersionInfo(files.First(f => f.EndsWith("MelonLoader.dll")));
            }
            catch
            {
                UpdateStatus("Failed to read MelonLoader DLL. It may be corrupt.", true);
                _gameInfo.IsMelonValid = false;
                return;
            }

            _gameInfo.MelonVersion = Version.Parse(fvi.FileVersion);

            UpdateStatus($"MelonVersion = {_gameInfo.MelonVersion}");
            UpdateStatus($"IsMelon6Plus = {_gameInfo.IsMelon6Plus}");
            
            // TODO: do something with this
        }

        private void UpdateStatus(string status, bool reset = false)
        {
            if (reset || _wasLastStatusReset)
            {
                StatusBlock.Text = status;
                _wasLastStatusReset = reset;
            }
            else
            {
                StatusBlock.Text += '\n';
                StatusBlock.Text += status;
            }
        }
    }
}
