using System;

namespace TooManyItems
{
    internal class Integrations
    {
        internal static bool itemStatsEnabled = false;

        internal static void Init()
        {
            var pluginInfos = BepInEx.Bootstrap.Chainloader.PluginInfos;
            if (pluginInfos.ContainsKey("com.xoxfaby.BetterUI"))
            {
                try
                {
                    BetterUIIntegration.Init();
                    itemStatsEnabled = true;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}