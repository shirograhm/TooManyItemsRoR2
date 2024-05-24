using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class Horseshoe
    {
        public static EquipmentDef equipmentDef;

        // Gain a random assortment of stat bonuses on pickup. Activate to reroll. (190 seconds)
        public static ConfigurableValue<bool> isEnabled = new(
            "Equipment: Golden Horseshoe",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> totalPointsCap = new(
            "Equipment: Golden Horseshoe",
            "Stat Points Cap",
            20f,
            "Max value a reroll can have. This value is scaled by your level.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> damagePerPoint = new(
            "Equipment: Golden Horseshoe",
            "Damage Per Point",
            0.5f,
            "Base damage gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> attackSpeedPerPoint = new(
            "Equipment: Golden Horseshoe",
            "Attack Speed Per Point",
            3f,
            "Percent attack speed gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> moveSpeedPerPoint = new(
            "Equipment: Golden Horseshoe",
            "Move Speed Per Point",
            3f,
            "Percent movement speed gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> critChancePerPoint = new(
            "Equipment: Golden Horseshoe",
            "Crit Chance Per Point",
            1f,
            "Percent crit chance gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> critDamagePerPoint = new(
            "Equipment: Golden Horseshoe",
            "Crit Damage Per Point",
            4f,
            "Percent crit damage gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> healthPerPoint = new(
            "Equipment: Golden Horseshoe",
            "Health Per Point",
            12f,
            "Max health gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> regenPerPoint = new(
            "Equipment: Golden Horseshoe",
            "Regeneration Per Point",
            0.8f,
            "Regeneration gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> shieldPerPoint = new(
            "Equipment: Golden Horseshoe",
            "Shield Per Point",
            12f,
            "Shield gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> armorPerPoint = new(
            "Equipment: Golden Horseshoe",
            "Armor Per Point",
            2f,
            "Armor gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<float> cooldownReductionPerPoint = new(
            "Equipment: Golden Horseshoe",
            "Cooldown Reduction Per Point",
            3.5f,
            "Percent cooldown reduction gained per stat point invested.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );
        public static ConfigurableValue<int> equipCooldown = new(
            "Equipment: Golden Horseshoe",
            "Cooldown",
            160,
            "Equipment cooldown.",
            new List<string>()
            {
                "ITEM_HORSESHOE_DESC"
            }
        );

        internal static void Init()
        {
            GenerateEquipment();
            AddTokens();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomEquipment(equipmentDef, displayRules));

            Hooks();
        }

        private static void GenerateEquipment()
        {
            equipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();

            equipmentDef.name = "HORSESHOE_EQUIPMENT";
            equipmentDef.nameToken = "HORSESHOE_EQUIPMENT_NAME";
            equipmentDef.pickupToken = "HORSESHOE_EQUIPMENT_PICKUP";
            equipmentDef.descriptionToken = "HORSESHOE_EQUIPMENT_DESCRIPTION";
            equipmentDef.loreToken = "HORSESHOE_EQUIPMENT_LORE";

            equipmentDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Horseshoe.png");
            equipmentDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Horseshoe.prefab");

            equipmentDef.appearsInMultiPlayer = true;
            equipmentDef.appearsInSinglePlayer = true;
            equipmentDef.canBeRandomlyTriggered = false;
            equipmentDef.enigmaCompatible = false;
            equipmentDef.canDrop = true;

            equipmentDef.cooldown = equipCooldown.Value;
        }
        
        public static void Hooks()
        {
            On.RoR2.Inventory.UpdateEquipment += (orig, self) =>
            {
                CharacterMaster master = self.GetComponentInParent<CharacterMaster>();

                if (self && master && master.GetBody())
                {
                    if (self.GetEquipmentIndex() != equipmentDef.equipmentIndex)
                    {
                        if(self.GetItemCount(HorseshoeHelperItem.itemDef) > 0)
                        {
                            // Remove stacks of horseshoe helper item if still present
                            self.RemoveItem(HorseshoeHelperItem.itemDef);
                        }
                    }
                }
            };

            On.RoR2.EquipmentSlot.PerformEquipmentAction += (orig, self, equipDef) =>
            {
                if (NetworkServer.active && equipDef == equipmentDef)
                {
                    return OnUse(self);
                }
                return orig(self, equipDef);
            };
        }

        private static bool OnUse(EquipmentSlot slot)
        {
            CharacterBody body = slot.characterBody;
            if (body && body.inventory)
            {
                int count = body.inventory.GetItemCount(HorseshoeHelperItem.itemDef);
                if (count == 0)
                    body.inventory.GiveItem(HorseshoeHelperItem.itemDef);
             
                HorseshoeHelperItem.Reroll(body.inventory, body);
            }

            Utils.ForceRecalculate(body);
            return true;
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("HORSESHOE_EQUIPMENT", "Golden Horseshoe");
            LanguageAPI.Add("HORSESHOE_EQUIPMENT_NAME", "Golden Horseshoe");
            LanguageAPI.Add("HORSESHOE_EQUIPMENT_PICKUP", "Gain a random assortment of stat bonuses that are <style=cWorldEvent>rerolled</style> upon <style=cIsUtility>level up</style>. Activate to <style=cWorldEvent>reroll</style> manually.");

            string desc = $"Gain a random assortment of " +
                $"<style=cIsDamage>base damage</style>, " +
                $"<style=cIsDamage>attack speed</style>, " +
                $"<style=cIsDamage>crit chance</style>, " +
                $"<style=cIsDamage>crit damage</style>, " +
                $"<style=cEvent>armor</style>, " +
                $"<style=cIsHealing>regeneration</style>, " +
                $"<style=cIsHealth>health</style>, " +
                $"<style=cIsUtility>shield</style>, " +
                $"<style=cIsUtility>movement speed</style>, and " +
                $"<style=cIsUtility>cooldown reduction</style>. " +
                $"These stats are <style=cWorldEvent>rerolled</style> upon <style=cIsUtility>level up</style>. " +
                $"Activate to <style=cWorldEvent>reroll</style> manually.";
            LanguageAPI.Add("HORSESHOE_EQUIPMENT_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("HORSESHOE_EQUIPMENT_LORE", lore);
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
