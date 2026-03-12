using System;

namespace SmokeMod
{
    public static class ModEntry
    {
        public const string BuildMarker = "SmokeMod pack-manifest build";

        public static void Initialize()
        {
            Console.WriteLine("[SmokeMod] ModEntry.Initialize invoked.");
        }
    }
}
