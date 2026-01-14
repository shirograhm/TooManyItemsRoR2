using R2API;
using RoR2;
using RoR2.Items;
using TooManyItems.Managers;

namespace TooManyItems.Items.Void
{
    public class ShadowCrestItemBehaviour : BaseItemBodyBehavior
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        public static ItemDef GetItemDef()
        {
            return ShadowCrest.itemDef;
        }

        public void FixedUpdate()
        {
            if (stack > 0) Utilities.ForceRecalculate(body);
        }
    }

    public class ShadowCrest
    {
        public static ItemDef itemDef;

        // Gain health regen based on missing health.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Shadow Crest",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_SHADOWCREST_DESC"]
        );
        public static ConfigurableValue<float> regenPerSecond = new(
            "Item: Shadow Crest",
            "Regen Per Second",
            1f,
            "Percentage of missing health regenerated per second.",
            ["ITEM_SHADOWCREST_DESC"]
        );
        public static ConfigurableValue<float> regenPerSecondExtraStacks = new(
            "Item: Shadow Crest",
            "Regen Per Second Extra Stacks",
            1f,
            "Percentage of missing health regenerated per second for extra stacks.",
            ["ITEM_SHADOWCREST_DESC"]
        );
        public static float percentRegenPerSecond = regenPerSecond.Value / 100f;
        public static float percentRegenPerSecondExtraStacks = regenPerSecondExtraStacks.Value / 100f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("ShadowCrest", [ItemTag.Healing, ItemTag.CanBeTemporary], ItemTier.VoidTier2);

            Hooks();
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory && sender.healthComponent)
                {
                    int count = sender.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        // Make sure this calculation only runs when healthFraction is below 1, not above 1
                        if (sender.healthComponent.combinedHealthFraction < 1f)
                        {
                            args.baseRegenAdd += Utilities.GetHyperbolicStacking(percentRegenPerSecond, percentRegenPerSecondExtraStacks, count) * sender.healthComponent.missingCombinedHealth;
                        }
                    }
                }
            };
        }
    }
}
