namespace Snow.Extensions
{
    using System;

    public static class DebugHelperExtensions
    {
        private static bool _debugIsEnabled;

        public static void EnableDebugging()
        {
            _debugIsEnabled = true;
        }

        public static void WaitForContinue()
        {
            if (_debugIsEnabled)
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        public static void OutputIfDebug(this string value, string prefixWith = "")
        {
            if (_debugIsEnabled)
            {
                Console.WriteLine(prefixWith + value);
            }
        }
    }
}