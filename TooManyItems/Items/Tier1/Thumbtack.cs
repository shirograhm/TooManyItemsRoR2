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
            4f,
            "Chance to bleed per stack of this item.",
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

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("Thumbtack.png");
            itemDef.pickupModelPrefab = AssetHandler.bundle.LoadAsset<GameObject>("Thumbtack.prefab");
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
            GenericGameEvents.OnHitEnemy += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (attackerInfo.body && victimInfo.body && attackerInfo.inventory)
                {
                    int itemCount = attackerInfo.inventory.GetItemCountEffective(itemDef);
                    if (attackerInfo.master && itemCount > 0)
                    {
                        // If the hit doesn't already apply bleed, and isn't applied by a member of the same team, roll for bleed
                        if (damageInfo.damageType != DamageType.BleedOnHit && attackerInfo.teamIndex != victimInfo.teamIndex)
                        {
                            if (Util.CheckRoll(bleedChance.Value * itemCount * damageInfo.procCoefficient, attackerInfo.master.luck, attackerInfo.master))
                            {
                                InflictDotInfo info = new()
                                {
                                    victimObject = victimInfo.gameObject,
                                    attackerObject = attackerInfo.gameObject,
                                    damageMultiplier = bleedDamagePercent,
                                    dotIndex = DotController.DotIndex.Bleed,
                                    duration = bleedDuration.Value
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

            if (info.attackerObject)
            {
                CharacterBody atkBody = info.attackerObject.GetComponent<CharacterBody>();
                if (atkBody && atkBody.inventory)
                {
                    int itemCount = atkBody.inventory.GetItemCountEffective(itemDef);
                    if (itemCount > 0 && info.dotIndex == DotController.DotIndex.Bleed)
                    {
                        info.duration += bleedDurationBonus.Value * itemCount;
                    }
                }
            }
            orig(ref info);
        }
    }
}
