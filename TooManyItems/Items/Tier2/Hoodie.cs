using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class Hoodie
    {
        public static ItemDef itemDef;
        public static BuffDef hoodieBuffActive;
        public static BuffDef hoodieBuffCooldown;

        // The next timed buff received has its duration doubled. Recharges every 8 (-10% per stack) seconds.
        public static ConfigurableValue<float> rechargeTime = new(
            "Item: Suspicious Hoodie",
            "Recharge Time",
            8f,
            "Time this item takes to recharge.",
            new List<string>()
            {
                "ITEM_HOODIE_DESC"
            }
        );
        public static ConfigurableValue<float> rechargeTimeReductionPerStack = new(
            "Item: Suspicious Hoodie",
            "Recharge Time Reduction",
            10f,
            "Percent of recharge time removed for every additional stack of this item.",
            new List<string>()
            {
                "ITEM_HOODIE_DESC"
            }
        );
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

            itemDef.pickupIconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("Hoodie.png");
            itemDef.pickupModelPrefab = TooManyItems.MainAssets.LoadAsset<GameObject>("Hoodie.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        private static void GenerateBuff()
        {
            hoodieBuffActive = ScriptableObject.CreateInstance<BuffDef>();

            hoodieBuffActive.name = "Hoodie Active";
            hoodieBuffActive.iconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("HoodieBuffActive.png");
            hoodieBuffActive.canStack = false;
            hoodieBuffActive.isHidden = false;
            hoodieBuffActive.isDebuff = false;
            hoodieBuffActive.isCooldown = false;

            hoodieBuffCooldown = ScriptableObject.CreateInstance<BuffDef>();

            hoodieBuffCooldown.name = "Hoodie Cooldown";
            hoodieBuffCooldown.iconSprite = TooManyItems.MainAssets.LoadAsset<Sprite>("HoodieBuffCooldown.png");
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
            };

            On.RoR2.CharacterBody.OnBuffFinalStackLost += (orig, self, buffDef) =>
            {
                orig(self, buffDef);

                if (buffDef == hoodieBuffCooldown && self.inventory && self.inventory.GetItemCount(itemDef) > 0)
                {
                    self.AddBuff(hoodieBuffActive);
                }
            };

            On.RoR2.Inventory.GiveItem_ItemIndex_int += (orig, self, itemIndex, count) =>
            {
                if (NetworkServer.active)
                {
                    CharacterMaster master = self.GetComponent<CharacterMaster>();
                    if (master && itemIndex == itemDef.itemIndex)
                    {
                        CharacterBody body = master.GetBody();
                        if (body && !body.HasBuff(hoodieBuffActive) && !body.HasBuff(hoodieBuffCooldown))
                        {
                            body.AddBuff(hoodieBuffActive);
                        }
                    }
                }
                orig(self, itemIndex, count);
            };

            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += (orig, self, buffDef, duration) =>
            {
                if (self && self.inventory)
                {

                    if (!buffDef.isDebuff && !buffDef.isCooldown && self.HasBuff(hoodieBuffActive) && !ignoredBuffDefs.Contains(buffDef))
                    {
                        int count = self.inventory.GetItemCount(itemDef);
                        duration *= 2f;

                        self.RemoveBuff(hoodieBuffActive);
                        self.AddTimedBuff(hoodieBuffCooldown, CalculateHoodieCooldown(count));
                    }
                }
                orig(self, buffDef, duration);
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("HOODIE", "Suspicious Hoodie");
            LanguageAPI.Add("HOODIE_NAME", "Suspicious Hoodie");
            LanguageAPI.Add("HOODIE_PICKUP", "The next buff received is doubled. Recharges over time.");

            string desc = $"The next timed buff received has its duration doubled. " +
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