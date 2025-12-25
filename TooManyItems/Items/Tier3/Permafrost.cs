using R2API;
using RoR2;
using System.Collections.Generic;
using TooManyItems.Helpers;
using TooManyItems.Managers;
using UnityEngine;

namespace TooManyItems.Items.Tier3
{
    internal class Permafrost
    {
        public static ItemDef itemDef;

        public static DamageColorIndex damageColor = DamageColorManager.RegisterDamageColor(Utilities.PERMAFROST_COLOR);

        // Dealing damage has a chance to freeze enemies. You deal bonus damage to frozen enemies.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Permafrost",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_PERMAFROST_DESC"
            }
        );
        public static ConfigurableValue<float> freezeChance = new(
            "Item: Permafrost",
            "Freeze Chance",
            1.5f,
            "Chance to apply freeze when dealing damage.",
            new List<string>()
            {
                "ITEM_PERMAFROST_DESC"
            }
        );
        public static ConfigurableValue<float> frozenDamageMultiplier = new(
            "Item: Permafrost",
            "Bonus Frozen Damage",
            45f,
            "Percent bonus damage dealt to frozen enemies.",
            new List<string>()
            {
                "ITEM_PERMAFROST_DESC"
            }
        );
        public static float freezeChancePercent = freezeChance.Value / 100.0f;
        public static float frozenDamageMultiplierPercent = frozenDamageMultiplier.Value / 100.0f;

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

            itemDef.name = "PERMAFROST";
            itemDef.AutoPopulateTokens();

            Utilities.SetItemTier(itemDef, ItemTier.Tier3);

            GameObject prefab = AssetManager.bundle.LoadAsset<GameObject>("Permafrost.prefab");
            prefab.AddComponent<PermafrostRotationHandler>();

            ModelPanelParameters modelPanelParameters = prefab.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = prefab.transform;
            modelPanelParameters.cameraPositionTransform = prefab.transform;
            modelPanelParameters.maxDistance = 10f;
            modelPanelParameters.minDistance = 5f;

            itemDef.pickupIconSprite = AssetManager.bundle.LoadAsset<Sprite>("Permafrost.png");
            itemDef.pickupModelPrefab = prefab;
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage,
                ItemTag.Utility,

                ItemTag.CanBeTemporary
            };
        }

        public static void Hooks()
        {
            GameEventManager.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                CharacterBody attackerBody = attackerInfo.body;
                CharacterBody victimBody = victimInfo.body;
                if (attackerBody && victimBody && attackerBody.inventory)
                {
                    int count = attackerBody.inventory.GetItemCountEffective(itemDef);
                    if (count > 0 && attackerBody.master)
                    {
                        if (Util.CheckRoll(Utilities.GetHyperbolicStacking(freezeChancePercent, count) * 100f * damageInfo.procCoefficient, attackerBody.master.luck, attackerBody.master))
                        {
                            damageInfo.damageType |= DamageType.Freeze2s;
                        }

                        if (victimBody.healthComponent.isInFrozenState)
                        {
                            damageInfo.damage *= 1 + frozenDamageMultiplierPercent * count;
                            damageInfo.damageColorIndex = damageColor;
                        }
                    }
                }
            };
        }
    }
}
