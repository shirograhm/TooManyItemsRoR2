using R2API;
using RoR2;
using TooManyItems.Managers;

namespace TooManyItems.Items.Tier2
{
    internal class HereticSeal
    {
        public static ItemDef itemDef;

        // Gain bonus damage based on missing health.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Seal of the Heretic",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_HERETICSEAL_DESC"]
        );
        public static ConfigurableValue<float> damagePerMissing = new(
            "Item: Seal of the Heretic",
            "Damage Increase",
            0.3f,
            "Base damage gained for each percentage of missing health.",
            ["ITEM_HERETICSEAL_DESC"]
        );

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("HereticSeal", [ItemTag.Damage, ItemTag.CanBeTemporary], ItemTier.Tier2);

            Hooks();
        }

        public static void Hooks()
        {
            Utilities.AddRecalculateOnFrameHook(itemDef);

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        // Make sure this calculation only runs when healthFraction is below 1, not above 1
                        if (sender.healthComponent.combinedHealthFraction < 1f)
                        {
                            args.baseDamageAdd += count * damagePerMissing.Value * (1f - sender.healthComponent.combinedHealthFraction) * 100f;
                        }
                    }
                }
            };
        }
    }
}
