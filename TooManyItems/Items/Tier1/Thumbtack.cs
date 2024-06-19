using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class Thumbtack
    {
        public static ItemDef itemDef;

        // Your bleed effects last longer.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Thumbtack",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_THUMBTACK_DESC"
            }
        );
        public static ConfigurableValue<float> bleedChance = new(
            "Item: Thumbtack",
            "Bleed Chance",
            5f,
            "Chance to bleed with this item.",
            new List<string>()
            {
                "ITEM_THUMBTACK_DESC"
            }
        );
        public static float bleedChancePercent = bleedChance.Value / 100f;

        public static ConfigurableValue<float> bleedDamage = new(
            "Item: Thumbtack",
            "Bleed Damage",
            240f,
            "Base damage dealt as bleed for this item.",
            new List<string>()
            {
                "ITEM_THUMBTACK_DESC"
            }
        );
        public static float bleedDamagePercent = bleedDamage.Value / 100f;

        public static ConfigurableValue<float> bleedDuration = new(
            "Item: Thumbtack",
            "Bleed Duration",
            3f,
            "Bleed duration for this item.",
            new List<string>()
            {
                "ITEM_THUMBTACK_DESC"
            }
        );

        public static ConfigurableValue<float> bleedDurationBonus = new(
            "Item: Thumbtack",
            "Bonus Bleed Duration",
            0.25f,
            "How much longer your bleed effects last for each stack of this item.",
            new List<string>()
            {
                "ITEM_THUMBTACK_DESC"
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

            itemDef.name = "THUMBTACK";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier1);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Thumbtack.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Thumbtack.prefab");
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
            GenericGameEvents.OnHitEnemy += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (attackerInfo.body && victimInfo.body && attackerInfo.inventory)
                {
                    int itemCount = attackerInfo.inventory.GetItemCount(itemDef);
                    if (attackerInfo.master && itemCount > 0)
                    {
                        // If the hit has a proc coefficient, doesn't already apply bleed, and isn't applied to a member of the same team, roll for bleed
                        if (damageInfo.procCoefficient > 0 && damageInfo.damageType != DamageType.BleedOnHit && attackerInfo.teamIndex != victimInfo.teamIndex)
                        {
                            if (Util.CheckRoll(bleedChance.Value, attackerInfo.master.luck, attackerInfo.master))
                            {
                                InflictDotInfo info = new()
                                {
                                    victimObject = victimInfo.gameObject,
                                    attackerObject = attackerInfo.gameObject,
                                    damageMultiplier = bleedDamagePercent,
                                    dotIndex = DotController.DotIndex.Bleed,
                                    duration = bleedDuration.Value * damageInfo.procCoefficient
                                };
                                DotController.InflictDot(ref info);
                            }
                        }
                    }
                }
            };

            On.RoR2.DotController.InflictDot_refInflictDotInfo += DotController_InflictDot_refInflictDotInfo;
        }

        private static void DotController_InflictDot_refInflictDotInfo(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo info)
        {
            if (!NetworkServer.active) return;

            CharacterBody atkBody = info.attackerObject.GetComponent<CharacterBody>();
            if (atkBody && atkBody.inventory)
            {
                int itemCount = atkBody.inventory.GetItemCount(itemDef);
                if (itemCount > 0 && info.dotIndex == DotController.DotIndex.Bleed)
                {
                    float bleedTickRate = 4.0f;
                    float extraTicks = bleedDurationBonus.Value * itemCount * bleedTickRate;

                    float damageMultPerTick = info.damageMultiplier / (info.duration * bleedTickRate);
                    info.damageMultiplier += damageMultPerTick * extraTicks;
                    info.duration += bleedDurationBonus.Value * itemCount;
                }
            }

            orig(ref info);
        }
    }
}
