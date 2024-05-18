using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class EdibleGlue
    {
        public static ItemDef itemDef;

        // On kill, slow enemies within 8m (+8m per stack) by 80% for 4 seconds.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Edible Glue",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_EDIBLEGLUE_DESC"
            }
        );
        public static ConfigurableValue<float> slowRadiusPerStack = new(
            "Item: Edible Glue",
            "Slow Radius",
            8f,
            "Slow radius amount for each stack of item.",
            new List<string>()
            {
                "ITEM_EDIBLEGLUE_DESC"
            }
        );
        public static float slowDuration = 4f;

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

            itemDef.name = "EDIBLE_GLUE";
            itemDef.nameToken = "EDIBLE_GLUE_NAME";
            itemDef.pickupToken = "EDIBLE_GLUE_PICKUP";
            itemDef.descriptionToken = "EDIBLE_GLUE_DESCRIPTION";
            itemDef.loreToken = "EDIBLE_GLUE_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier1;
            });

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("EdibleGlue.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("GlueBottle.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility,

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
                    int count = atkBody.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        HurtBox[] hurtboxes = new SphereSearch
                        {
                            mask = LayerIndex.enemyBody.mask,
                            origin = atkBody.corePosition,
                            queryTriggerInteraction = QueryTriggerInteraction.Collide,
                            radius = slowRadiusPerStack.Value * count
                        }.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

                        foreach (HurtBox hb in hurtboxes)
                        {
                            CharacterBody parent = hb.healthComponent.body;
                            if (parent && parent != atkBody)
                            {
                                parent.AddTimedBuff(RoR2Content.Buffs.Slow80, slowDuration);
                            }
                        }
                    }
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("EDIBLE_GLUE", "Edible Glue");
            LanguageAPI.Add("EDIBLE_GLUE_NAME", "Edible Glue");
            LanguageAPI.Add("EDIBLE_GLUE_PICKUP", "On kill, slow nearby enemies.");

            string desc = $"On kill, slow enemies within <style=cIsUtility>{slowRadiusPerStack.Value}m</style> <style=cStack>(+{slowRadiusPerStack.Value}m per stack)</style> by 80% for 2 seconds.";
            LanguageAPI.Add("EDIBLE_GLUE_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("EDIBLE_GLUE_LORE", lore);
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