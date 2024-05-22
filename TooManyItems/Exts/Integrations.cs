using System;

namespace TooManyItems
{
    internal class Integrations
    {
        internal static bool betterUIEnabled = false;
        internal static bool lookingGlassEnabled = false;

        internal static void Init()
        {
            var pluginInfos = BepInEx.Bootstrap.Chainloader.PluginInfos;
            if (pluginInfos.ContainsKey("com.xoxfaby.BetterUI"))
            {
                try
                {
                    Log.Debug("Better UI enabled.");
                    BetterUIIntegration.Init();
                    betterUIEnabled = true;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
            if (pluginInfos.ContainsKey("droppod.lookingglass"))
            {
                try
                {
                    Log.Debug("Looking Glass enabled.");
                    LookingGlassIntegration.Init();
                    lookingGlassEnabled = true;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}