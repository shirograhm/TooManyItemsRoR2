using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class TalismanItem
    {
        public static ItemDef itemDef;

        public enum Bonuses
        {
            DAMAGE, ATTACK_SPEED, MOVEMENT_SPEED, CRIT_CHANCE, CRIT_DAMAGE, HEALTH, HEALTH_REGEN, SHIELD, ARMOR,

            NUM_STATS
        }

        internal static void Init()
        {
            GenerateItem();
            AddTokens();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            NetworkingAPI.RegisterMessageType<TalismanStatistics.Sync>();

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "TALISMAN_ITEM";
            itemDef.nameToken = "TALISMAN_ITEM_NAME";
            itemDef.pickupToken = "TALISMAN_ITEM_PICKUP";
            itemDef.descriptionToken = "TALISMAN_ITEM_DESCRIPTION";
            itemDef.loreToken = "TALISMAN_ITEM_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.NoTier;
            });

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("TalismanItem.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("TalismanItem.prefab");
            itemDef.canRemove = false;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.AIBlacklist,
                ItemTag.EquipmentRelated
            };
        }

        public static void ClearStats(Inventory inventory)
        {
            var component = inventory.GetComponent<TalismanStatistics>();
            if (component)
            {
                component.BaseDamageBonus = 0;
                component.AttackSpeedPercentBonus = 0;
                component.MoveSpeedPercentBonus = 0;
                component.CritChanceBonus = 0;
                component.CritDamageBonus = 0;
                component.MaxHealthBonus = 0;
                component.RegenerationBonus = 0;
                component.ShieldBonus = 0;
                component.ArmorBonus = 0;
            }
        }

        public static void Reroll(Inventory inventory, int level)
        {
            var component = inventory.GetComponent<TalismanStatistics>();
            if (component)
            {
                ClearStats(inventory);

                float pointsRemaining = TalismanEquipment.totalPointsCap.Value * ((level + 3) / 4f);
                while (pointsRemaining > 0)
                {
                    float randomPoints;
                    if (pointsRemaining > 2)
                        randomPoints = Random.Range(0f, 4f);
                    else
                        randomPoints = pointsRemaining;

                    Bonuses chosenStat = (Bonuses)Random.Range(0, (int)Bonuses.NUM_STATS);
                    switch (chosenStat)
                    {
                        case Bonuses.DAMAGE:
                            component.BaseDamageBonus += randomPoints * TalismanEquipment.damagePerPoint.Value;
                            break;
                        case Bonuses.ATTACK_SPEED:
                            component.AttackSpeedPercentBonus += randomPoints * TalismanEquipment.attackSpeedPerPoint.Value / 100f;
                            break;
                        case Bonuses.MOVEMENT_SPEED:
                            component.MoveSpeedPercentBonus += randomPoints * TalismanEquipment.moveSpeedPerPoint.Value / 100f;
                            break;
                        case Bonuses.CRIT_CHANCE:
                            component.CritChanceBonus += randomPoints * TalismanEquipment.critChancePerPoint.Value;
                            break;
                        case Bonuses.CRIT_DAMAGE:
                            component.CritDamageBonus += randomPoints * TalismanEquipment.critDamagePerPoint.Value / 100f;
                            break;
                        case Bonuses.HEALTH:
                            component.MaxHealthBonus += randomPoints * TalismanEquipment.healthPerPoint.Value;
                            break;
                        case Bonuses.HEALTH_REGEN:
                            component.RegenerationBonus += randomPoints * TalismanEquipment.regenPerPoint.Value;
                            break;
                        case Bonuses.SHIELD:
                            component.ShieldBonus += randomPoints * TalismanEquipment.shieldPerPoint.Value;
                            break;
                        case Bonuses.ARMOR:
                            component.ArmorBonus += randomPoints * TalismanEquipment.armorPerPoint.Value;
                            break;
                        default:
                            Log.Error("Attempted to boost an invalid stat.\n");
                            break;
                    }

                    pointsRemaining -= randomPoints;
                }
            }
        }

        public static void Hooks()
        {
            CharacterMaster.onStartGlobal += (obj) =>
            {
                obj.inventory?.gameObject.AddComponent<TalismanStatistics>();
            };

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        var component = sender.inventory.GetComponent<TalismanStatistics>();
                        if(component)
                        {
                            args.baseDamageAdd += component.BaseDamageBonus;
                            args.attackSpeedMultAdd += component.AttackSpeedPercentBonus;
                            args.moveSpeedMultAdd += component.MoveSpeedPercentBonus;
                            args.critAdd += component.CritChanceBonus;
                            args.critDamageMultAdd += component.CritDamageBonus;
                            args.baseHealthAdd += component.MaxHealthBonus;
                            args.baseRegenAdd += component.RegenerationBonus;
                            args.baseShieldAdd += component.ShieldBonus;
                            args.armorAdd += component.ArmorBonus;
                        }
                    }
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("TALISMAN_ITEM", "Talisman Stat Bonuses");
            LanguageAPI.Add("TALISMAN_ITEM_NAME", "Talisman Stat Bonuses");
            LanguageAPI.Add("TALISMAN_ITEM_PICKUP", "An assortment of stat bonuses.");

            string desc = $"An assortment of stat bonuses. Activate your equipment to reroll these stats.";
            LanguageAPI.Add("TALISMAN_ITEM_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("TALISMAN_LORE", lore);
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
