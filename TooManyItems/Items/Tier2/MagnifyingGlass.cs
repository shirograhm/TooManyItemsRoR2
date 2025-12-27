using R2API;
using RoR2;
using TooManyItems.Managers;
using UnityEngine;

namespace TooManyItems.Items.Tier2
{
    internal class MagnifyingGlass
    {
        public static ItemDef itemDef;
        public static BuffDef analyzedDebuff;

        // Gain crit chance. Critical strikes have a chance to Analyze the enemy, increasing their damage taken from all sources.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Magnifying Glass",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_MAGNIFYINGGLASS_DESC"]
        );
        public static ConfigurableValue<float> critBonus = new(
            "Item: Magnifying Glass",
            "Crit Chance",
            5f,
            "Crit chance increase so that the item isn't worthless without other crit items.",
            ["ITEM_MAGNIFYINGGLASS_DESC"]
        );
        public static ConfigurableValue<float> analyzeChance = new(
            "Item: Magnifying Glass",
            "Analyze Chance",
            6f,
            "Percent chance to Analyze an enemy on crit.",
            ["ITEM_MAGNIFYINGGLASS_DESC"]
        );
        public static ConfigurableValue<float> damageTakenBonus = new(
            "Item: Magnifying Glass",
            "Damage Taken Bonus",
            18f,
            "Percent damage taken bonus once Analyzed.",
            ["ITEM_MAGNIFYINGGLASS_DESC"]
        );
        public static float analyzeChancePercent = analyzeChance.Value / 100f;
        public static float damageTakenBonusPercent = damageTakenBonus.Value / 100f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("MagnifyingGlass", [ItemTag.Damage, ItemTag.CanBeTemporary], ItemTier.Tier2);

            analyzedDebuff = ItemManager.GenerateBuff("Analyzed", AssetManager.bundle.LoadAsset<Sprite>("Analyzed.png"), isDebuff: true);
            ContentAddition.AddBuffDef(analyzedDebuff);

            Hooks();
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        args.critAdd += critBonus.Value;
                    }
                }
            };

            GameEventManager.OnTakeDamage += (damageReport) =>
            {
                CharacterBody atkBody = damageReport.attackerBody;
                CharacterBody vicBody = damageReport.victimBody;
                CharacterMaster atkMaster = damageReport.attackerMaster;
                if (atkBody && vicBody && atkMaster && atkBody.inventory)
                {
                    int count = atkBody.inventory.GetItemCountEffective(itemDef);
                    if (count > 0 && damageReport.damageInfo.crit)
                    {
                        if (Util.CheckRoll(analyzeChance.Value * damageReport.damageInfo.procCoefficient, atkMaster.luck, atkMaster))
                        {
                            vicBody.AddBuff(analyzedDebuff);
                        }
                    }
                }
            };

            GameEventManager.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                CharacterBody atkBody = attackerInfo.body;
                CharacterBody vicBody = victimInfo.body;
                if (atkBody && atkBody.inventory && vicBody && vicBody.HasBuff(analyzedDebuff))
                {
                    damageInfo.damage *= 1f + damageTakenBonusPercent * atkBody.inventory.GetItemCountEffective(itemDef);
                }
            };
        }
    }
}
