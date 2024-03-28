using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class Abacus
    {
        public static ItemDef itemDef;
        public static BuffDef countedBuff;

        // Killing an enemy grants 1% (+1% per stack) crit chance for the rest of the stage.
        public static ConfigurableValue<float> critChancePerStack = new(
            "Item: Abacus",
            "Crit Chance On Kill",
            1f,
            "Crit chance gained on kill per stack of item.",
            new List<string>()
            {
                "ITEM_ABACUS_DESC"
            }
        );

        internal static void Init()
        {
            GenerateItem();
            GenerateBuff();
            AddTokens();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            ContentAddition.AddBuffDef(countedBuff);

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "ABACUS";
            itemDef.nameToken = "ABACUS_NAME";
            itemDef.pickupToken = "ABACUS_PICKUP";
            itemDef.descriptionToken = "ABACUS_DESCRIPTION";
            itemDef.loreToken = "ABACUS_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier3;
            });

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Abacus.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Abacus.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage
            };
        }

        private static void GenerateBuff()
        {
            countedBuff = ScriptableObject.CreateInstance<BuffDef>();

            countedBuff.name = "Counted";
            countedBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Counted.png");
            countedBuff.canStack = true;
            countedBuff.isHidden = false;
            countedBuff.isDebuff = false;
            countedBuff.isCooldown = false;
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        int buffCount = sender.GetBuffCount(countedBuff);
                        args.critAdd += buffCount * critChancePerStack.Value;
                    }
                }
            };

            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, eventManager, damageReport) =>
            {
                orig(eventManager, damageReport);

                if (!NetworkServer.active) return;

                CharacterBody atkBody = damageReport.attackerBody;
                if (atkBody && atkBody.inventory)
                {
                    int count = atkBody.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++) atkBody.AddBuff(countedBuff);
                        Utils.ForceRecalculate(atkBody);
                    }
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("ABACUS", "Abacus");
            LanguageAPI.Add("ABACUS_NAME", "Abacus");
            LanguageAPI.Add("ABACUS_PICKUP", "Killing enemies grants stacking crit chance. Resets each stage.");

            string desc = $"Killing an enemy grants <style=cIsUtility>{critChancePerStack.Value}%</style> " +
                $"<style=cStack>(+{critChancePerStack.Value}% per stack)</style> crit chance until the next stage.";
            LanguageAPI.Add("ABACUS_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("ABACUS_LORE", lore);
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