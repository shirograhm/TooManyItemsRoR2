using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TooManyItems.Managers
{
    public static class ConfigOptions
    {
        internal static void Init()
        {
            On.RoR2.UI.LogBook.LogBookController.Awake += LogBookController_Awake;
            On.RoR2.Language.GetLocalizedStringByToken += Language_GetLocalizedStringByToken;
        }

        private static bool reloadLogbook = false;
        private static void LogBookController_Awake(On.RoR2.UI.LogBook.LogBookController.orig_Awake orig, RoR2.UI.LogBook.LogBookController self)
        {
            orig(self);
            if (reloadLogbook)
            {
                reloadLogbook = false;
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                RoR2.UI.LogBook.LogBookController.BuildStaticData();
#pragma warning restore Publicizer001 // Accessing a member that was not originally public
            }
        }

        private static string Language_GetLocalizedStringByToken(On.RoR2.Language.orig_GetLocalizedStringByToken orig, RoR2.Language self, string token)
        {
            string result = orig(self, token);
            foreach (ConfigurableValue configurableValue in ConfigurableValue.instancesList.FindAll(x => x.stringsToAffect.Contains(token)))
            {
                result = result.Replace("{" + configurableValue.key + "}", configurableValue.ToString());
            }
            return result;
        }

        public abstract class ConfigurableValue
        {
            public static List<ConfigurableValue> instancesList = new();

            public List<string> stringsToAffect = new();
            public string key = "";
            public string id = "";

            public static ConfigurableValue<T> Create<T>(ConfigFile configFile, string section, string key, T defaultValue, string description = "", List<string> stringsToAffect = null, ConfigEntry<bool> useCustomValueConfigEntry = null, bool restartRequired = false, Action<T> onChanged = null)
            {
                return new ConfigurableValue<T>(configFile, section, key, defaultValue, description, stringsToAffect, useCustomValueConfigEntry, restartRequired, onChanged);
            }

            public static ConfigurableValue<int> CreateInt(string modGUID, string modName, ConfigFile configFile, string section, string key, int defaultValue, int min = 0, int max = 1000, string description = "", List<string> stringsToAffect = null, ConfigEntry<bool> useCustomValueConfigEntry = null, bool restartRequired = false, Action<int> onChanged = null)
            {
                ConfigurableValue<int> configurableValue = Create(configFile, section, key, defaultValue, description, stringsToAffect, useCustomValueConfigEntry, restartRequired, onChanged);

                return configurableValue;
            }

            public static ConfigurableValue<float> CreateFloat(string modGUID, string modName, ConfigFile configFile, string section, string key, float defaultValue, float min = 0, float max = 1000, string description = "", List<string> stringsToAffect = null, ConfigEntry<bool> useCustomValueConfigEntry = null, bool restartRequired = false, Action<float> onChanged = null)
            {
                ConfigurableValue<float> configurableValue = Create(configFile, section, key, defaultValue, description, stringsToAffect, useCustomValueConfigEntry, restartRequired, onChanged);

                return configurableValue;
            }

            public static ConfigurableValue<bool> CreateBool(string modGUID, string modName, ConfigFile configFile, string section, string key, bool defaultValue, string description = "", List<string> stringsToAffect = null, ConfigEntry<bool> useCustomValueConfigEntry = null, bool restartRequired = false, Action<bool> onChanged = null)
            {
                ConfigurableValue<bool> configurableValue = Create(configFile, section, key, defaultValue, description, stringsToAffect, useCustomValueConfigEntry, restartRequired, onChanged);

                return configurableValue;
            }

            public static ConfigurableValue<string> CreateString(string modGUID, string modName, ConfigFile configFile, string section, string key, string defaultValue, string description = "", List<string> stringsToAffect = null, ConfigEntry<bool> useCustomValueConfigEntry = null, bool restartRequired = false, Action<string> onChanged = null)
            {
                ConfigurableValue<string> configurableValue = Create(configFile, section, key, defaultValue, description, stringsToAffect, useCustomValueConfigEntry, restartRequired, onChanged);

                return configurableValue;
            }
        }

        public class ConfigurableValue<T> : ConfigurableValue
        {
            public ConfigEntry<T> bepinexConfigEntry;
            private ConfigEntry<bool> useCustomValueConfigEntry;
            private T defaultValue;

            public ConfigurableValue(ConfigFile configFile, string section, string key, T defaultValue, string description = "", List<string> stringsToAffect = null, ConfigEntry<bool> useCustomValueConfigEntry = null, bool restartRequired = false, Action<T> onChanged = null)
            {
                id = System.IO.Path.GetFileNameWithoutExtension(configFile.ConfigFilePath) + "." + section + "." + key;
                ConfigurableValue existing = instancesList.FirstOrDefault(x => x.id == id);
                if (existing != null)
                {
                    ConfigurableValue<T> existingCast = existing as ConfigurableValue<T>;
                    bepinexConfigEntry = existingCast.bepinexConfigEntry;
                    this.useCustomValueConfigEntry = useCustomValueConfigEntry;
                }
                else
                {
                    bepinexConfigEntry = configFile.Bind(section, key, defaultValue, description);
                    instancesList.Add(this);
                }

                this.useCustomValueConfigEntry = useCustomValueConfigEntry;
                this.key = key;
                this.defaultValue = defaultValue;
                if (stringsToAffect != null) this.stringsToAffect = stringsToAffect;
                else this.stringsToAffect = new List<string>();

                if (onChanged != null)
                {
                    bepinexConfigEntry.SettingChanged += (x, y) =>
                    {
                        onChanged(bepinexConfigEntry.Value);
                        reloadLogbook = true;
                    };
                    onChanged(bepinexConfigEntry.Value);
                    reloadLogbook = true;
                }
            }

            public T Value
            {
                get
                {
                    if (useCustomValueConfigEntry != null && useCustomValueConfigEntry.Value) return bepinexConfigEntry.Value;
                    return defaultValue;
                }
            }

            public override string ToString()
            {
                return Convert.ToString(Value, System.Globalization.CultureInfo.InvariantCulture);
            }

            public static implicit operator T(ConfigurableValue<T> configurableValue)
            {
                return configurableValue.Value;
            }
        }
    }
}