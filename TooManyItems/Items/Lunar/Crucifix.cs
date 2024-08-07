﻿using R2API;
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
            40f,
            "Percentage of damage reduced.",
            new List<string>()
            {
                "ITEM_CRUCIFIX_DESC"
            }
        );
        public static ConfigurableValue<float> maxHealthBurnAmount = new(
            "Item: Crucifix",
            "Burn Amount",
            5f,
            "Percentage of max health taken over the duration of the burn.",
            new List<string>()
            {
                "ITEM_CRUCIFIX_DESC"
            }
        );
        public static ConfigurableValue<int> fireDuration = new(
            "Item: Crucifix",
            "Duration of Fire",
            3,
            "Duration of fire debuff after taking damage.",
            new List<string>()
            {
                "ITEM_CRUCIFIX_DESC"
            }
        );
        public static float damageReductionPercent = damageReduction.Value / 100f;

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

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("Crucifix.png");
            itemDef.pickupModelPrefab = AssetHandler.bundle.LoadAsset<GameObject>("Crucifix.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        public static void Hooks()
        {
            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (victimInfo.inventory == null || victimInfo.body == null || attackerInfo.body == null) return;

                int count = victimInfo.inventory.GetItemCount(itemDef);
                if (count > 0 && attackerInfo.body != victimInfo.body)
                {
                    damageInfo.damage *= (1 - damageReductionPercent);

                    InflictDotInfo dotInfo = new()
                    {
                        victimObject = victimInfo.body.gameObject,
                        attackerObject = victimInfo.body.gameObject,
                        totalDamage = victimInfo.body.healthComponent.fullCombinedHealth * maxHealthBurnAmount.Value / 100f,
                        dotIndex = DotController.DotIndex.Burn,
                        duration = fireDuration.Value * count,
                        damageMultiplier = 1f
                    };
                    DotController.InflictDot(ref dotInfo);
                }
            };
        }
    }
}
