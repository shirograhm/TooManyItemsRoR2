using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    internal class DebitCard
    {
        public static ItemDef itemDef;

        // Get a 8% (+8% per stack) rebate on purchases.
        public static ConfigurableValue<float> rebate = new(
            "Item: Debit Card",
            "Rebate Amount",
            8f,
            "Percentage of spent gold refunded as rebate.",
            new List<string>()
            {
                "ITEM_DEBITCARD_DESC"
            }
        );
        public static float rebatePercent = rebate.Value / 100f;

        internal static void Init()
        {
            GenerateItem();
            AddTokens();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "DEBIT_CARD";
            itemDef.nameToken = "DEBIT_CARD_NAME";
            itemDef.pickupToken = "DEBIT_CARD_PICKUP";
            itemDef.descriptionToken = "DEBIT_CARD_DESCRIPTION";
            itemDef.loreToken = "DEBIT_CARD_LORE";

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = ItemTier.Tier1;
            });

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("DebitCard.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("DebitCard.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility
            };
        }

        public static void Hooks()
        {
            On.RoR2.Items.MultiShopCardUtils.OnPurchase += (orig, context, moneyCost) =>
            {
                orig(context, moneyCost);

                CharacterMaster activator = context.activatorMaster;
                if (activator && activator.hasBody && activator.inventory)
                {
                    int count = activator.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        float refundScaling = 1 - (1 / (1 + (rebatePercent * count)));
                        activator.GiveMoney(Convert.ToUInt32(moneyCost * refundScaling));
                    }
                }
            };
        }

        private static void AddTokens()
        {
            LanguageAPI.Add("DEBIT_CARD", "Debit Card");
            LanguageAPI.Add("DEBIT_CARD_NAME", "Debit Card");
            LanguageAPI.Add("DEBIT_CARD_PICKUP", "Get a rebate on purchases.");

            string desc = $"Get a <style=cIsUtility>{rebate.Value}%</style> <style=cStack>(+{rebate.Value}% per stack) rebate</style> on purchases.";
            LanguageAPI.Add("DEBIT_CARD_DESCRIPTION", desc);

            string lore = "";
            LanguageAPI.Add("DEBIT_CARD_LORE", lore);
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