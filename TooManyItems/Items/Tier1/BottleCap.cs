using R2API;
using RoR2;
using TooManyItems.Managers;

namespace TooManyItems.Items.Tier1
{
    internal class BottleCap
    {
        public static ItemDef itemDef;

        // Reduce your special skill cooldown by a percentage.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Bottle Cap",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_BOTTLECAP_DESC"]
        );
        public static ConfigurableValue<float> specialCDR = new(
            "Item: Bottle Cap",
            "Cooldown Reduction",
            10f,
            "Percent cooldown reduction on special skill.",
            ["ITEM_BOTTLECAP_DESC"]
        );
        public static float specialCDRPercent = specialCDR.Value / 100f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("BottleCap", [ItemTag.Utility, ItemTag.CanBeTemporary], ItemTier.Tier1);

            Hooks();
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int itemCount = sender.inventory.GetItemCountEffective(itemDef);
                    if (itemCount > 0)
                    {
                        float cdr = Utilities.GetHyperbolicStacking(specialCDRPercent, itemCount);
                        // Calculate the actual number needed for the denominator to achieve the desired cooldown reduction
                        // because RecalculateStatsAPI no longer allows negative cooldownMultAdd values
                        float convertedCDR = cdr / (1f - cdr);
                        args.specialSkill.cooldownReductionMultAdd += convertedCDR;
                    }
                }
            };
        }
    }
}
