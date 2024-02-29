using R2API;
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

        // On-hit, gain 2.5% attack speed for 10 seconds, up to a max of 25% (+25% per stack).
        public static ConfigurableValue<float> attackSpeedOnHit = new(
            "Item: Photodiode",
            "Attack Speed On-Hit",
            2.5f,
            "Percent attack speed gained on-hit.",
            new List<string>()
            {
                "ITEM_PHOTODIODE_DESC"
            }
        );
        public static ConfigurableValue<int> attackSpeedDuration = new(
            "Item: Photodiode",
            "Attack Speed Duration",
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
            AddTokens();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            ContentAddition.AddBuffDef(attackSpeedBuff);

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "PHOTODIODE";
            itemDef.nameToken = "PHOTODIODE_NAME";
            itemDef.pickupToken = "PHOTODIODE_PICKUP";
            itemDef.descriptionToken = "PHOTODIODE_DESCRIPTION";
            itemDef.loreToken = "PHOTODIODE_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier1;
            });

            itemDef.pickupIconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("Photodiode.png");
            itemDef.pickupModelPrefab = TooManyItems.MainAssets.LoadAsset<GameObject>("Photodiode.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        private static void GenerateBuff()
        {
            attackSpeedBuff = ScriptableObject.CreateInstance<BuffDef>();

            attackSpeedBuff.name = "Voltage";
            attackSpeedBuff.iconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("Voltage.png");
            attackSpeedBuff.canStack = true;
            attackSpeedBuff.isDebuff = false;
        }

        public static void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victim) =>
            {
                orig(self, damageInfo, victim);

                if (!NetworkServer.active) return;
                if (damageInfo.attacker == null || victim == null) return;

                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();

                if (attackerBody != null && attackerBody.inventory != null)
                {
                    int count = attackerBody.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        int currentStacks = attackerBody.GetBuffCount(attackSpeedBuff);
                        if (currentStacks < maxAttackSpeedStacks.Value * count)
                        {
                            attackerBody.AddTimedBuff(attackSpeedBuff, attackSpeedDuration.Value);
                        }
                    }
                }
            };

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender == null || sender.inventory == null) return;

                int count = sender.inventory.GetItemCount(itemDef);
                if (count > 0)
                {
                    args.attackSpeedMultAdd += sender.GetBuffCount(attackSpeedBuff) * attackSpeedOnHitPercent;
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("PHOTODIODE", "Photodiode");
            LanguageAPI.Add("PHOTODIODE_NAME", "Photodiode");
            LanguageAPI.Add("PHOTODIODE_PICKUP", "Gain temporary attack speed on-hit.");

            string desc = $"On-hit, gain <style=cIsUtility>{attackSpeedOnHit.Value}%</style> attack speed for <style=cIsUtility>{attackSpeedDuration.Value} seconds</style>, " +
                $"up to a maximum of <style=cIsUtility>{maxAttackSpeedAllowed}%</style> <style=cStack>(+{maxAttackSpeedAllowed}% per stack)</style>.";
            LanguageAPI.Add("PHOTODIODE_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("PHOTODIODE_LORE", lore);
        }
    }
}

// Styles
// <style=cIsHealth>" + exampleValue + "</style>
// <style=cIsDamage>" + exampleValue + "</style>
// <style=cIsHealing>" + exampleValue + "</style>
// <style=cIsUtility>" + exampleValue + "</style>
// <style=cIsVoid>" + exampleValue + "</style>
// <style=cHumanObjective>" + exampleValue + "</style>
// <style=cLunarObjective>" + exampleValue + "</style>
// <style=cStack>" + exampleValue + "</style>
// <style=cWorldEvent>" + exampleValue + "</style>
// <style=cArtifact>" + exampleValue + "</style>
// <style=cUserSetting>" + exampleValue + "</style>
// <style=cDeath>" + exampleValue + "</style>
// <style=cSub>" + exampleValue + "</style>
// <style=cMono>" + exampleValue + "</style>
// <style=cShrine>" + exampleValue + "</style>
// <style=cEvent>" + exampleValue + "</style>