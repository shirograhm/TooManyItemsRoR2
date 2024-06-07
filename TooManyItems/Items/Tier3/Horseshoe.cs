using R2API;
using R2API.Networking;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class Horseshoe
    {
        public static ItemDef itemDef;

        // Gain an assortment of random stat bonuses that reroll every stage.
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
        public static ConfigurableValue<float> healthPerPoint = new(
            "Item: Golden Horseshoe",
            "Health Per Point",
            12f,
            "Max health gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> damagePerPoint = new(
            "Item: Golden Horseshoe",
            "Damage Per Point",
            0.75f,
            "Base damage gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> attackSpeedPerPoint = new(
            "Item: Golden Horseshoe",
            "Attack Speed Per Point",
            4f,
            "Percent attack speed gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> critChancePerPoint = new(
            "Item: Golden Horseshoe",
            "Crit Chance Per Point",
            2f,
            "Percent crit chance gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> critDamagePerPoint = new(
            "Item: Golden Horseshoe",
            "Crit Damage Per Point",
            2f,
            "Percent crit damage gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> armorPerPoint = new(
            "Item: Golden Horseshoe",
            "Armor Per Point",
            2f,
            "Armor gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        
        public static ConfigurableValue<float> regenPerPoint = new(
            "Item: Golden Horseshoe",
            "Regeneration Per Point",
            0.75f,
            "Regeneration gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> shieldPerPoint = new(
            "Item: Golden Horseshoe",
            "Shield Per Point",
            15f,
            "Shield gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> moveSpeedPerPoint = new(
            "Item: Golden Horseshoe",
            "Move Speed Per Point",
            4f,
            "Percent movement speed gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        //public static ConfigurableValue<float> luckPerPoint = new(
        //    "Item: Golden Horseshoe",
        //    "Luck Per Point",
        //    0.025f,
        //    "Luck gained per stat point invested.",
        //    new List<string>()
        //    {
        //        "ITEM_HORSESHOE_DESC"
        //    }
        //);
        public static ConfigurableValue<float> extraStackMultiplier = new(
            "Item: Golden Horseshoe",
            "Increase for Additional Stacks",
            30f,
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

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            NetworkingAPI.RegisterMessageType<HorseshoeStatistics.Sync>();

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "HORSESHOE";
            itemDef.AutoPopulateTokens();

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
                        HorseshoeStatistics component = sender.inventory.GetComponent<HorseshoeStatistics>();
                        if (component)
                        {
                            args.baseHealthAdd += GetScaledValue(component.MaxHealthBonus, sender.level, count);
                            args.baseDamageAdd += GetScaledValue(component.BaseDamageBonus, sender.level, count);
                            args.attackSpeedMultAdd += GetScaledValue(component.AttackSpeedPercentBonus, sender.level, count);
                            args.critAdd += GetScaledValue(component.CritChanceBonus, sender.level, count);
                            args.critDamageMultAdd += GetScaledValue(component.CritDamageBonus, sender.level, count);
                            args.armorAdd += GetScaledValue(component.ArmorBonus, sender.level, count);
                            args.baseRegenAdd += GetScaledValue(component.RegenerationBonus, sender.level, count);
                            args.baseShieldAdd += GetScaledValue(component.ShieldBonus, sender.level, count);
                            args.moveSpeedMultAdd += GetScaledValue(component.MoveSpeedPercentBonus, sender.level, count);
                        }
                    }
                }
            };
        }

        public static float GetScaledValue(float value, float level, int count)
        {
            // Level 1 -> 100%, Level 11 -> 200%, Level 21 -> 300%, Level 31 -> 400%
            float levelScaling = (level + 9) / 10f;
            float extraStackScaling = 1 + extraStackMultiplierPercent * count;

            return value * levelScaling * extraStackScaling;
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
            HorseshoeStatistics component = inventory.GetComponent<HorseshoeStatistics>();
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
                    if (pointsRemaining > step * 2)
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
                            Log.Error("Attempted to boost an invalid stat.");
                            break;
                    }
                    pointsRemaining -= randomPoints;
                }
                Utils.ForceRecalculate(body);
            }
            else
            {
                Log.Error("Unable to reroll Horseshoe statistics.");
            }
        }
    }
}
