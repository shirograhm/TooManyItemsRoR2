using R2API;
using RoR2;
using System;
using System.Collections.Generic;

namespace TooManyItems
{
    internal class DebitCard : BaseItem
    {
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
            GenerateItem(
                "DEBITCARD",
                "DebitCard.prefab",
                "DebitCard.png",
                ItemTier.Tier1,
                [
                    ItemTag.Utility,
                    ItemTag.CanBeTemporary
                ]
            );

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        public static new void Hooks()
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
                        float refundScaling = Utils.GetHyperbolicStacking(rebatePercent, count);
                        activator.GiveMoney(Convert.ToUInt32(moneyCost * refundScaling));
                    }
                }
            };
        }
    }
}
