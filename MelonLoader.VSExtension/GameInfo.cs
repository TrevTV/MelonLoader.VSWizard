using System;

namespace MelonLoader.VSExtension
{
    public class GameInfo
    {
        private static readonly Version _ml6Version = new Version(0, 6, 0);

        // Compat Check
        public bool IsUnityGame { get; set; } = true;
        public bool HasMelonInstalled { get; set; } = true;
        public bool IsMelonValid { get; set; } = true;

        // Data
        public string Path { get; set; } = "";
        public Version MelonVersion { get; set; } = new Version();
        public bool IsMelon6Plus { get => MelonVersion >= _ml6Version; }
        public bool IsIl2Cpp { get; set; } = false;
    }
}
