using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
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
            60f,
            "Attack speed gained after taking damage.",
            new List<string>()
            {
                "ITEM_EPINEPHRINE_DESC"
            }
        );
        public static ConfigurableValue<float> buffDuration = new(
            "Item: Epinephrine",
            "Buff Duration",
            1.5f,
            "Duration of attack speed gained after taking damage.",
            new List<string>()
            {
                "ITEM_EPINEPHRINE_DESC"
            }
        );
        public static float attackSpeedBonusPercent = attackSpeedBonus.Value / 100f;

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

            itemDef.name = "EPINEPHRINE";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier2);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Epinephrine.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Epinephrine.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage,
                ItemTag.Utility
            };
        }

        private static void GenerateBuff()
        {
            attackSpeedBuff = ScriptableObject.CreateInstance<BuffDef>();

            attackSpeedBuff.name = "Adrenaline";
            attackSpeedBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Adrenaline.png");
            attackSpeedBuff.canStack = false;
            attackSpeedBuff.isHidden = false;
            attackSpeedBuff.isDebuff = false;
            attackSpeedBuff.isCooldown = false;
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCount(itemDef);
                    if (count > 0 && sender.HasBuff(attackSpeedBuff))
                    {
                        args.attackSpeedMultAdd += attackSpeedBonusPercent;
                    }
                }
            };

            GenericGameEvents.OnTakeDamage += (damageReport) =>
            {
                CharacterBody vicBody = damageReport.victimBody;
                if (vicBody && vicBody.inventory)
                {
                    int count = vicBody.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        vicBody.AddTimedBuff(attackSpeedBuff, buffDuration * count);
                    }
                }
            };
        }
    }
}
