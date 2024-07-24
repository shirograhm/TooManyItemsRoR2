﻿using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class Photodiode
    {
        public static ItemDef itemDef;
        public static BuffDef attackSpeedBuff;

        // Gain temporary attack speed on-hit.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Photodiode",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_PHOTODIODE_DESC"
            }
        );
        public static ConfigurableValue<float> attackSpeedOnHit = new(
            "Item: Photodiode",
            "Attack Speed",
            2f,
            "Percent attack speed gained on-hit.",
            new List<string>()
            {
                "ITEM_PHOTODIODE_DESC"
            }
        );
        public static ConfigurableValue<int> attackSpeedDuration = new(
            "Item: Photodiode",
            "Buff Duration",
            10,
            "Duration of attack speed buff in seconds.",
            new List<string>()
            {
                "ITEM_PHOTODIODE_DESC"
            }
        );
        public static ConfigurableValue<int> maxAttackSpeedStacks = new(
            "Item: Photodiode",
            "Max Stacks",
            10,
            "Max attack speed stacks allowed per stack of item.",
            new List<string>()
            {
                "ITEM_PHOTODIODE_DESC"
            }
        );
        public static float attackSpeedOnHitPercent = attackSpeedOnHit.Value / 100f;
        public static float maxAttackSpeedAllowed = attackSpeedOnHit.Value * maxAttackSpeedStacks.Value;
        public static float maxAttackSpeedAllowedPercent = maxAttackSpeedAllowed / 100f;

        internal static void Init()
        {
            GenerateItem();
            GenerateBuff();

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            ContentAddition.AddBuffDef(attackSpeedBuff);

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "PHOTODIODE";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier1);

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("Photodiode.png");
            itemDef.pickupModelPrefab = AssetHandler.bundle.LoadAsset<GameObject>("Photodiode.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage
            };
        }

        private static void GenerateBuff()
        {
            attackSpeedBuff = ScriptableObject.CreateInstance<BuffDef>();

            attackSpeedBuff.name = "Voltage";
            attackSpeedBuff.iconSprite = AssetHandler.bundle.LoadAsset<Sprite>("Voltage.png");
            attackSpeedBuff.canStack = true;
            attackSpeedBuff.isHidden = false;
            attackSpeedBuff.isDebuff = false;
            attackSpeedBuff.isCooldown = false;
        }

        public static void Hooks()
        {
            GenericGameEvents.OnHitEnemy += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (attackerInfo.body && attackerInfo.inventory)
                {
                    int itemCount = attackerInfo.inventory.GetItemCount(itemDef);
                    if (itemCount > 0)
                    {
                        int currentStacks = attackerInfo.body.GetBuffCount(attackSpeedBuff);
                        if (currentStacks < maxAttackSpeedStacks.Value * itemCount)
                        {
                            attackerInfo.body.AddTimedBuff(attackSpeedBuff, attackSpeedDuration.Value);
                        }
                    }
                }
            };

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        args.attackSpeedMultAdd += sender.GetBuffCount(attackSpeedBuff) * attackSpeedOnHitPercent;
                    }
                }
            };
        }
    }
}
