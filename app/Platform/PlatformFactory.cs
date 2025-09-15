using System.Runtime.InteropServices;

namespace GHelper.Platform
{
    /// <summary>
    /// Factory for creating platform-specific adapters
    /// </summary>
    public static class PlatformFactory
    {
        private static IPlatformAdapter? _instance;

        /// <summary>
        /// Get the appropriate platform adapter for the current OS
        /// </summary>
        public static IPlatformAdapter GetPlatformAdapter()
        {
            if (_instance != null)
                return _instance;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
#if NET8_0_WINDOWS
                _instance = new WindowsPlatformAdapter();
#else
                throw new PlatformNotSupportedException("Windows platform adapter not available in this build");
#endif
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _instance = new LinuxPlatformAdapter();
            }
            else
            {
                throw new PlatformNotSupportedException($"Platform {RuntimeInformation.OSDescription} is not supported");
            }

            return _instance;
        }

        /// <summary>
        /// Initialize the platform adapter
        /// </summary>
        public static bool Initialize()
        {
            var adapter = GetPlatformAdapter();
            return adapter.Initialize();
        }
    }
}