using R2API;
using RoR2;
using TooManyItems.Managers;

namespace TooManyItems.Items.Void
{
    internal class ShadowCrest
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
            Utilities.AddRecalculateOnFrameHook(itemDef);

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
