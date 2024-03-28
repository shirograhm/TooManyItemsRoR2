using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class Chalice
    {
        public static EquipmentDef equipmentDef;
        public static BuffDef consecratedBuff;

        public static DamageColorIndex damageColor = DamageColorAPI.RegisterDamageColor(new(0.55f, 0.12f, 0.09f, 1f));

        // Consecrate yourself and all allies. Consecrated allies gain 40% damage and 110% attack speed for 8 seconds, but lose 50% max health. (60 sec)
        public static ConfigurableValue<float> consecrateDuration = new(
            "Equipment: Chalice",
            "Consecrate Duration",
            8f,
            "Duration of the Consecrate buff given.",
            new List<string>()
            {
                "ITEM_CHALICE_DESC"
            }
        );
        public static ConfigurableValue<float> consecrateDamageBonus = new(
            "Equipment: Chalice",
            "Consecrate Damage Bonus",
            40f,
            "Percent bonus damage dealt while Consecrated.",
            new List<string>()
            {
                "ITEM_CHALICE_DESC"
            }
        );
        public static ConfigurableValue<float> consecrateAttackSpeedBonus = new(
            "Equipment: Chalice",
            "Consecrate Attack Speed Bonus",
            110f,
            "Percent bonus attack speed gained while Consecrated.",
            new List<string>()
            {
                "ITEM_CHALICE_DESC"
            }
        );
        public static ConfigurableValue<float> consecrateMaxHealthLost = new(
            "Equipment: Chalice",
            "Max Health Loss",
            50f,
            "Percent of max health lost when Consecrated.",
            new List<string>()
            {
                "ITEM_CHALICE_DESC"
            }
        );
        public static ConfigurableValue<int> equipCooldown = new(
            "Equipment: Chalice",
            "Cooldown",
            60,
            "Equipment cooldown.",
            new List<string>()
            {
                "ITEM_CHALICE_DESC"
            }
        );

        internal static void Init()
        {
            GenerateEquipment();
            GenerateBuff();
            AddTokens();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomEquipment(equipmentDef, displayRules));

            ContentAddition.AddBuffDef(consecratedBuff);

            Hooks();
        }

        private static void GenerateEquipment()
        {
            equipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();

            equipmentDef.name = "CHALICE";
            equipmentDef.nameToken = "CHALICE_NAME";
            equipmentDef.pickupToken = "CHALICE_PICKUP";
            equipmentDef.descriptionToken = "CHALICE_DESCRIPTION";
            equipmentDef.loreToken = "CHALICE_LORE";

            equipmentDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Chalice.png");
            equipmentDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Chalice.prefab");

            equipmentDef.isLunar = true;
            equipmentDef.colorIndex = ColorCatalog.ColorIndex.LunarItem;

            equipmentDef.appearsInMultiPlayer = true;
            equipmentDef.appearsInSinglePlayer = true;
            equipmentDef.canBeRandomlyTriggered = true;
            equipmentDef.enigmaCompatible = true;
            equipmentDef.canDrop = true;

            equipmentDef.cooldown = equipCooldown.Value;
        }

        private static void GenerateBuff()
        {
            consecratedBuff = ScriptableObject.CreateInstance<BuffDef>();

            consecratedBuff.name = "Consecrated";
            consecratedBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("ConsecratedBuff.png");
            consecratedBuff.canStack = false;
            consecratedBuff.isHidden = false;
            consecratedBuff.isDebuff = false;
            consecratedBuff.isCooldown = false;
        }

        public static void Hooks()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += (orig, self, equipDef) =>
            {
                if (NetworkServer.active && equipDef == equipmentDef)
                {
                    return OnUse(self);
                }
                return orig(self, equipDef);
            };

            On.RoR2.HealthComponent.GetHealthBarValues += (orig, self) =>
            {
                HealthComponent.HealthBarValues values = orig(self);
                if (self.body && self.body.HasBuff(consecratedBuff))
                {
                    values.curseFraction += (1f - values.curseFraction) * consecrateMaxHealthLost.Value / 100f;
                    values.healthFraction = self.health * (1f - values.curseFraction) / self.fullCombinedHealth;
                    values.shieldFraction = self.shield * (1f - values.curseFraction) / self.fullCombinedHealth;
                }
                return values;
            };

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    if (sender.HasBuff(consecratedBuff))
                    {
                        args.healthMultAdd -= consecrateMaxHealthLost.Value / 100f;
                        args.attackSpeedMultAdd += consecrateAttackSpeedBonus.Value / 100f;
                        args.damageMultAdd += consecrateDamageBonus.Value / 100f;
                    }
                }
            };
        }

        private static bool OnUse(EquipmentSlot slot)
        {
            // Add consecrated buff to all allies and pay max health 
            if (slot.characterBody)
            {
                foreach (TeamComponent component in TeamComponent.GetTeamMembers(slot.characterBody.teamComponent.teamIndex))
                {
                    component.body.AddTimedBuff(consecratedBuff, consecrateDuration.Value);
                }
            }
            return true;
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("CHALICE", "Chalice");
            LanguageAPI.Add("CHALICE_NAME", "Chalice");
            LanguageAPI.Add("CHALICE_PICKUP", "Consecrate all allies for a short duration. " +
                "Consecrated allies gain bonus damage and attack speed, but lose max health.");

            string desc = $"Consecrate yourself and all allies. Consecrated allies gain " +
                $"<style=cIsDamage>{consecrateDamageBonus.Value}%</style> damage and " +
                $"<style=cIsUtility>{consecrateAttackSpeedBonus.Value}%</style> attack speed for " +
                $"<style=cIsUtility>{consecrateDuration.Value}</style> seconds, " +
                $"<style=cDeath>but lose {consecrateMaxHealthLost.Value}% max health.</style>";
            LanguageAPI.Add("CHALICE_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("CHALICE_LORE", lore);
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
