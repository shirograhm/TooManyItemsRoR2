﻿using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class RedBlueGlasses
    {
        public static ItemDef itemDef;

        // Crit more and crit harder.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: 3D Glasses",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_REDBLUEGLASSES_DESC"
            }
        );
        public static ConfigurableValue<float> critChancePerStack = new(
            "Item: 3D Glasses",
            "Crit Chance",
            6f,
            "Amount of crit chance gained per stack.",
            new List<string>()
            {
                "ITEM_REDBLUEGLASSES_DESC"
            }
        );
        public static ConfigurableValue<float> critDamagePerStack = new(
            "Item: 3D Glasses",
            "Crit Damage",
            6f,
            "Amount of crit damage gained per stack.",
            new List<string>()
            {
                "ITEM_REDBLUEGLASSES_DESC"
            }
        );
        public static float critChancePercent = critChancePerStack.Value / 100f;
        public static float critDamagePercent = critDamagePerStack.Value / 100f;

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

            itemDef.name = "REDBLUEGLASSES";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier1);

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("3DGlasses.png");
            itemDef.pickupModelPrefab = AssetHandler.bundle.LoadAsset<GameObject>("3DGlasses.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage
            };
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
                        args.critAdd += count * critChancePerStack.Value;
                        args.critDamageMultAdd += count * critDamagePercent;
                    }
                }
            };
        }
    }
}
