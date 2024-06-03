using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class BuffTotem
    {
        public static EquipmentDef equipmentDef;

        public static BuffDef armorBuff;
        public static BuffDef damageBuff;
        public static BuffDef attackSpeedBuff;
        public static BuffDef healthRegenBuff;

        // On activation, grants either 100 armor, 30% damage, 75% attack speed, or 12% max HP/s regeneration for 12 seconds. (55 sec)
        public static ConfigurableValue<bool> isEnabled = new(
            "Equipment: Totem of Prayer",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_BUFFTOTEM_DESC"
            }
        );
        public static ConfigurableValue<float> armorIncrease = new(
            "Equipment: Totem of Prayer",
            "Armor Increase",
            100f,
            "Armor increase if rolled.",
            new List<string>()
            {
                "ITEM_BUFFTOTEM_DESC"
            }
        );
        public static ConfigurableValue<float> damageIncrease = new(
            "Equipment: Totem of Prayer",
            "Damage Increase",
            30f,
            "Percent damage increase if rolled.",
            new List<string>()
            {
                "ITEM_BUFFTOTEM_DESC"
            }
        );
        public static ConfigurableValue<float> attackSpeedIncrease = new(
            "Equipment: Totem of Prayer",
            "Attack Speed Increase",
            75f,
            "Percent attack speed increase if rolled.",
            new List<string>()
            {
                "ITEM_BUFFTOTEM_DESC"
            }
        );
        public static ConfigurableValue<float> regenIncrease = new(
            "Equipment: Totem of Prayer",
            "Health Regen Increase",
            12f,
            "Health regeneration bonus (as max HP/s) if rolled.",
            new List<string>()
            {
                "ITEM_BUFFTOTEM_DESC"
            }
        );
        public static ConfigurableValue<float> buffDuration = new(
            "Equipment: Totem of Prayer",
            "Buff Duration",
            12f,
            "Duration of the buff given.",
            new List<string>()
            {
                "ITEM_BUFFTOTEM_DESC"
            }
        );
        public static ConfigurableValue<int> equipCooldown = new(
            "Equipment: Totem of Prayer",
            "Cooldown",
            55,
            "Equipment cooldown.",
            new List<string>()
            {
                "ITEM_BUFFTOTEM_DESC"
            }
        );
        public static float damageIncreasePercent = damageIncrease.Value / 100f;
        public static float attackSpeedIncreasePercent = attackSpeedIncrease.Value / 100f;
        public static float regenIncreasePercent = regenIncrease.Value / 100f;

        public enum Result
        {
            ARMOR, DAMAGE, ATTACK_SPEED, HEALTH_REGEN
        }

        public static Result lastBuffGiven = Result.ARMOR;

        internal static void Init()
        {
            GenerateEquipment();
            GenerateBuff();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomEquipment(equipmentDef, displayRules));

            ContentAddition.AddBuffDef(armorBuff);
            ContentAddition.AddBuffDef(damageBuff);
            ContentAddition.AddBuffDef(attackSpeedBuff);
            ContentAddition.AddBuffDef(healthRegenBuff);

            Hooks();
        }

        private static void GenerateEquipment()
        {
            equipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();

            equipmentDef.name = "BUFF_TOTEM";
            equipmentDef.nameToken = "BUFF_TOTEM_NAME";
            equipmentDef.pickupToken = "BUFF_TOTEM_PICKUP";
            equipmentDef.descriptionToken = "BUFF_TOTEM_DESCRIPTION";
            equipmentDef.loreToken = "BUFF_TOTEM_LORE";

            equipmentDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("BuffTotem.png");
            equipmentDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("BuffTotem.prefab");

            equipmentDef.appearsInMultiPlayer = true;
            equipmentDef.appearsInSinglePlayer = true;
            equipmentDef.canBeRandomlyTriggered = true;
            equipmentDef.enigmaCompatible = true;
            equipmentDef.canDrop = true;

            equipmentDef.cooldown = equipCooldown.Value;
        }

        private static void GenerateBuff()
        {
            armorBuff = ScriptableObject.CreateInstance<BuffDef>();
            armorBuff.name = "Prayer of Defense";
            armorBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("DefensePrayer.png");
            armorBuff.canStack = false;
            armorBuff.isHidden = false;
            armorBuff.isDebuff = false;
            armorBuff.isCooldown = false;

            damageBuff = ScriptableObject.CreateInstance<BuffDef>();
            damageBuff.name = "Prayer of Power";
            damageBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("DamagePrayer.png");
            damageBuff.canStack = false;
            damageBuff.isHidden = false;
            damageBuff.isDebuff = false;
            damageBuff.isCooldown = false;

            attackSpeedBuff = ScriptableObject.CreateInstance<BuffDef>();
            attackSpeedBuff.name = "Prayer of Cadence";
            attackSpeedBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("CadencePrayer.png");
            attackSpeedBuff.canStack = false;
            attackSpeedBuff.isHidden = false;
            attackSpeedBuff.isDebuff = false;
            attackSpeedBuff.isCooldown = false;

            healthRegenBuff = ScriptableObject.CreateInstance<BuffDef>();
            healthRegenBuff.name = "Prayer of Remedy";
            healthRegenBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("RemedyPrayer.png");
            healthRegenBuff.canStack = false;
            healthRegenBuff.isHidden = false;
            healthRegenBuff.isDebuff = false;
            healthRegenBuff.isCooldown = false;
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

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    if (sender.HasBuff(armorBuff))
                    {
                        args.armorAdd += armorIncrease.Value;
                    }
                    if (sender.HasBuff(damageBuff))
                    {
                        args.damageMultAdd += damageIncreasePercent;
                    }
                    if (sender.HasBuff(attackSpeedBuff))
                    {
                        args.attackSpeedMultAdd += attackSpeedIncreasePercent;
                    }
                    if (sender.HasBuff(healthRegenBuff))
                    {
                        args.baseRegenAdd += regenIncreasePercent * sender.healthComponent.fullCombinedHealth;
                    }
                }
            };
        }

        private static bool OnUse(EquipmentSlot slot)
        {
            CharacterBody body = slot.characterBody;

            Array values = Enum.GetValues(typeof(Result));
            Result r = (Result)values.GetValue(TooManyItems.rand.Next(values.Length));

            while (r == lastBuffGiven)
            {
                r = (Result)values.GetValue(TooManyItems.rand.Next(values.Length));
            }
            lastBuffGiven = r;

            switch (r)
            {
                case Result.ARMOR:
                    body.AddTimedBuff(armorBuff, buffDuration.Value);
                    break;
                case Result.DAMAGE:
                    body.AddTimedBuff(damageBuff, buffDuration.Value);
                    break;
                case Result.ATTACK_SPEED:
                    body.AddTimedBuff(attackSpeedBuff, buffDuration.Value);
                    break;
                case Result.HEALTH_REGEN:
                    body.AddTimedBuff(healthRegenBuff, buffDuration.Value);
                    break;
            }

            Utils.ForceRecalculate(body);
            return true;
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
