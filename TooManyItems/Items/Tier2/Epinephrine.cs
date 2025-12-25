using R2API;
using RoR2;
using System.Collections.Generic;
using TooManyItems.Managers;
using UnityEngine;

namespace TooManyItems.Items.Tier2
{
    internal class Epinephrine
    {
        public static ItemDef itemDef;
        public static BuffDef attackSpeedBuff;

        // Gain temporary attack speed after taking damage.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Epinephrine",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_EPINEPHRINE_DESC"
            }
        );
        public static ConfigurableValue<float> attackSpeedBonus = new(
            "Item: Epinephrine",
            "Attack Speed",
            75f,
            "Attack speed gained after taking damage.",
            new List<string>()
            {
                "ITEM_EPINEPHRINE_DESC"
            }
        );
        public static ConfigurableValue<float> buffDuration = new(
            "Item: Epinephrine",
            "Buff Duration",
            1f,
            "Duration of attack speed gained after taking damage.",
            new List<string>()
            {
                "ITEM_EPINEPHRINE_DESC"
            }
        );
        public static float attackSpeedBonusPercent = attackSpeedBonus.Value / 100f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("Epinephrine", [ItemTag.Damage, ItemTag.Utility, ItemTag.CanBeTemporary], ItemTier.Tier2);

            attackSpeedBuff = ItemManager.GenerateBuff("Adrenaline", AssetManager.bundle.LoadAsset<Sprite>("Adrenaline.png"));
            ContentAddition.AddBuffDef(attackSpeedBuff);

            Hooks();
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCountEffective(itemDef);
                    if (count > 0 && sender.HasBuff(attackSpeedBuff))
                    {
                        args.attackSpeedMultAdd += attackSpeedBonusPercent;
                    }
                }
            };

            GameEventManager.OnTakeDamage += (damageReport) =>
            {
                CharacterBody vicBody = damageReport.victimBody;
                if (vicBody && vicBody.inventory)
                {
                    int count = vicBody.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        vicBody.AddTimedBuff(attackSpeedBuff, buffDuration * count);
                    }
                }
            };
        }
    }
}
