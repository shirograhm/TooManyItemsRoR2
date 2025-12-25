using R2API;
using RoR2;
using System.Collections.Generic;
using TooManyItems.Managers;
using UnityEngine;

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
            new List<string>()
            {
                "ITEM_HERETICSEAL_DESC"
            }
        );
        public static ConfigurableValue<float> damagePerMissing = new(
            "Item: Seal of the Heretic",
            "Damage Increase",
            0.3f,
            "Base damage gained for each percentage of missing health.",
            new List<string>()
            {
                "ITEM_HERETICSEAL_DESC"
            }
        );

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

            itemDef.name = "HERETICSEAL";
            itemDef.AutoPopulateTokens();

            Utilities.SetItemTier(itemDef, ItemTier.Tier2);

            GameObject prefab = AssetManager.bundle.LoadAsset<GameObject>("HereticSeal.prefab");
            ModelPanelParameters modelPanelParameters = prefab.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = prefab.transform;
            modelPanelParameters.cameraPositionTransform = prefab.transform;
            modelPanelParameters.maxDistance = 10f;
            modelPanelParameters.minDistance = 5f;

            itemDef.pickupIconSprite = AssetManager.bundle.LoadAsset<Sprite>("HereticSeal.png");
            itemDef.pickupModelPrefab = prefab;
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage,

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
                            args.baseDamageAdd += count * damagePerMissing.Value * (1f - sender.healthComponent.combinedHealthFraction) * 100f;
                        }
                    }
                }
            };
        }
    }
}
