using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using TooManyItems.Managers;
using UnityEngine;

namespace TooManyItems.Items.Tier1
{
    internal class DebitCard
    {
        public static ItemDef itemDef;

        // Get a rebate on purchases.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Debit Card",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_DEBITCARD_DESC"
            }
        );
        public static ConfigurableValue<float> rebate = new(
            "Item: Debit Card",
            "Rebate",
            10f,
            "Percentage of spent gold refunded as rebate.",
            new List<string>()
            {
                "ITEM_DEBITCARD_DESC"
            }
        );
        public static float rebatePercent = rebate.Value / 100f;

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

            itemDef.name = "DEBITCARD";
            itemDef.AutoPopulateTokens();

            Utilities.SetItemTier(itemDef, ItemTier.Tier1);

            GameObject prefab = AssetManager.bundle.LoadAsset<GameObject>("DebitCard.prefab");
            ModelPanelParameters modelPanelParameters = prefab.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = prefab.transform;
            modelPanelParameters.cameraPositionTransform = prefab.transform;
            modelPanelParameters.maxDistance = 10f;
            modelPanelParameters.minDistance = 5f;

            itemDef.pickupIconSprite = AssetManager.bundle.LoadAsset<Sprite>("DebitCard.png");
            itemDef.pickupModelPrefab = prefab;
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
            On.RoR2.Items.MultiShopCardUtils.OnPurchase += (orig, context, moneyCost) =>
            {
                orig(context, moneyCost);

                CharacterMaster activator = context.activatorMaster;
                if (activator && activator.hasBody && activator.inventory)
                {
                    int count = activator.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        float refundScaling = Utilities.GetHyperbolicStacking(rebatePercent, count);
                        activator.GiveMoney(Convert.ToUInt32(moneyCost * refundScaling));
                    }
                }
            };
        }
    }
}
