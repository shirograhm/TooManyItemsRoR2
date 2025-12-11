using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class PropellerHat
    {
        public static ItemDef itemDef;

        // Increase damage while airborne.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Propeller Hat",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_PROPELLERHAT_DESC"
            }
        );
        public static ConfigurableValue<float> movespeedBonus = new(
            "Item: Propeller Hat",
            "Movement Speed",
            16f,
            "Percent bonus movement speed per stack while airborne.",
            new List<string>()
            {
                "ITEM_PROPELLERHAT_DESC"
            }
        );
        public static ConfigurableValue<float> fallDamageTaken = new(
            "Item: Propeller Hat",
            "Fall Damage Taken",
            20f,
            "Percent fall damage taken while holding this item.",
            new List<string>()
            {
                "ITEM_PROPELLERHAT_DESC"
            }
        );
        public static float movespeedBonusPercent = movespeedBonus.Value / 100f;
        public static float fallDamageTakenPercent = fallDamageTaken.Value / 100f;

        internal static void Init()
        {
            GenerateItem();

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict();
            displayRules.Add("mdlCommandoDualies", Utils.GenerateItemDisplayRule(itemDef, "Head", new Vector3(0f, 0.38f, 0.023f), new Vector3(0f, 40f, 10f), new Vector3(1.5f, 1.5f, 1.5f)));
            displayRules.Add("mdlHuntress", Utils.GenerateItemDisplayRule(itemDef, "Head", new Vector3(0f, 0.3f, 0.013f), new Vector3(0f, 40f, 10f), new Vector3(1.2f, 1.2f, 1.2f)));
            displayRules.Add("mdlBandit2", Utils.GenerateItemDisplayRule(itemDef, "Hat", new Vector3(0f, 0.25f, 0.023f), new Vector3(0f, 40f, 10f), new Vector3(1.2f, 1.2f, 1.2f)));
            displayRules.Add("mdlToolbot", Utils.GenerateItemDisplayRule(itemDef, "Head", new Vector3(0f, 1.3f, 0.023f), new Vector3(0f, 50f, 10f), new Vector3(15f, 15f, 15f)));
            displayRules.Add("mdlEngi", Utils.GenerateItemDisplayRule(itemDef, "HeadCenter", new Vector3(0f, 0.18f, 0.023f), new Vector3(0f, 50f, 10f), new Vector3(1.5f, 1.5f, 1.5f)));
            displayRules.Add("mdlEngiTurret", Utils.GenerateItemDisplayRule(itemDef, "Head", new Vector3(0f, 1.8f, 0.53f), new Vector3(0f, 50f, 10f), new Vector3(3f, 3f, 3f)));
            displayRules.Add("mdlMage", Utils.GenerateItemDisplayRule(itemDef, "Head", new Vector3(0f, 0.17f, 0.004f), new Vector3(0f, -20f, 10f), new Vector3(1.2f, 1.2f, 1.2f)));
            displayRules.Add("mdlMerc", Utils.GenerateItemDisplayRule(itemDef, "Head", new Vector3(0f, 0.25f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1.5f, 1.5f, 1.5f)));
            displayRules.Add("mdlTreebot", Utils.GenerateItemDisplayRule(itemDef, "PlatformBase", new Vector3(0f, 2.2f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1f, 1f, 1f)));
            displayRules.Add("mdlLoader", Utils.GenerateItemDisplayRule(itemDef, "Head", new Vector3(0f, 0.2f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1.5f, 1.5f, 1.5f)));
            displayRules.Add("mdlCroco", Utils.GenerateItemDisplayRule(itemDef, "Head", new Vector3(0f, 1.5f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1f, 1f, 1f)));
            displayRules.Add("mdlCaptain", Utils.GenerateItemDisplayRule(itemDef, "Hat", new Vector3(0f, 0.5f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1f, 1f, 1f)));
            displayRules.Add("mdlRailGunner", Utils.GenerateItemDisplayRule(itemDef, "Head", new Vector3(0f, 0.2f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1f, 1f, 1f)));
            displayRules.Add("mdlVoidSurvivor", Utils.GenerateItemDisplayRule(itemDef, "Neck", new Vector3(0f, 0.28f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1.5f, 1.5f, 1.5f)));
            //displayRules.Add("mdlSeeker", Utils.GenerateItemDisplayRule(itemDef, "Head", new Vector3(0f, 0.5f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1f, 1f, 1f)));
            //displayRules.Add("mdlChef", Utils.GenerateItemDisplayRule(itemDef, "Head", new Vector3(0f, 0.5f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1f, 1f, 1f)));
            //displayRules.Add("mdlFalseSon", Utils.GenerateItemDisplayRule(itemDef, "Head", new Vector3(0f, 0.5f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1f, 1f, 1f)));
            //displayRules.Add("mdlScav", Utils.GenerateItemDisplayRule(itemDef, "Head", new Vector3(0f, 0.5f, 0.023f), new Vector3(0f, -20f, 10f), new Vector3(1f, 1f, 1f)));
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }



        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "PROPELLERHAT";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier1);

            GameObject prefab = AssetHandler.bundle.LoadAsset<GameObject>("PropellerHat.prefab");
            ModelPanelParameters modelPanelParameters = prefab.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = prefab.transform;
            modelPanelParameters.cameraPositionTransform = prefab.transform;
            modelPanelParameters.maxDistance = 10f;
            modelPanelParameters.minDistance = 5f;

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("PropellerHat.png");
            itemDef.pickupModelPrefab = prefab;
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility,

                ItemTag.CanBeTemporary
            };
        }

        public static void Hooks()
        {
            On.RoR2.CharacterBody.FixedUpdate += (orig, self) =>
            {
                orig(self);

                if (self && self.inventory)
                {
                    if (self.inventory.GetItemCountEffective(itemDef) > 0)
                    {
                        Utils.ForceRecalculate(self);
                    }
                }
            };

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCountEffective(itemDef);
                    if (count > 0 && sender.characterMotor && !sender.characterMotor.isGrounded)
                    {
                        args.moveSpeedMultAdd += movespeedBonusPercent * count;
                    }
                }
            };

            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                if (victimInfo.inventory == null || victimInfo.body == null) return;

                int count = victimInfo.inventory.GetItemCountEffective(itemDef);
                if (count > 0 && damageInfo.damageType == DamageType.FallDamage)
                {
                    damageInfo.damage *= fallDamageTakenPercent;
                }
            };
        }
    }
}
