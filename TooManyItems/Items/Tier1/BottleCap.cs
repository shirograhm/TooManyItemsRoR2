using R2API;
using RoR2;
using System.Collections.Generic;

namespace TooManyItems
{
    internal class BottleCap : BaseItem
    {
        // Reduce your special skill cooldown by a percentage.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Bottle Cap",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_BOTTLECAP_DESC"
            }
        );
        public static ConfigurableValue<float> specialCDR = new(
            "Item: Bottle Cap",
            "Cooldown Reduction",
            10f,
            "Percent cooldown reduction on special skill.",
            new List<string>()
            {
                "ITEM_BOTTLECAP_DESC"
            }
        );
        public static float specialCDRPercent = specialCDR.Value / 100f;

        internal static void Init()
        {
            GenerateItem(
                "BOTTLECAP",
                "BottleCap.prefab",
                "BottleCap.png",
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
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int itemCount = sender.inventory.GetItemCountEffective(itemDef);
                    if (itemCount > 0)
                    {
                        float cdr = Utils.GetHyperbolicStacking(specialCDRPercent, itemCount);
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
