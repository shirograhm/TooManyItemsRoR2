using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class Crucifix
    {
        public static ItemDef itemDef;

        // Reduce damage taken. Taking damage set you on fire.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Crucifix",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_CRUCIFIX_DESC"
            }
        );
        public static ConfigurableValue<float> damageReduction = new(
            "Item: Crucifix",
            "Damage Reduction",
            100f,
            "Percentage of damage reduced.",
            new List<string>()
            {
                "ITEM_CRUCIFIX_DESC"
            }
        );
        public static ConfigurableValue<float> maxHealthBurnAmount = new(
            "Item: Crucifix",
            "Burn Amount",
            40f,
            "Percentage of max health taken over the duration of the burn.",
            new List<string>()
            {
                "ITEM_CRUCIFIX_DESC"
            }
        );
        public static ConfigurableValue<float> maxHealthBurnAmountReduction = new(
            "Item: Crucifix",
            "Burn Amount Reduction",
            8f,
            "Percentage of burn damage reduced per stack of this item. This scales hyperbolically.",
            new List<string>()
            {
                "ITEM_CRUCIFIX_DESC"
            }
        );
        public static ConfigurableValue<bool> isCrucifixBurnStackable = new(
            "Item: Crucifix",
            "Is Burn Stackable",
            false,
            "Whether or not the burn caused by Crucifix is stackable.",
            new List<string>()
            {
                "ITEM_CRUCIFIX_DESC"
            }
        );
        public static float damageReductionPercent = damageReduction.Value / 100f;
        public static float maxHealthBurnAmountPercent = maxHealthBurnAmount.Value / 100f;
        public static float maxHealthBurnAmountReductionPercent = maxHealthBurnAmountReduction.Value / 100f;

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

            itemDef.name = "CRUCIFIX";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Lunar);

            GameObject prefab = AssetHandler.bundle.LoadAsset<GameObject>("Crucifix.prefab");
            ModelPanelParameters modelPanelParameters = prefab.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = prefab.transform;
            modelPanelParameters.cameraPositionTransform = prefab.transform;
            modelPanelParameters.maxDistance = 10f;
            modelPanelParameters.minDistance = 5f;

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("Crucifix.png");
            itemDef.pickupModelPrefab = prefab;
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        public static void Hooks()
        {
            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (victimInfo.inventory == null || victimInfo.body == null || victimInfo.body.healthComponent == null || attackerInfo.body == null) return;

                // Not immune to void death
                if (damageInfo.damageType == DamageType.VoidDeath) return;

                int count = victimInfo.inventory.GetItemCountPermanent(itemDef);
                if (count > 0 && attackerInfo.body != victimInfo.body)
                {
                    damageInfo.damage *= (1 - damageReductionPercent);
                    float stackedPercentage = Utils.GetReverseExponentialStacking(maxHealthBurnAmountPercent, maxHealthBurnAmountReductionPercent, count);

                    InflictDotInfo dotInfo = new()
                    {
                        victimObject = victimInfo.body.gameObject,
                        attackerObject = victimInfo.body.gameObject,
                        totalDamage = victimInfo.body.healthComponent.fullCombinedHealth * stackedPercentage,
                        dotIndex = DotController.DotIndex.Burn,
                        duration = 0f,
                        damageMultiplier = 1f
                    };
                    if (!isCrucifixBurnStackable) dotInfo.maxStacksFromAttacker = 1;

                    DotController.InflictDot(ref dotInfo);
                }
            };
        }
    }
}
