namespace GHelper
{
    /// <summary>
    /// Linux-specific configuration stub
    /// </summary>
    public static class AppConfig
    {
        public static string GetModel()
        {
            return "Linux ASUS Device";
        }

        public static bool IsASUS()
        {
            return true;
        }

        public static string GetString(string key)
        {
            // Placeholder - would be implemented with proper config storage
            return "";
        }

        public static int Get(string key)
        {
            // Placeholder - would be implemented with proper config storage
            return 0;
        }

        public static void Set(string key, int value)
        {
            // Placeholder - would be implemented with proper config storage
        }

        public static bool Is(string key)
        {
            // Placeholder - would be implemented with proper config storage
            return false;
        }

        public static bool IsZ13()
        {
            return false;
        }

        public static bool IsAlly()
        {
            return false;
        }
    }
}