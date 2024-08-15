using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace MelonLoader.VSExtension
{
    /// <summary>
    /// Interaction logic for WizardDialog.xaml
    /// </summary>
    public partial class WizardDialog : Window
    {
        public GameInfo GameInfo => _gameInfo;
        private readonly GameInfo _gameInfo;

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
            using OpenFileDialog dialog = new();
            dialog.Title = "Select Game Executable";
            dialog.Multiselect = false;
            dialog.Filter = "Unity Executables (*.exe)|*.exe";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                GamePathTextBox.Text = Path.GetDirectoryName(dialog.FileName);
            }
        }

        private void GamePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TryParseGamePath(GamePathTextBox.Text);
        }

        private void TryParseGamePath(string dir)
        {
            UpdateStatus("", true);

            _gameInfo.Path = dir;

            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            {
                UpdateStatus("Game path does not exist.", true);
                return;
            }

            var files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);

            string exe = files.FirstOrDefault(f => f.EndsWith(".exe") && !f.Contains("UnityCrashHandler"));

            if (string.IsNullOrWhiteSpace(exe))
            {
                UpdateStatus("Path does not contain an EXE.", true);
                _gameInfo.IsUnityGame = false;
                return;
            }

            string dataDir = Path.Combine(Path.GetDirectoryName(exe), Path.GetFileNameWithoutExtension(exe) + "_Data");
            if (!Directory.Exists(dataDir))
            {
                UpdateStatus("Path does not contain a Data folder. It may not be a Unity game.", true);
                _gameInfo.IsUnityGame = false;
                return;
            }

            _gameInfo.ExePath = exe;
            _gameInfo.DataPath = dataDir;

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
            if (_gameInfo.MelonVersion < new Version(0, 5, 0))
            {
                UpdateStatus("The installed MelonLoader version is too old. This wizard only supports MelonLoader 0.5+.", true);
                _gameInfo.IsMelonValid = false;
                return;
            }

            UpdateStatus($"MelonVersion = {_gameInfo.MelonVersion}");
            UpdateStatus($"IsMelon6Plus = {_gameInfo.IsMelon6Plus}");

            UnityDataParser.Run(_gameInfo);
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
