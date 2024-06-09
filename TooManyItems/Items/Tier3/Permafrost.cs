using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class Permafrost
    {
        public static ItemDef itemDef;

        public static DamageColorIndex damageColor = DamageColorAPI.RegisterDamageColor(Utils.PERMAFROST_COLOR);

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
            3f,
            "Chance to apply freeze when dealing damage.",
            new List<string>()
            {
                "ITEM_PERMAFROST_DESC"
            }
        );
        public static ConfigurableValue<float> frozenDamageMultiplier = new(
            "Item: Permafrost",
            "Bonus Frozen Damage",
            90f,
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

            Utils.SetItemTier(itemDef, ItemTier.Tier3);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Permafrost.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Permafrost.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage,
                ItemTag.Utility
            };
        }

        public static void Hooks()
        {
            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                CharacterBody attackerBody = attackerInfo.body;
                CharacterBody victimBody = victimInfo.body;
                if (attackerBody && victimBody && attackerBody.inventory)
                {
                    int count = attackerBody.inventory.GetItemCount(itemDef);
                    if (count > 0 && attackerBody.master)
                    {
                        if (Util.CheckRoll(Utils.GetHyperbolicStacking(freezeChancePercent, count) * 100f, attackerBody.master.luck, attackerBody.master))
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
