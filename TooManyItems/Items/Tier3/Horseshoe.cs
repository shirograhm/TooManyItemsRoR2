using R2API;
using R2API.Networking;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class Horseshoe
    {
        public static ItemDef itemDef;

        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Golden Horseshoe",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> totalPointsCap = new(
            "Item: Golden Horseshoe",
            "Stat Points Cap",
            20f,
            "Max value of stat points a reroll can have. See following configs for scalings.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> damagePerPoint = new(
            "Item: Golden Horseshoe",
            "Damage Per Point",
            0.5f,
            "Base damage gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> attackSpeedPerPoint = new(
            "Item: Golden Horseshoe",
            "Attack Speed Per Point",
            3f,
            "Percent attack speed gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> moveSpeedPerPoint = new(
            "Item: Golden Horseshoe",
            "Move Speed Per Point",
            3f,
            "Percent movement speed gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> critChancePerPoint = new(
            "Item: Golden Horseshoe",
            "Crit Chance Per Point",
            1.5f,
            "Percent crit chance gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> critDamagePerPoint = new(
            "Item: Golden Horseshoe",
            "Crit Damage Per Point",
            3f,
            "Percent crit damage gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> healthPerPoint = new(
            "Item: Golden Horseshoe",
            "Health Per Point",
            8f,
            "Max health gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> regenPerPoint = new(
            "Item: Golden Horseshoe",
            "Regeneration Per Point",
            0.8f,
            "Regeneration gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> shieldPerPoint = new(
            "Item: Golden Horseshoe",
            "Shield Per Point",
            10f,
            "Shield gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> armorPerPoint = new(
            "Item: Golden Horseshoe",
            "Armor Per Point",
            2.5f,
            "Armor gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<int> extraStackMultiplier = new(
            "Item: Golden Horseshoe",
            "Increase for Additional Stacks",
            30,
            "Percent increase to all bonuses given for each additional stack.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static float extraStackMultiplierPercent = extraStackMultiplier.Value / 100f;

        public enum Bonuses
        {
            HEALTH,
            DAMAGE,
            ATTACK_SPEED,
            CRIT_CHANCE,
            CRIT_DAMAGE,
            ARMOR,
            HEALTH_REGEN,
            SHIELD,
            MOVEMENT_SPEED,

            NUM_STATS
        }

        internal static void Init()
        {
            GenerateItem();
            AddTokens();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            NetworkingAPI.RegisterMessageType<HorseshoeStatistics.Sync>();

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "HORSESHOE";
            itemDef.nameToken = "HORSESHOE_NAME";
            itemDef.pickupToken = "HORSESHOE_PICKUP";
            itemDef.descriptionToken = "HORSESHOE_DESCRIPTION";
            itemDef.loreToken = "HORSESHOE_LORE";

            Utils.SetItemTier(itemDef, ItemTier.Tier3);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Horseshoe.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Horseshoe.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.AIBlacklist,

                ItemTag.Utility,
                ItemTag.Damage,
                ItemTag.Healing
            };
        }

        public static void Hooks()
        {
            CharacterMaster.onStartGlobal += (obj) =>
            {
                obj.inventory?.gameObject.AddComponent<HorseshoeStatistics>();
            };

            Stage.onStageStartGlobal += (stage) =>
            {
                foreach (NetworkUser user in NetworkUser.readOnlyInstancesList)
                {
                    CharacterMaster master = user.masterController.master;
                    if (master)
                    {
                        if (master.inventory && master.inventory.GetItemCount(itemDef) > 0)
                        {
                            Reroll(master.inventory, master.GetBody());
                        }
                    }
                }
            };

            On.RoR2.Inventory.GiveItem_ItemIndex_int += (orig, self, index, count) =>
            {
                HorseshoeStatistics component = self.GetComponent<HorseshoeStatistics>();
                CharacterMaster master = self.GetComponent<CharacterMaster>();

                if (component && master && index == itemDef.itemIndex)
                {
                    // Check if rolled yet
                    if (HasNotRolledYet(component) && master.GetBody())
                    {
                        Reroll(self, master.GetBody());
                    }
                }
                orig(self, index, count);
            };

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        var component = sender.inventory.GetComponent<HorseshoeStatistics>();
                        if (component)
                        {
                            Dictionary<Bonuses, float> values = GetScaledValues(component, sender.level, count);

                            float temp = 0;
                            if (!values.TryGetValue(Bonuses.HEALTH, out temp)) Log.Error("Unable to set Horseshoe bonus for max health.");
                            args.baseHealthAdd += temp;
                            if (!values.TryGetValue(Bonuses.DAMAGE, out temp)) Log.Error("Unable to set Horseshoe bonus for damage.");
                            args.baseDamageAdd += temp;
                            if (!values.TryGetValue(Bonuses.ATTACK_SPEED, out temp)) Log.Error("Unable to set Horseshoe bonus for attack speed.");
                            args.attackSpeedMultAdd += temp;
                            if (!values.TryGetValue(Bonuses.CRIT_CHANCE, out temp)) Log.Error("Unable to set Horseshoe bonus for crit chance.");
                            args.critAdd += temp;
                            if (!values.TryGetValue(Bonuses.CRIT_DAMAGE, out temp)) Log.Error("Unable to set Horseshoe bonus for crit damage.");
                            args.critDamageMultAdd += temp;
                            if (!values.TryGetValue(Bonuses.ARMOR, out temp)) Log.Error("Unable to set Horseshoe bonus for armor.");
                            args.armorAdd += temp;
                            if (!values.TryGetValue(Bonuses.HEALTH_REGEN, out temp)) Log.Error("Unable to set Horseshoe bonus for health regen.");
                            args.baseRegenAdd += temp;
                            if (!values.TryGetValue(Bonuses.SHIELD, out temp)) Log.Error("Unable to set Horseshoe bonus for shield.");
                            args.baseShieldAdd += temp;
                            if (!values.TryGetValue(Bonuses.MOVEMENT_SPEED, out temp)) Log.Error("Unable to set Horseshoe bonus for movement speed.");
                            args.moveSpeedMultAdd += temp;
                        }
                    }
                }
            };
        }

        public static Dictionary<Bonuses, float> GetScaledValues(HorseshoeStatistics component, float level, int count)
        {
            // Level 1 -> 100%, Level 11 -> 200%, Level 21 -> 300%, Level 31 -> 400%
            float levelScaling = (level + 9) / 10f;
            float extraStackScaling = 1 + extraStackMultiplierPercent * count;

            return new Dictionary<Bonuses, float>
            {
                { Bonuses.HEALTH, component.MaxHealthBonus * levelScaling * extraStackScaling },
                { Bonuses.DAMAGE, component.BaseDamageBonus * levelScaling * extraStackScaling },
                { Bonuses.ATTACK_SPEED, component.AttackSpeedPercentBonus * levelScaling * extraStackScaling },
                { Bonuses.CRIT_CHANCE, component.CritChanceBonus * levelScaling * extraStackScaling },
                { Bonuses.CRIT_DAMAGE, component.CritDamageBonus * levelScaling * extraStackScaling },
                { Bonuses.ARMOR, component.ArmorBonus * levelScaling * extraStackScaling },
                { Bonuses.HEALTH_REGEN, component.RegenerationBonus * levelScaling * extraStackScaling },
                { Bonuses.SHIELD, component.ShieldBonus * levelScaling * extraStackScaling },
                { Bonuses.MOVEMENT_SPEED, component.MoveSpeedPercentBonus * levelScaling * extraStackScaling }
            };
        }

        public static bool HasNotRolledYet(HorseshoeStatistics bonuses)
        {
            if (bonuses.MaxHealthBonus != 0) return false;
            if (bonuses.BaseDamageBonus != 0) return false;
            if (bonuses.AttackSpeedPercentBonus != 0) return false;
            if (bonuses.CritChanceBonus != 0) return false;
            if (bonuses.CritDamageBonus != 0) return false;
            if (bonuses.ArmorBonus != 0) return false;
            if (bonuses.RegenerationBonus != 0) return false;
            if (bonuses.ShieldBonus != 0) return false;
            if (bonuses.MoveSpeedPercentBonus != 0) return false;

            return true;
        }

        public static void Reroll(Inventory inventory, CharacterBody body)
        {
            var component = inventory.GetComponent<HorseshoeStatistics>();
            if (component)
            {
                component.MaxHealthBonus = 0;
                component.BaseDamageBonus = 0;
                component.AttackSpeedPercentBonus = 0;
                component.CritChanceBonus = 0;
                component.CritDamageBonus = 0;
                component.ArmorBonus = 0;
                component.RegenerationBonus = 0;
                component.ShieldBonus = 0;
                component.MoveSpeedPercentBonus = 0;

                float pointsRemaining = Horseshoe.totalPointsCap.Value;
                while (pointsRemaining > 0)
                {
                    float randomPoints;
                    float step = 1.8f;
                    if (pointsRemaining > step)
                        randomPoints = UnityEngine.Random.Range(step, step * 2);
                    else
                        randomPoints = pointsRemaining;

                    Bonuses chosenStat = (Bonuses)UnityEngine.Random.Range(0, (int)Bonuses.NUM_STATS);
                    switch (chosenStat)
                    {
                        case Bonuses.HEALTH:
                            component.MaxHealthBonus += randomPoints * Horseshoe.healthPerPoint.Value;
                            break;
                        case Bonuses.DAMAGE:
                            component.BaseDamageBonus += randomPoints * Horseshoe.damagePerPoint.Value;
                            break;
                        case Bonuses.ATTACK_SPEED:
                            component.AttackSpeedPercentBonus += randomPoints * Horseshoe.attackSpeedPerPoint.Value / 100f;
                            break;
                        case Bonuses.CRIT_CHANCE:
                            component.CritChanceBonus += randomPoints * Horseshoe.critChancePerPoint.Value;
                            break;
                        case Bonuses.CRIT_DAMAGE:
                            component.CritDamageBonus += randomPoints * Horseshoe.critDamagePerPoint.Value / 100f;
                            break;
                        case Bonuses.ARMOR:
                            component.ArmorBonus += randomPoints * Horseshoe.armorPerPoint.Value;
                            break;
                        case Bonuses.HEALTH_REGEN:
                            component.RegenerationBonus += randomPoints * Horseshoe.regenPerPoint.Value;
                            break;
                        case Bonuses.SHIELD:
                            component.ShieldBonus += randomPoints * Horseshoe.shieldPerPoint.Value;
                            break;
                        case Bonuses.MOVEMENT_SPEED:
                            component.MoveSpeedPercentBonus += randomPoints * Horseshoe.moveSpeedPerPoint.Value / 100f;
                            break;
                        default:
                            Log.Error("Attempted to boost an invalid stat.\n");
                            break;
                    }

                    pointsRemaining -= randomPoints;
                }
            }
            Utils.ForceRecalculate(body);
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("HORSESHOE", "Golden Horseshoe");
            LanguageAPI.Add("HORSESHOE_NAME", "Golden Horseshoe");
            LanguageAPI.Add("HORSESHOE_PICKUP", "Gain a random assortment of stat bonuses that are <style=cWorldEvent>rerolled</style> every stage.");

            string desc = $"Gain a random assortment of stat bonuses that are <style=cWorldEvent>rerolled</style> upon entering a new stage. " +
                $"These bonuses scale with <style=cIsUtility>level</style>, and each <style=cStack>additional stack</style> increases all bonuses by <style=cIsUtility>{extraStackMultiplier.Value}%</style>.";
            LanguageAPI.Add("HORSESHOE_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("HORSESHOE_LORE", lore);
        }
    }
}

// Styles
// <style=cIsHealth>" + exampleValue + "</style>
// <style=cIsDamage>" + exampleValue + "</style>
// <style=cIsHealing>" + exampleValue + "</style>
// <style=cIsUtility>" + exampleValue + "</style>
// <style=cIsVoid>" + exampleValue + "</style>
// <style=cHumanObjective>" + exampleValue + "</style>
// <style=cLunarObjective>" + exampleValue + "</style>
// <style=cStack>" + exampleValue + "</style>
// <style=cWorldEvent>" + exampleValue + "</style>
// <style=cArtifact>" + exampleValue + "</style>
// <style=cUserSetting>" + exampleValue + "</style>
// <style=cDeath>" + exampleValue + "</style>
// <style=cSub>" + exampleValue + "</style>
// <style=cMono>" + exampleValue + "</style>
// <style=cShrine>" + exampleValue + "</style>
// <style=cEvent>" + exampleValue + "</style>
