using RoR2;
using System;
using TooManyItems.Managers;

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
            ["ITEM_DEBITCARD_DESC"]
        );
        public static ConfigurableValue<float> rebate = new(
            "Item: Debit Card",
            "Rebate",
            10f,
            "Percentage of spent gold refunded as rebate.",
            ["ITEM_DEBITCARD_DESC"]
        );
        public static ConfigurableValue<float> rebateExtraStacks = new(
            "Item: Debit Card",
            "Rebate Extra Stacks",
            10f,
            "Percentage of spent gold refunded as rebate for extra stacks.",
            ["ITEM_DEBITCARD_DESC"]
        );
        public static float percentRebate = rebate.Value / 100f;
        public static float percentRebateExtraStacks = rebateExtraStacks.Value / 100f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("DebitCard", [ItemTag.AIBlacklist, ItemTag.Utility, ItemTag.CanBeTemporary], ItemTier.Tier1);

            Hooks();
        }

        public static void Hooks()
        {
            On.RoR2.Items.MultiShopCardUtils.OnPurchase += (orig, context, moneyCost) =>
            {
                orig(context, moneyCost);

                CharacterMaster activator = context.activatorMaster;
                if (activator && activator.hasBody && activator.inventory)
                {
                    CharacterBody activatorBody = activator.GetBody();

                    int count = activator.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        float refundScaling = Utilities.GetHyperbolicStacking(percentRebate, percentRebateExtraStacks, count);
                        Utilities.SendGoldOrbAndEffect(
                            Convert.ToUInt32(moneyCost * refundScaling),
                            context.purchaseInteraction ? context.purchaseInteraction.GetPosition() : activatorBody.corePosition,
                            activatorBody.mainHurtBox
                        );
                    }
                }
            };
        }
    }
}
