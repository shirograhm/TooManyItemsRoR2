using R2API;
using RoR2;
using System.Collections.Generic;
using TooManyItems.Managers;
using UnityEngine;

namespace TooManyItems.Items.Tier2
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

            Utilities.SetItemTier(itemDef, ItemTier.Tier2);

            GameObject prefab = AssetManager.bundle.LoadAsset<GameObject>("MagnifyingGlass.prefab");
            ModelPanelParameters modelPanelParameters = prefab.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = prefab.transform;
            modelPanelParameters.cameraPositionTransform = prefab.transform;
            modelPanelParameters.maxDistance = 10f;
            modelPanelParameters.minDistance = 5f;

            itemDef.pickupIconSprite = AssetManager.bundle.LoadAsset<Sprite>("MagnifyingGlass.png");
            itemDef.pickupModelPrefab = prefab;
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage,

                ItemTag.CanBeTemporary
            };
        }

        private static void GenerateBuff()
        {
            analyzedDebuff = ScriptableObject.CreateInstance<BuffDef>();

            analyzedDebuff.name = "Analyzed";
            analyzedDebuff.iconSprite = AssetManager.bundle.LoadAsset<Sprite>("Analyzed.png");
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
                    int count = sender.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        args.critAdd += critBonus.Value;
                    }
                }
            };

            GameEventManager.OnTakeDamage += (damageReport) =>
            {
                CharacterBody atkBody = damageReport.attackerBody;
                CharacterBody vicBody = damageReport.victimBody;
                CharacterMaster atkMaster = damageReport.attackerMaster;
                if (atkBody && vicBody && atkMaster && atkBody.inventory)
                {
                    int count = atkBody.inventory.GetItemCountEffective(itemDef);
                    if (count > 0 && damageReport.damageInfo.crit)
                    {
                        if (Util.CheckRoll(analyzeChance.Value * damageReport.damageInfo.procCoefficient, atkMaster.luck, atkMaster))
                        {
                            vicBody.AddBuff(analyzedDebuff);
                        }
                    }
                }
            };

            GameEventManager.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                CharacterBody atkBody = attackerInfo.body;
                CharacterBody vicBody = victimInfo.body;
                if (atkBody && atkBody.inventory && vicBody && vicBody.HasBuff(analyzedDebuff))
                {
                    damageInfo.damage *= 1f + damageTakenBonusPercent * atkBody.inventory.GetItemCountEffective(itemDef);
                }
            };
        }
    }
}
