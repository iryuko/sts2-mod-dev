using System;
using System.Runtime.InteropServices;
using System.Threading;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Saves;

namespace UnifiedSavePath
{
    [ModInitializer("Initialize")]
    public static class UnifiedSavePathMod
    {
        private static Harmony? _harmony;
        private static Thread? _flagThread;
        private static volatile bool _keepRunning;

        public static void Initialize()
        {
            Console.WriteLine("[UnifiedSavePath] Initialize start.");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                InitializeWindowsPatchMode();
                return;
            }

            InitializeFallbackFlagMode("non-Windows platform");
        }

        private static void InitializeWindowsPatchMode()
        {
            try
            {
                ForceFlagOnce();

                if (_harmony != null)
                {
                    Console.WriteLine("[UnifiedSavePath] Harmony patch mode already initialized.");
                    return;
                }

                _harmony = new Harmony("sts2.unifiedsavepath.crossplatform");
                _harmony.PatchAll(typeof(UnifiedSavePathMod).Assembly);
                Console.WriteLine("[UnifiedSavePath] Applied Harmony patch mode for Windows.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[UnifiedSavePath] Harmony patch mode failed, falling back to flag thread: " + ex);
                InitializeFallbackFlagMode("Harmony patch failure");
            }
        }

        private static void InitializeFallbackFlagMode(string reason)
        {
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
            Console.WriteLine("[UnifiedSavePath] Started IsRunningModded override thread (" + reason + ").");
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

        [HarmonyPatch(typeof(UserDataPathProvider), "get_IsRunningModded")]
        private static class PatchGetIsRunningModded
        {
            private static bool Prefix(ref bool __result)
            {
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(UserDataPathProvider), "set_IsRunningModded")]
        private static class PatchSetIsRunningModded
        {
            private static bool Prefix()
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(UserDataPathProvider), nameof(UserDataPathProvider.GetProfileDir))]
        private static class PatchGetProfileDir
        {
            private static bool Prefix(int profile, ref string __result)
            {
                __result = $"profile{profile}";
                return false;
            }
        }
    }
}
