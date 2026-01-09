using System;

namespace TooManyItems.Extensions
{
    internal class Integrations
    {
        internal static bool lookingGlassEnabled = false;
        internal static bool properSaveEnabled = false;

        internal static void Init()
        {
            System.Collections.Generic.Dictionary<string, BepInEx.PluginInfo> pluginInfos = BepInEx.Bootstrap.Chainloader.PluginInfos;

            if (pluginInfos.ContainsKey(LookingGlass.PluginInfo.PLUGIN_GUID))
            {
                try
                {
                    Log.Debug("Running code injection for LookingGlass.");
                    LookingGlassIntegration.Init();
                    lookingGlassEnabled = true;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
            if (pluginInfos.ContainsKey(ProperSave.ProperSavePlugin.GUID))
            {
                try
                {
                    Log.Debug("Running code injection for ProperSave.");
                    ProperSaveIntegration.Init();
                    properSaveEnabled = true;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}