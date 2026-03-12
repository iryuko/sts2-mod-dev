using System;
using System.Threading;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Saves;

namespace UnifiedSavePath
{
    [ModInitializer("Initialize")]
    public static class UnifiedSavePathMod
    {
        private static Thread? _flagThread;
        private static volatile bool _keepRunning;

        public static void Initialize()
        {
            Console.WriteLine("[UnifiedSavePath] Initialize start.");
            ForceFlagOnce();

            if (_flagThread != null)
            {
                Console.WriteLine("[UnifiedSavePath] Flag thread already started.");
                return;
            }

            _keepRunning = true;
            _flagThread = new Thread(FlagLoop)
            {
                IsBackground = true,
                Name = "UnifiedSavePathFlagLoop",
            };
            _flagThread.Start();
            Console.WriteLine("[UnifiedSavePath] Started IsRunningModded override thread.");
        }

        private static void FlagLoop()
        {
            var attempts = 0;

            while (_keepRunning)
            {
                ForceFlagOnce();
                attempts++;

                if (attempts < 40)
                {
                    Thread.Sleep(250);
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private static void ForceFlagOnce()
        {
            try
            {
                if (UserDataPathProvider.IsRunningModded)
                {
                    UserDataPathProvider.IsRunningModded = false;
                    Console.WriteLine("[UnifiedSavePath] Reset UserDataPathProvider.IsRunningModded to false.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[UnifiedSavePath] Failed to update IsRunningModded flag: " + ex);
            }
        }
    }
}
