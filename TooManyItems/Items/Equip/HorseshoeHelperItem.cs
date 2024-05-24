using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class HorseshoeHelperItem
    {
        public static ItemDef itemDef;

        public enum Bonuses
        {
            DAMAGE, 
            ATTACK_SPEED, 
            CRIT_CHANCE, 
            CRIT_DAMAGE, 
            ARMOR,
            HEALTH_REGEN,
            HEALTH, 
            SHIELD,
            MOVEMENT_SPEED,
            COOLDOWN_REDUCTION,

            NUM_STATS
        }

        internal static void Init()
        {
            GenerateItem();
            AddTokens();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            NetworkingAPI.RegisterMessageType<HorseshoeStatistics.Sync>();

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "HORSESHOE_ITEM";
            itemDef.nameToken = "HORSESHOE_ITEM_NAME";
            itemDef.pickupToken = "HORSESHOE_ITEM_PICKUP";
            itemDef.descriptionToken = "HORSESHOE_ITEM_DESCRIPTION";
            itemDef.loreToken = "HORSESHOE_ITEM_LORE";

            Utils.SetItemTier(itemDef, ItemTier.NoTier);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("HorseshoeItem.png");
            itemDef.canRemove = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.AIBlacklist,
                ItemTag.CannotDuplicate,
                ItemTag.CannotSteal,
                ItemTag.CannotCopy
            };
        }

        public static void ClearStats(Inventory inventory)
        {
            var component = inventory.GetComponent<HorseshoeStatistics>();
            if (component)
            {
                component.BaseDamageBonus = 0;
                component.AttackSpeedPercentBonus = 0;
                component.CritChanceBonus = 0;
                component.CritDamageBonus = 0;
                component.ArmorBonus = 0;
                component.RegenerationBonus = 0;
                component.MaxHealthBonus = 0;
                component.ShieldBonus = 0;
                component.MoveSpeedPercentBonus = 0;
                component.CooldownReductionBonus = 0;
            }
        }

        public static void Reroll(Inventory inventory, CharacterBody body)
        {
            if (inventory.GetItemCount(itemDef) == 0 && body.equipmentSlot.equipmentIndex == Horseshoe.equipmentDef.equipmentIndex)
            {
                body.inventory.GiveItem(itemDef);
            }

            var component = inventory.GetComponent<HorseshoeStatistics>();
            if (component)
            {
                ClearStats(inventory);

                float pointsRemaining = Horseshoe.totalPointsCap.Value * ((body.level + 5) / 6f);
                while (pointsRemaining > 0)
                {
                    float randomPoints;
                    float step = 2.3f;
                    if (pointsRemaining > step)
                        randomPoints = Random.Range(0, step * 2);
                    else
                        randomPoints = pointsRemaining;

                    Bonuses chosenStat = (Bonuses)Random.Range(0, (int)Bonuses.NUM_STATS);
                    switch (chosenStat)
                    {
                        case Bonuses.DAMAGE:
                            component.BaseDamageBonus += randomPoints * Horseshoe.damagePerPoint.Value;
                            break;
                        case Bonuses.ATTACK_SPEED:
                            component.AttackSpeedPercentBonus += randomPoints * Horseshoe.attackSpeedPerPoint.Value / 100f;
                            break;
                        case Bonuses.CRIT_CHANCE:
                            component.CritChanceBonus += randomPoints * Horseshoe.critChancePerPoint.Value;
                            break;
                        case Bonuses.CRIT_DAMAGE:
                            component.CritDamageBonus += randomPoints * Horseshoe.critDamagePerPoint.Value / 100f;
                            break;
                        case Bonuses.ARMOR:
                            component.ArmorBonus += randomPoints * Horseshoe.armorPerPoint.Value;
                            break;
                        case Bonuses.HEALTH_REGEN:
                            component.RegenerationBonus += randomPoints * Horseshoe.regenPerPoint.Value;
                            break;
                        case Bonuses.HEALTH:
                            component.MaxHealthBonus += randomPoints * Horseshoe.healthPerPoint.Value;
                            break;
                        case Bonuses.SHIELD:
                            component.ShieldBonus += randomPoints * Horseshoe.shieldPerPoint.Value;
                            break;
                        case Bonuses.MOVEMENT_SPEED:
                            component.MoveSpeedPercentBonus += randomPoints * Horseshoe.moveSpeedPerPoint.Value / 100f;
                            break;
                        case Bonuses.COOLDOWN_REDUCTION:
                            component.CooldownReductionBonus += randomPoints * Horseshoe.cooldownReductionPerPoint.Value / 100f;
                            break;
                        default:
                            Log.Error("Attempted to boost an invalid stat.\n");
                            break;
                    }

                    pointsRemaining -= randomPoints;
                }
            }
            Utils.ForceRecalculate(body);
        }

        public static void Hooks()
        {
            CharacterMaster.onStartGlobal += (obj) =>
            {
                obj.inventory?.gameObject.AddComponent<HorseshoeStatistics>();
            };

            On.RoR2.CharacterBody.OnLevelUp += (orig, self) =>
            {
                orig(self);

                if (self && self.inventory)
                {
                    Reroll(self.inventory, self);
                }
            };

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        var component = sender.inventory.GetComponent<HorseshoeStatistics>();
                        if(component)
                        {
                            args.baseDamageAdd += component.BaseDamageBonus;
                            args.attackSpeedMultAdd += component.AttackSpeedPercentBonus;
                            args.critAdd += component.CritChanceBonus;
                            args.critDamageMultAdd += component.CritDamageBonus;
                            args.armorAdd += component.ArmorBonus;
                            args.baseRegenAdd += component.RegenerationBonus;
                            args.baseHealthAdd += component.MaxHealthBonus;
                            args.baseShieldAdd += component.ShieldBonus;
                            args.moveSpeedMultAdd += component.MoveSpeedPercentBonus;
                            args.cooldownMultAdd -= component.CooldownReductionBonus;
                        }
                    }
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("HORSESHOE_ITEM", "Horseshoe\'s Handout");
            LanguageAPI.Add("HORSESHOE_ITEM_NAME", "Horseshoe\'s Handout");
            LanguageAPI.Add("HORSESHOE_ITEM_PICKUP", "An assortment of stat bonuses.");

            string desc = $"An assortment of stat bonuses that are <style=cWorldEvent>rerolled</style> upon equipment activation or <style=cIsUtility>level up</style>.";
            LanguageAPI.Add("HORSESHOE_ITEM_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("HORSESHOE_ITEM_LORE", lore);
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
