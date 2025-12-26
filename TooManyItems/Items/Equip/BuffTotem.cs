using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using TooManyItems.Managers;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems.Items.Equip
{
    internal class BuffTotem
    {
        public static EquipmentDef equipmentDef;

        public static BuffDef armorBuff;
        public static BuffDef damageBuff;
        public static BuffDef attackSpeedBuff;
        public static BuffDef healthRegenBuff;

        // On activation, grants either armor, damage, attack speed, or regeneration for a short duration.
        public static ConfigurableValue<bool> isEnabled = new(
            "Equipment: Totem of Prayer",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["EQUIPMENT_BUFFTOTEM_DESC"]
        );
        public static ConfigurableValue<float> armorIncrease = new(
            "Equipment: Totem of Prayer",
            "Armor Increase",
            100f,
            "Armor increase if rolled.",
            ["EQUIPMENT_BUFFTOTEM_DESC"]
        );
        public static ConfigurableValue<float> damageIncrease = new(
            "Equipment: Totem of Prayer",
            "Damage Increase",
            25f,
            "Percent damage increase if rolled.",
            ["EQUIPMENT_BUFFTOTEM_DESC"]
        );
        public static ConfigurableValue<float> attackSpeedIncrease = new(
            "Equipment: Totem of Prayer",
            "Attack Speed Increase",
            75f,
            "Percent attack speed increase if rolled.",
            ["EQUIPMENT_BUFFTOTEM_DESC"]
        );
        public static ConfigurableValue<float> regenIncrease = new(
            "Equipment: Totem of Prayer",
            "Health Regen Increase",
            8f,
            "Health regeneration bonus (as max HP/s) if rolled.",
            ["EQUIPMENT_BUFFTOTEM_DESC"]
        );
        public static ConfigurableValue<float> buffDuration = new(
            "Equipment: Totem of Prayer",
            "Buff Duration",
            12f,
            "Duration of the buff given.",
            new List<string>()
            {
                "EQUIPMENT_BUFFTOTEM_DESC"
            }
        );
        public static ConfigurableValue<int> equipCooldown = new(
            "Equipment: Totem of Prayer",
            "Cooldown",
            80,
            "Equipment cooldown.",
            new List<string>()
            {
                "EQUIPMENT_BUFFTOTEM_DESC"
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
            equipmentDef = ItemManager.GenerateEquipment("BuffTotem", equipCooldown.Value);

            armorBuff = ItemManager.GenerateBuff("Prayer of Defense", AssetManager.bundle.LoadAsset<Sprite>("DefensePrayer.png"));
            ContentAddition.AddBuffDef(armorBuff);
            damageBuff = ItemManager.GenerateBuff("Prayer of Power", AssetManager.bundle.LoadAsset<Sprite>("DamagePrayer.png"));
            ContentAddition.AddBuffDef(damageBuff);
            attackSpeedBuff = ItemManager.GenerateBuff("Prayer of Cadence", AssetManager.bundle.LoadAsset<Sprite>("CadencePrayer.png"));
            ContentAddition.AddBuffDef(attackSpeedBuff);
            healthRegenBuff = ItemManager.GenerateBuff("Prayer of Remedy", AssetManager.bundle.LoadAsset<Sprite>("RemedyPrayer.png"));
            ContentAddition.AddBuffDef(healthRegenBuff);

            Hooks();
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
            if (body)
            {
                Array values = Enum.GetValues(typeof(Result));
                Result r = (Result)values.GetValue(TooManyItems.RandGen.Next(values.Length));

                // Reroll once if the same buff was given last time
                if (r == lastBuffGiven)
                    r = (Result)values.GetValue(TooManyItems.RandGen.Next(values.Length));
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
                Utilities.ForceRecalculate(body);

                return true;

            }
            return false;
        }
    }
}
