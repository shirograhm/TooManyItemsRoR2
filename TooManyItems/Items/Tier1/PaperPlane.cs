﻿using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class PaperPlane
    {
        public static ItemDef itemDef;

        // Increase movement speed while airborne.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Paper Plane",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_PAPERPLANE_DESC"
            }
        );
        public static ConfigurableValue<float> movespeedIncrease = new(
            "Item: Paper Plane",
            "Movement Speed",
            18f,
            "Percent movement speed gained per stack while airborne.",
            new List<string>()
            {
                "ITEM_PAPERPLANE_DESC"
            }
        );
        public static float movespeedIncreasePercent = movespeedIncrease.Value / 100f;

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

            itemDef.name = "PAPERPLANE";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier1);

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("PaperPlane.png");
            itemDef.pickupModelPrefab = AssetHandler.bundle.LoadAsset<GameObject>("PaperPlane.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility
            };
        }

        public static void Hooks()
        {
            On.RoR2.CharacterBody.FixedUpdate += (orig, self) =>
            {
                orig(self);

                if (self && self.inventory)
                {
                    if (self.inventory.GetItemCount(itemDef) > 0)
                    {
                        Utils.ForceRecalculate(self);
                    }
                }
            };

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCount(itemDef);
                    if (count > 0 && sender.characterMotor && !sender.characterMotor.isGrounded)
                    {
                        args.moveSpeedMultAdd += movespeedIncreasePercent * count;
                    }
                }
            };
        }
    }
}
