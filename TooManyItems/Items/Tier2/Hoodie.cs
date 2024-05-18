using R2API;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TooManyItems
{
    internal class Hoodie
    {
        public static ItemDef itemDef;
        public static BuffDef hoodieBuffActive;
        public static BuffDef hoodieBuffCooldown;

        // The next timed buff received has its duration increased by 40% (+40% per stack). Recharges every 8 (-15% per stack) seconds.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Fleece Hoodie",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_HOODIE_DESC"
            }
        );
        public static ConfigurableValue<float> durationIncrease = new(
            "Item: Fleece Hoodie",
            "Duration Increase",
            40f,
            "Buff duration percentage multiplier per stack.",
            new List<string>()
            {
                "ITEM_HOODIE_DESC"
            }
        );
        public static ConfigurableValue<float> rechargeTime = new(
            "Item: Fleece Hoodie",
            "Recharge Time",
            8f,
            "Time this item takes to recharge.",
            new List<string>()
            {
                "ITEM_HOODIE_DESC"
            }
        );
        public static ConfigurableValue<float> rechargeTimeReductionPerStack = new(
            "Item: Fleece Hoodie",
            "Recharge Time Reduction",
            15f,
            "Percent of recharge time removed for every additional stack of this item.",
            new List<string>()
            {
                "ITEM_HOODIE_DESC"
            }
        );
        public static float durationIncreasePercent = durationIncrease.Value / 100f;
        public static float rechargeTimeReductionPercent = rechargeTimeReductionPerStack.Value / 100f;

        public static List<BuffDef> ignoredBuffDefs = new List<BuffDef>();

        internal static void Init()
        {
            GenerateItem();
            GenerateBuff();
            AddTokens();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            ContentAddition.AddBuffDef(hoodieBuffActive);
            ContentAddition.AddBuffDef(hoodieBuffCooldown);

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "HOODIE";
            itemDef.nameToken = "HOODIE_NAME";
            itemDef.pickupToken = "HOODIE_PICKUP";
            itemDef.descriptionToken = "HOODIE_DESCRIPTION";
            itemDef.loreToken = "HOODIE_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier2;
            });

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Hoodie.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Hoodie.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility
            };
        }

        private static void GenerateBuff()
        {
            hoodieBuffActive = ScriptableObject.CreateInstance<BuffDef>();

            hoodieBuffActive.name = "Hoodie Active";
            hoodieBuffActive.iconSprite = Assets.bundle.LoadAsset<Sprite>("HoodieActive.png");
            hoodieBuffActive.canStack = false;
            hoodieBuffActive.isHidden = false;
            hoodieBuffActive.isDebuff = false;
            hoodieBuffActive.isCooldown = false;

            hoodieBuffCooldown = ScriptableObject.CreateInstance<BuffDef>();

            hoodieBuffCooldown.name = "Hoodie Cooldown";
            hoodieBuffCooldown.iconSprite = Assets.bundle.LoadAsset<Sprite>("HoodieCooldown.png");
            hoodieBuffCooldown.canStack = false;
            hoodieBuffCooldown.isHidden = false;
            hoodieBuffCooldown.isDebuff = false;
            hoodieBuffCooldown.isCooldown = true;
        }

        private static float CalculateHoodieCooldown(int itemCount)
        {
            return rechargeTime.Value * Mathf.Pow(1 - rechargeTimeReductionPercent, itemCount - 1);
        }

        public static void Hooks()
        {
            RoR2Application.onLoad += () =>
            {
                // Buffs that this item shouldn't affect
                ignoredBuffDefs.Add(RoR2Content.Buffs.MedkitHeal);
                ignoredBuffDefs.Add(RoR2Content.Buffs.HiddenInvincibility);

                // Ignore dot effects that use Buffs to keep track of them
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                foreach (var dotDebuff in DotController.dotDefs.Where(x => x.associatedBuff != null).Select(x => x.associatedBuff).Distinct())
                    ignoredBuffDefs.Add(dotDebuff);
                foreach (var dotDebuff in DotController.dotDefs.Where(x => x.terminalTimedBuff != null).Select(x => x.terminalTimedBuff).Distinct())
                    ignoredBuffDefs.Add(dotDebuff);
#pragma warning restore Publicizer001 // Accessing a member that was not originally public
            };

            On.RoR2.CharacterBody.FixedUpdate += (orig, self) =>
            {
                orig(self);

                if (self && self.inventory)
                {
                    if (!self.HasBuff(hoodieBuffActive) && !self.HasBuff(hoodieBuffCooldown) && self.inventory.GetItemCount(itemDef) > 0)
                    {
                        self.AddBuff(hoodieBuffActive);
                    }
                }
            };

            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += (orig, self, buffDef, duration) =>
            {
                if (self && self.inventory)
                {
                    if (!buffDef.isDebuff && !buffDef.isCooldown && self.HasBuff(hoodieBuffActive) && !ignoredBuffDefs.Contains(buffDef))
                    {
                        int count = self.inventory.GetItemCount(itemDef);
                        duration *= 1 + durationIncreasePercent * count;

                        self.RemoveBuff(hoodieBuffActive);
                        self.AddTimedBuff(hoodieBuffCooldown, CalculateHoodieCooldown(count));
                    }
                }
                orig(self, buffDef, duration);
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("HOODIE", "Fleece Hoodie");
            LanguageAPI.Add("HOODIE_NAME", "Fleece Hoodie");
            LanguageAPI.Add("HOODIE_PICKUP", "The next timed buff received lasts longer. Recharges over time.");

            string desc = $"The next timed buff received has its duration increased by " +
                $"<style=cIsUtility>{durationIncrease.Value}%</style> <style=cStack>(+{durationIncrease.Value}% per stack)</style>. " +
                $"Recharges every <style=cIsUtility>{rechargeTime.Value}</style> <style=cStack>(-{rechargeTimeReductionPerStack.Value}% per stack)</style> seconds.";
            LanguageAPI.Add("HOODIE_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("HOODIE_LORE", lore);
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