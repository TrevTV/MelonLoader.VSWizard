using AssetRipper.Primitives;
using System;

namespace MelonLoader.WizardExtension
{
    public class GameInfo
    {
        private static readonly Version _ml6Version = new(0, 6, 0);

        // Compat Check
        public bool IsUnityGame { get; set; } = true;
        public bool HasMelonInstalled { get; set; } = true;
        public bool IsMelonValid { get; set; } = true;

        // Data
        public string Path { get; set; } = "";
        public string ExePath { get; set; } = "";
        public string DataPath { get; set; } = "";
        public Version MelonVersion { get; set; } = new();
        public bool IsMelon6Plus { get => MelonVersion >= _ml6Version; }
        public bool IsIl2Cpp { get; set; } = false;

        public string GameName { get; set; } = "null";
        public string GameDeveloper { get; set; } = "null";
        public UnityVersion EngineVersion { get; set; } = UnityVersion.MinVersion;
    }
}
