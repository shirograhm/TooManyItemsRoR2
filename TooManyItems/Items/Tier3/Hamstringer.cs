using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class Hamstringer
    {
        public static ItemDef itemDef;

        public static DamageColorIndex damageColor = DamageColorAPI.RegisterDamageColor(Utils.HAMSTRINGER_COLOR);

        // Gain 3% (+3% per stack) chance to freeze enemies on-hit. You deal 90% (+90% per stack) bonus damage to frozen enemies.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: The Hamstringer",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_HAMSTRINGER_DESC"
            }
        );
        public static ConfigurableValue<float> freezeChance = new(
            "Item: The Hamstringer",
            "Freeze Chance",
            3f,
            "Chance on-hit to apply freeze.",
            new List<string>()
            {
                "ITEM_HAMSTRINGER_DESC"
            }
        );
        public static ConfigurableValue<float> frozenDamageMultiplier = new(
            "Item: The Hamstringer",
            "Bonus Damage to Frozen Enemies",
            90f,
            "Percent bonus damage dealt to frozen enemies.",
            new List<string>()
            {
                "ITEM_HAMSTRINGER_DESC"
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

            itemDef.name = "HAMSTRINGER";
            itemDef.nameToken = "HAMSTRINGER_NAME";
            itemDef.pickupToken = "HAMSTRINGER_PICKUP";
            itemDef.descriptionToken = "HAMSTRINGER_DESCRIPTION";
            itemDef.loreToken = "HAMSTRINGER_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier3;
            });

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Hamstringer.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Hamstringer.prefab");
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
            LanguageAPI.Add("HAMSTRINGER", "The Hamstringer");
            LanguageAPI.Add("HAMSTRINGER_NAME", "The Hamstringer");
            LanguageAPI.Add("HAMSTRINGER_PICKUP", "Chance to Cripple enemies on-hit. Deal bonus damage to Crippled enemies.");

            string desc = $"Gain <style=cIsUtility>{freezeChance.Value}%</style> <style=cStack>(+{freezeChance.Value}% per stack)</style> " +
                $"chance to freeze enemies on-hit. " +
                $"You deal <style=cIsDamage>{frozenDamageMultiplier.Value}% <style=cStack>(+{frozenDamageMultiplier.Value}% per stack)</style> " +
                $"bonus damage</style> to frozen enemies.";
            LanguageAPI.Add("HAMSTRINGER_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("HAMSTRINGER_LORE", lore);
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