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

        // Your attacks have a 6% (+2% per stack) chance on-hit to Cripple enemies for 3 (+3 per stack) seconds. Deal 90% bonus damage to Cripple enemies.
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
        public static ConfigurableValue<float> crippleChance = new(
            "Item: The Hamstringer",
            "Cripple Chance",
            6f,
            "Chance to Cripple enemies on-hit.",
            new List<string>()
            {
                "ITEM_HAMSTRINGER_DESC"
            }
        );
        public static ConfigurableValue<float> crippleChanceExtra = new(
            "Item: The Hamstringer",
            "Cripple Chance Additional Stacks",
            2f,
            "Chance to Cripple enemies on-hit for each additional stack of this item.",
            new List<string>()
            {
                "ITEM_HAMSTRINGER_DESC"
            }
        );
        public static ConfigurableValue<float> crippleDuration = new(
            "Item: The Hamstringer",
            "Cripple Duration",
            3f,
            "Duration of Cripple debuff.",
            new List<string>()
            {
                "ITEM_HAMSTRINGER_DESC"
            }
        );
        public static ConfigurableValue<float> crippleDamageBonus = new(
            "Item: The Hamstringer",
            "Cripple Damage Bonus",
            90f,
            "Percent bonus damage dealt to Crippleed enemies.",
            new List<string>()
            {
                "ITEM_HAMSTRINGER_DESC"
            }
        );
        public static float crippleChancePercent = crippleChance.Value / 100.0f;
        public static float crippleDamageBonusPercent = crippleDamageBonus.Value / 100.0f;

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
            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victim) =>
            {
                orig(self, damageInfo, victim);

                if (!NetworkServer.active) return;
                if (damageInfo.attacker == null || victim == null) return;

                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                CharacterBody victimBody = victim.GetComponent<CharacterBody>();

                if (attackerBody != null && attackerBody.inventory != null)
                {
                    CharacterMaster atkMaster = attackerBody.master;

                    int count = attackerBody.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        if (Util.CheckRoll(crippleChance.Value + crippleChanceExtra.Value, atkMaster.luck, atkMaster))
                        {
                            victimBody.AddTimedBuff(RoR2Content.Buffs.Weak, crippleDuration * count);
                        }
                    }
                }
            };

            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (attackerInfo.inventory == null || victimInfo.body == null) return;

                int count = attackerInfo.inventory.GetItemCount(itemDef);
                if (count > 0 && victimInfo.body.HasBuff(RoR2Content.Buffs.Weak))
                {
                    float percentMultiplier = 1f + crippleDamageBonusPercent + crippleDamageBonusExtraPercent * count;
                    damageInfo.damage *= percentMultiplier;
                    damageInfo.damageColorIndex = damageColor;
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("HAMSTRINGER", "The Hamstringer");
            LanguageAPI.Add("HAMSTRINGER_NAME", "The Hamstringer");
            LanguageAPI.Add("HAMSTRINGER_PICKUP", "Chance to Cripple enemies on-hit. Deal bonus damage to Crippleed enemies.");

            string desc = $"Your attacks have a " +
                $"<style=cIsUtility>{crippleChance.Value}%</style> chance on-hit to Cripple enemies for " +
                $"<style=cIsUtility>{crippleDuration.Value} <style=cStack>(+{crippleDuration.Value} per stack)</style> seconds</style>. Deal " +
                $"<style=cIsDamage>{crippleDamageBonus.Value}% <style=cStack>(+{crippleDamageBonusExtraStack.Value}% per stack)</style> bonus damage</style> to Crippleed enemies.";
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