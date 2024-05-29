using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class Permafrost
    {
        public static ItemDef itemDef;

        public static DamageColorIndex damageColor = DamageColorAPI.RegisterDamageColor(Utils.PERMAFROST_COLOR);

        // Dealing damage has a 5% (+5% per stack) chance to freeze enemies. You deal 75% (+75% per stack) bonus damage to frozen enemies.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Permafrost",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_PERMAFROST_DESC"
            }
        );
        public static ConfigurableValue<float> freezeChance = new(
            "Item: Permafrost",
            "Freeze Chance",
            5f,
            "Chance to apply freeze when dealing damage.",
            new List<string>()
            {
                "ITEM_PERMAFROST_DESC"
            }
        );
        public static ConfigurableValue<float> frozenDamageMultiplier = new(
            "Item: Permafrost",
            "Bonus Damage to Frozen Enemies",
            75f,
            "Percent bonus damage dealt to frozen enemies.",
            new List<string>()
            {
                "ITEM_PERMAFROST_DESC"
            }
        );
        public static float freezeChancePercent = freezeChance.Value / 100.0f;
        public static float frozenDamageMultiplierPercent = frozenDamageMultiplier.Value / 100.0f;

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

            itemDef.name = "PERMAFROST";
            itemDef.nameToken = "PERMAFROST_NAME";
            itemDef.pickupToken = "PERMAFROST_PICKUP";
            itemDef.descriptionToken = "PERMAFROST_DESCRIPTION";
            itemDef.loreToken = "PERMAFROST_LORE";

            Utils.SetItemTier(itemDef, ItemTier.Tier3);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Permafrost.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Permafrost.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage,
                ItemTag.Utility
            };
        }

        public static void Hooks()
        {
            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                CharacterBody attackerBody = attackerInfo.body;
                CharacterBody victimBody = victimInfo.body;
                if (attackerBody && victimBody && attackerBody.inventory)
                {
                    int count = attackerBody.inventory.GetItemCount(itemDef);
                    if (count > 0 && attackerBody.master)
                    {
                        if (Util.CheckRoll(Utils.GetHyperbolicStacking(freezeChancePercent, count) * 100f, attackerBody.master.luck, attackerBody.master))
                        {
                            damageInfo.damageType |= DamageType.Freeze2s;
                        }

                        if (victimBody.healthComponent.isInFrozenState)
                        {
                            damageInfo.damage *= 1 + frozenDamageMultiplierPercent * count;
                            damageInfo.damageColorIndex = damageColor;
                        }
                    }
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("PERMAFROST", "Permafrost");
            LanguageAPI.Add("PERMAFROST_NAME", "Permafrost");
            LanguageAPI.Add("PERMAFROST_PICKUP", "Chance to freeze enemies on-hit. Deal bonus damage to frozen enemies.");

            string desc = $"Dealing damage has a <style=cIsUtility>{freezeChance.Value}%</style> <style=cStack>(+{freezeChance.Value}% per stack)</style> " +
                $"chance to freeze enemies. " +
                $"You deal <style=cIsDamage>{frozenDamageMultiplier.Value}%</style> <style=cStack>(+{frozenDamageMultiplier.Value}% per stack)</style>" +
                $" bonus damage to frozen enemies.";
            LanguageAPI.Add("PERMAFROST_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("PERMAFROST_LORE", lore);
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