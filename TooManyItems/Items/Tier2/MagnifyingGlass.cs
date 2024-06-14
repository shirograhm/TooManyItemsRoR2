using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class MagnifyingGlass
    {
        public static ItemDef itemDef;
        public static BuffDef analyzedDebuff;

        // Gain crit chance. Critical strikes have a chance to Analyze the enemy, increasing their damage taken from all sources.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Magnifying Glass",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_MAGNIFYINGGLASS_DESC"
            }
        );
        public static ConfigurableValue<float> critBonus = new(
            "Item: Magnifying Glass",
            "Crit Chance",
            5f,
            "Crit chance increase so that the item isn't worthless without other crit items.",
            new List<string>()
            {
                "ITEM_MAGNIFYINGGLASS_DESC"
            }
        );
        public static ConfigurableValue<float> analyzeChance = new(
            "Item: Magnifying Glass",
            "Analyze Chance",
            6f,
            "Percent chance to Analyze an enemy on crit.",
            new List<string>()
            {
                "ITEM_MAGNIFYINGGLASS_DESC"
            }
        );
        public static ConfigurableValue<float> damageTakenBonus = new(
            "Item: Magnifying Glass",
            "Damage Taken Bonus",
            18f,
            "Percent damage taken bonus once Analyzed.",
            new List<string>()
            {
                "ITEM_MAGNIFYINGGLASS_DESC"
            }
        );
        public static float analyzeChancePercent = analyzeChance.Value / 100f;
        public static float damageTakenBonusPercent = damageTakenBonus.Value / 100f;

        internal static void Init()
        {
            GenerateItem();
            GenerateBuff();

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            ContentAddition.AddBuffDef(analyzedDebuff);

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "MAGNIFYINGGLASS";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier2);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("MagnifyingGlass.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("MagnifyingGlass.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage
            };
        }

        private static void GenerateBuff()
        {
            analyzedDebuff = ScriptableObject.CreateInstance<BuffDef>();

            analyzedDebuff.name = "Analyzed";
            analyzedDebuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Analyzed.png");
            analyzedDebuff.canStack = false;
            analyzedDebuff.isHidden = false;
            analyzedDebuff.isDebuff = true;
            analyzedDebuff.isCooldown = false;
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        args.critAdd += critBonus.Value;
                    }
                }
            };

            GenericGameEvents.OnTakeDamage += (damageReport) =>
            {
                CharacterBody atkBody = damageReport.attackerBody;
                CharacterBody vicBody = damageReport.victimBody;
                CharacterMaster atkMaster = damageReport.attackerMaster;
                if (atkBody && vicBody && atkMaster)
                {
                    int count = atkBody.inventory.GetItemCount(itemDef);
                    if (count > 0 && damageReport.damageInfo.crit)
                    {
                        if (Util.CheckRoll(analyzeChance.Value * count, atkMaster.luck, atkMaster))
                        {
                            vicBody.AddBuff(analyzedDebuff);
                        }
                    }
                }
            };

            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (victimInfo.body && victimInfo.body.HasBuff(analyzedDebuff))
                {
                    damageInfo.damage *= 1 + damageTakenBonusPercent;
                }
            };
        }
    }
}
