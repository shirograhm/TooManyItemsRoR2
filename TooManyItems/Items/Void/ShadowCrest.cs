using R2API;
using RoR2;
using System.Collections.Generic;
using TooManyItems.Managers;
using UnityEngine;

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
            GenerateItem();

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "SHADOWCREST";
            itemDef.AutoPopulateTokens();

            Utilities.SetItemTier(itemDef, ItemTier.VoidTier2);

            GameObject prefab = AssetManager.bundle.LoadAsset<GameObject>("ShadowCrest.prefab");
            ModelPanelParameters modelPanelParameters = prefab.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = prefab.transform;
            modelPanelParameters.cameraPositionTransform = prefab.transform;
            modelPanelParameters.maxDistance = 10f;
            modelPanelParameters.minDistance = 5f;

            itemDef.pickupIconSprite = AssetManager.bundle.LoadAsset<Sprite>("ShadowCrest.png");
            itemDef.pickupModelPrefab = prefab;
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.requiredExpansion = TooManyItems.sotvDLC;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Healing,

                ItemTag.CanBeTemporary
            };
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
                            args.baseRegenAdd += Utilities.GetHyperbolicStacking(regenPerSecondPercent, count) * sender.healthComponent.missingCombinedHealth;
                        }
                    }
                }
            };
        }
    }
}
