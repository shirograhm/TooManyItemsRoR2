using R2API;
using RoR2;
using System.Collections.Generic;

namespace TooManyItems
{
    internal class ShadowCrest : BaseItem
    {
        // Gain health regen based on missing health.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Shadow Crest",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_SHADOWCREST_DESC"
            }
        );
        public static ConfigurableValue<float> regenPerSecond = new(
            "Item: Shadow Crest",
            "Regen Per Second",
            1.2f,
            "Percentage of missing health regenerated per second.",
            new List<string>()
            {
                "ITEM_SHADOWCREST_DESC"
            }
        );
        public static float regenPerSecondPercent = regenPerSecond.Value / 100f;

        internal static void Init()
        {
            GenerateItem(
                "SHADOWCREST",
                "ShadowCrest.prefab",
                "ShadowCrest.png",
                ItemTier.VoidTier2,
                [
                    ItemTag.Healing,
                    ItemTag.CanBeTemporary
                ]
            );

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        public static new void Hooks()
        {
            On.RoR2.CharacterBody.FixedUpdate += (orig, self) =>
            {
                orig(self);

                if (self && self.inventory)
                {
                    int count = self.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        Utils.ForceRecalculate(self);
                    }
                }
            };

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
                            args.baseRegenAdd += Utils.GetHyperbolicStacking(regenPerSecondPercent, count) * sender.healthComponent.missingCombinedHealth;
                        }
                    }
                }
            };
        }
    }
}
