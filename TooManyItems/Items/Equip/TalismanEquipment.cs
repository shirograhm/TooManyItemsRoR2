using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class TalismanEquipment
    {
        public static EquipmentDef equipmentDef;

        // Gain a random assortment of stat bonuses on pickup. Activate to reroll. (190 seconds)
        public static ConfigurableValue<bool> isEnabled = new(
            "Equipment: Talisman Equipment",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_TALISMANEQUIP_DESC"
            }
        );
        public static ConfigurableValue<float> totalPointsCap = new(
            "Equipment: Talisman Equipment",
            "Stat Points Cap",
            20f,
            "Max value a reroll can have. This value is scaled by your level.",
            new List<string>()
            {
                "ITEM_TALISMANEQUIP_DESC"
            }
        );
        public static ConfigurableValue<float> damagePerPoint = new(
            "Equipment: Talisman Equipment",
            "Damage Per Point",
            0.5f,
            "Base damage gained per stat point invested.",
            new List<string>()
            {
                "ITEM_TALISMANEQUIP_DESC"
            }
        );
        public static ConfigurableValue<float> attackSpeedPerPoint = new(
            "Equipment: Talisman Equipment",
            "Attack Speed Per Point",
            4f,
            "Percent attack speed gained per stat point invested.",
            new List<string>()
            {
                "ITEM_TALISMANEQUIP_DESC"
            }
        );
        public static ConfigurableValue<float> moveSpeedPerPoint = new(
            "Equipment: Talisman Equipment",
            "Move Speed Per Point",
            4f,
            "Percent movement speed gained per stat point invested.",
            new List<string>()
            {
                "ITEM_TALISMANEQUIP_DESC"
            }
        );
        public static ConfigurableValue<float> critChancePerPoint = new(
            "Equipment: Talisman Equipment",
            "Crit Chance Per Point",
            2f,
            "Percent crit chance gained per stat point invested.",
            new List<string>()
            {
                "ITEM_TALISMANEQUIP_DESC"
            }
        );
        public static ConfigurableValue<float> critDamagePerPoint = new(
            "Equipment: Talisman Equipment",
            "Crit Damage Per Point",
            5f,
            "Percent crit damage gained per stat point invested.",
            new List<string>()
            {
                "ITEM_TALISMANEQUIP_DESC"
            }
        );
        public static ConfigurableValue<float> healthPerPoint = new(
            "Equipment: Talisman Equipment",
            "Health Per Point",
            12f,
            "Max health gained per stat point invested.",
            new List<string>()
            {
                "ITEM_TALISMANEQUIP_DESC"
            }
        );
        public static ConfigurableValue<float> regenPerPoint = new(
            "Equipment: Talisman Equipment",
            "Regeneration Per Point",
            1.2f,
            "Regeneration gained per stat point invested.",
            new List<string>()
            {
                "ITEM_TALISMANEQUIP_DESC"
            }
        );
        public static ConfigurableValue<float> shieldPerPoint = new(
            "Equipment: Talisman Equipment",
            "Shield Per Point",
            20f,
            "Shield gained per stat point invested.",
            new List<string>()
            {
                "ITEM_TALISMANEQUIP_DESC"
            }
        );
        public static ConfigurableValue<float> armorPerPoint = new(
            "Equipment: Talisman Equipment",
            "Armor Per Point",
            3f,
            "Armor gained per stat point invested.",
            new List<string>()
            {
                "ITEM_TALISMANEQUIP_DESC"
            }
        );
        public static ConfigurableValue<int> equipCooldown = new(
            "Equipment: Talisman Equipment",
            "Cooldown",
            190,
            "Equipment cooldown.",
            new List<string>()
            {
                "ITEM_TALISMANEQUIP_DESC"
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

            equipmentDef.name = "TALISMAN_EQUIPMENT";
            equipmentDef.nameToken = "TALISMAN_EQUIPMENT_NAME";
            equipmentDef.pickupToken = "TALISMAN_EQUIPMENT_PICKUP";
            equipmentDef.descriptionToken = "TALISMAN_EQUIPMENT_DESCRIPTION";
            equipmentDef.loreToken = "TALISMAN_EQUIPMENT_LORE";

            equipmentDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("TalismanEquipment.png");
            equipmentDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("TalismanEquipment.prefab");

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
                CharacterBody body = self.GetComponentInParent<CharacterBody>();

                if (self && body)
                {
                    if (self.GetEquipmentIndex() == equipmentDef.equipmentIndex)
                    {
                        if (self.GetItemCount(TalismanItem.itemDef) == 0)
                        {
                            self.GiveItem(TalismanItem.itemDef);
                            TalismanItem.Reroll(self, (int) body.level);
                        }
                    }
                    else
                    {
                        if(self.GetItemCount(TalismanItem.itemDef) > 0)
                        {
                            self.RemoveItem(TalismanItem.itemDef);
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
                int count = body.inventory.GetItemCount(TalismanItem.itemDef);
                if (count > 0)
                {
                    TalismanItem.Reroll(body.inventory, (int) body.level);
                }
            }

            return true;
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("TALISMAN_EQUIPMENT", "Talisman");
            LanguageAPI.Add("TALISMAN_EQUIPMENT_NAME", "Talisman");
            LanguageAPI.Add("TALISMAN_EQUIPMENT_PICKUP", "Gain a random assortment of stat bonuses. Activate this equipment to reroll.");

            string desc = $"Gain a random assortment of damage, attack speed, movement speed, crit chance, crit damage, max health, health regeneration, max shield, and armor, <style=cShrine>scaling with difficulty</style>. " +
                $"Activate this equipment to <style=cWorldEvent>reroll</style> your stat bonuses with the current difficulty modifier.";
            LanguageAPI.Add("TALISMAN_EQUIPMENT_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("TALISMAN_EQUIPMENT_LORE", lore);
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
