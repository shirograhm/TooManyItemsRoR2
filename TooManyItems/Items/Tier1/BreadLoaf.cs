using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class BreadLoaf
    {
        public static ItemDef itemDef;

        // While the teleporter is charging, killing enemies heals you for 4% (+4% per stack) of your missing health.
        public static ConfigurableValue<float> healthGainOnKill = new(
            "Item: Loaf of Bread",
            "Healing On Kill",
            4f,
            "Percent missing health gained after killing an enemy during the teleporter event.",
            new List<string>()
            {
                "ITEM_BREADLOAF_DESC"
            }
        );
        public static float healthGainOnKillPercent = healthGainOnKill.Value / 100f;

        internal static void Init()
        {
            GenerateItem();
            AddTokens();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "BREAD_LOAF";
            itemDef.nameToken = "BREAD_LOAF_NAME";
            itemDef.pickupToken = "BREAD_LOAF_PICKUP";
            itemDef.descriptionToken = "BREAD_LOAF_DESCRIPTION";
            itemDef.loreToken = "BREAD_LOAF_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier1;
            });

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("BreadLoaf.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("BreadLoaf.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Healing,

                ItemTag.OnKillEffect
            };
        }

        public static void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, eventManager, damageReport) =>
            {
                orig(eventManager, damageReport);

                if (!NetworkServer.active) return;

                CharacterBody atkBody = damageReport.attackerBody;

                if (atkBody && atkBody.inventory)
                {
                    int itemCount = atkBody.inventory.GetItemCount(itemDef);
                    if (itemCount > 0)
                    {
                        foreach (var holdoutZoneController in InstanceTracker.GetInstancesList<HoldoutZoneController>())
                        {
                            if (holdoutZoneController.isActiveAndEnabled && holdoutZoneController.IsBodyInChargingRadius(atkBody))
                            {
                                float healing = healthGainOnKillPercent * itemCount * atkBody.healthComponent.missingCombinedHealth;
                                atkBody.healthComponent.Heal(healing, new ProcChainMask());
                            }
                        }
                    }
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("BREAD_LOAF", "Loaf of Bread");
            LanguageAPI.Add("BREAD_LOAF_NAME", "Loaf of Bread");
            LanguageAPI.Add("BREAD_LOAF_PICKUP", "While the teleporter is charging, killing enemies heals you.");

            string desc = $"While the teleporter is charging, killing enemies heals you for " +
                $"<style=cIsUtility>{healthGainOnKill.Value}%</style> " +
                $"<style=cStack>(+{healthGainOnKill.Value}% per stack)</style> of your missing health.";
            LanguageAPI.Add("BREAD_LOAF_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("BREAD_LOAF_LORE", lore);
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