using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class BottleCap
    {
        public static ItemDef itemDef;

        // Reduce your ultimate skill cooldown by a percentage.
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
        public static ConfigurableValue<float> ultimateCDR = new(
            "Item: Bottle Cap",
            "Cooldown Reduction",
            8f,
            "Percent cooldown reduction on ultimate skill.",
            new List<string>()
            {
                "ITEM_BOTTLECAP_DESC"
            }
        );
        public static float ultimateCDRPercent = ultimateCDR.Value / 100f;

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

            itemDef.name = "BOTTLECAP";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier1);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("BottleCap.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("BottleCap.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility
            };
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        float cdr = 1 - (1 / (1 + (ultimateCDRPercent * count)));
                        args.specialCooldownMultAdd -= cdr;
                    }
                }
            };
        }
    }
}
