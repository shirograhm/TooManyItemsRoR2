using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class HolyWater
    {
        public static ItemDef itemDef;

        // Killing an elite enemy grants all allies a portion of its max health as bonus experience, scaling with difficulty.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Holy Water",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_HOLYWATER_DESC"
            }
        );
        public static ConfigurableValue<float> minExperienceMultiplierPerStack = new(
            "Item: Holy Water",
            "Minimum XP",
            1f,
            "Minimum enemy max health converted to bonus experience when killing an elite.",
            new List<string>()
            {
                "ITEM_HOLYWATER_DESC"
            }
        );
        public static float minExperienceMultiplierAsPercent = minExperienceMultiplierPerStack.Value / 100f;

        public static ConfigurableValue<float> maxExperienceMultiplierPerStack = new(
            "Item: Holy Water",
            "Maximum XP",
            100f,
            "Maximum enemy max health converted to bonus experience when killing an elite.",
            new List<string>()
            {
                "ITEM_HOLYWATER_DESC"
            }
        );
        public static float maxExperienceMultiplierAsPercent = maxExperienceMultiplierPerStack.Value / 100f;

        public static ConfigurableValue<float> extraStacksMultiplier = new(
            "Item: Holy Water",
            "Extra Stack Scaling",
            50f,
            "Experience bonus for additional stacks.",
            new List<string>()
            {
                "ITEM_HOLYWATER_DESC"
            }
        );
        public static float extraStacksMultiplierPercent = extraStacksMultiplier.Value / 100f;

        internal static void Init()
        {
            GenerateItem();

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "HOLYWATER";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier2);

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("HolyWater.png");
            itemDef.pickupModelPrefab = AssetHandler.bundle.LoadAsset<GameObject>("HolyWater.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility,

                ItemTag.CanBeTemporary
            };
        }

        public static void Hooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += (damageReport) =>
            {
                if (!NetworkServer.active) return;

                CharacterMaster atkMaster = damageReport.attackerMaster;
                CharacterBody atkBody = damageReport.attackerBody;
                CharacterBody vicBody = damageReport.victimBody;
                if (atkMaster && vicBody && vicBody.isElite)
                {
                    atkBody = Utils.GetMinionOwnershipParentBody(atkBody);
                    if (atkBody && atkBody.inventory)
                    {
                        int count = atkBody.inventory.GetItemCountEffective(itemDef);
                        if (count > 0)
                        {
                            float bonusXP = vicBody.healthComponent.fullCombinedHealth * CalculateExperienceMultiplier(count);

                            atkMaster.GiveExperience(Convert.ToUInt64(bonusXP));
                        }
                    }
                }
            };
        }

        public static float CalculateExperienceMultiplier(int itemCount)
        {
            float difference = maxExperienceMultiplierAsPercent - minExperienceMultiplierAsPercent;
            float multiplier = minExperienceMultiplierAsPercent + difference * Utils.GetDifficultyAsPercentage();

            return multiplier * (1 + extraStacksMultiplierPercent * (itemCount - 1));
        }
    }
}
