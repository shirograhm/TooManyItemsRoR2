using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class Photodiode : BaseItem
    {
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
            GenerateItem(
                "PHOTODIODE",
                "Photodiode.prefab",
                "Photodiode.png",
                ItemTier.Tier1,
                [
                    ItemTag.Damage,
                    ItemTag.CanBeTemporary
                ]
            );
            GenerateBuff();

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            ContentAddition.AddBuffDef(attackSpeedBuff);

            Hooks();
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

        public static new void Hooks()
        {
            GenericGameEvents.OnHitEnemy += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (attackerInfo.body && attackerInfo.inventory)
                {
                    int itemCount = attackerInfo.inventory.GetItemCountEffective(itemDef);
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
                    int count = sender.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        args.attackSpeedMultAdd += sender.GetBuffCount(attackSpeedBuff) * attackSpeedOnHitPercent;
                    }
                }
            };
        }
    }
}
