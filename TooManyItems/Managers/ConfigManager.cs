using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;

namespace TooManyItems.Managers
{
    public static class ConfigManager
    {
        public static class Scaling
        {
            public static ConfigFile config = new(Paths.ConfigPath + "\\TooManyItems.cfg", true);
            public static string categoryName = "TooManyItems Config";
            public static string categoryGUID = TooManyItems.PluginGUID + "_config";

            // Global configs
            public static ConfigOptions.ConfigurableValue<bool> useCustomValues = ConfigOptions.ConfigurableValue.CreateBool(
                categoryGUID,
                categoryName,
                config,
                "! Important !",
                "Use Custom Config",
                false,
                "Set to true to enable custom configurations."
            );
        }
    }

    public class ConfigurableValue<T> : ConfigOptions.ConfigurableValue<T>
    {
        public ConfigurableValue(string section, string key, float defaultValue, string description = "", List<string> stringsToAffect = null, Action<float> onChanged = null) : base(ConfigManager.Scaling.config, section, key, (T)Convert.ChangeType(defaultValue, typeof(T)), description, stringsToAffect, ConfigManager.Scaling.useCustomValues.bepinexConfigEntry)
        {
            CreateFloat(ConfigManager.Scaling.categoryGUID, ConfigManager.Scaling.categoryName, ConfigManager.Scaling.config, section, key, defaultValue, min: 0f, max: 1000000f, description: description, stringsToAffect: stringsToAffect, useCustomValueConfigEntry: ConfigManager.Scaling.useCustomValues.bepinexConfigEntry, onChanged: onChanged);
        }

        public ConfigurableValue(string section, string key, int defaultValue, string description = "", List<string> stringsToAffect = null, Action<int> onChanged = null) : base(ConfigManager.Scaling.config, section, key, (T)Convert.ChangeType(defaultValue, typeof(T)), description, stringsToAffect, ConfigManager.Scaling.useCustomValues.bepinexConfigEntry)
        {
            CreateInt(ConfigManager.Scaling.categoryGUID, ConfigManager.Scaling.categoryName, ConfigManager.Scaling.config, section, key, defaultValue, min: 0, max: 1000000, description: description, stringsToAffect: stringsToAffect, useCustomValueConfigEntry: ConfigManager.Scaling.useCustomValues.bepinexConfigEntry, onChanged: onChanged);
        }

        public ConfigurableValue(string section, string key, bool defaultValue, string description = "", List<string> stringsToAffect = null, Action<bool> onChanged = null) : base(ConfigManager.Scaling.config, section, key, (T)Convert.ChangeType(defaultValue, typeof(T)), description, stringsToAffect, ConfigManager.Scaling.useCustomValues.bepinexConfigEntry)
        {
            CreateBool(ConfigManager.Scaling.categoryGUID, ConfigManager.Scaling.categoryName, ConfigManager.Scaling.config, section, key, defaultValue, description: description, stringsToAffect: stringsToAffect, useCustomValueConfigEntry: ConfigManager.Scaling.useCustomValues.bepinexConfigEntry, onChanged: onChanged);
        }

        public ConfigurableValue(string section, string key, string defaultValue, string description = "", List<string> stringsToAffect = null, Action<string> onChanged = null) : base(ConfigManager.Scaling.config, section, key, (T)Convert.ChangeType(defaultValue, typeof(T)), description, stringsToAffect, ConfigManager.Scaling.useCustomValues.bepinexConfigEntry)
        {
            CreateString(ConfigManager.Scaling.categoryGUID, ConfigManager.Scaling.categoryName, ConfigManager.Scaling.config, section, key, defaultValue, description: description, stringsToAffect: stringsToAffect, useCustomValueConfigEntry: ConfigManager.Scaling.useCustomValues.bepinexConfigEntry, onChanged: onChanged);
        }
    }
}
