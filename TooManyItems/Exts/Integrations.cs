using System;

namespace TooManyItems
{
    internal class Integrations
    {
        internal static bool lookingGlassEnabled = false;

        internal static void Init()
        {
            var pluginInfos = BepInEx.Bootstrap.Chainloader.PluginInfos;
            if (pluginInfos.ContainsKey("droppod.lookingglass"))
            {
                try
                {
                    Log.Debug("LookingGlass detected, running integrations for TooManyItems.");
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