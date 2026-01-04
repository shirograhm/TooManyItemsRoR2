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
            "Percent damage taken bonus once Analyzed for the first stack of this item.",
            ["ITEM_MAGNIFYINGGLASS_DESC"]
        );
        public static ConfigurableValue<float> damageTakenBonusExtraStacks = new(
            "Item: Magnifying Glass",
            "Damage Taken Bonus Extra Stacks",
            18f,
            "Percent damage taken bonus once Analyzed for extra stacks.",
            ["ITEM_MAGNIFYINGGLASS_DESC"]
        );
        public static float percentAnalyzeChance = analyzeChance.Value / 100f;
        public static float percentDamageTakenBonus = damageTakenBonus.Value / 100f;
        public static float percentDamageTakenBonusExtraStacks = damageTakenBonusExtraStacks.Value / 100f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("MagnifyingGlass", [ItemTag.Damage, ItemTag.CanBeTemporary], ItemTier.Tier2);

            analyzedDebuff = ItemManager.GenerateBuff("Analyzed", AssetManager.bundle.LoadAsset<Sprite>("Analyzed.png"), canStack: true, isDebuff: true);
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
                            int existingStacks = vicBody.GetBuffCount(analyzedDebuff.buffIndex);
                            // Only add stacks if the user has more item stacks to prevent accidentally reducing stacks
                            if (existingStacks < count)
                                vicBody.SetBuffCount(analyzedDebuff.buffIndex, count);
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
                    damageInfo.damage *= 1f + Utilities.GetLinearStacking(percentDamageTakenBonus, percentDamageTakenBonusExtraStacks, vicBody.GetBuffCount(analyzedDebuff.buffIndex));
                }
            };
        }
    }
}
