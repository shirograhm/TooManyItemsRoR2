using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class VoidHeart
    {
        public static ItemDef itemDef;

        // Gain 150 HP. Gain 2% (+2% per stack) of your max health as bonus damage.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Defiled Heart",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_VOIDHEART_DESC"
            }
        );
        public static ConfigurableValue<float> healthIncrease = new(
            "Item: Defiled Heart",
            "Health Increase",
            300f,
            "Bonus health gained from this item. Does not increase with stacks.",
            new List<string>()
            {
                "ITEM_VOIDHEART_DESC"
            }
        );
        public static ConfigurableValue<float> percentDamagePerStack = new(
            "Item: Defiled Heart",
            "Bonus Damage Scaling",
            1f,
            "Percent of maximum health gained as bonus damage.",
            new List<string>()
            {
                "ITEM_VOIDHEART_DESC"
            }
        );
        public static float multiplierPerStack = percentDamagePerStack.Value / 100.0f;

        internal static void Init()
        {
            GenerateItem();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "VOIDHEART";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.VoidTier3);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("VoidHeart.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("VoidHeart.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage,
                ItemTag.Utility
            };
        }

        public static float CalculateDamageBonus(CharacterBody sender, float itemCount)
        {
            return sender.healthComponent.fullCombinedHealth * itemCount * multiplierPerStack;
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        args.baseHealthAdd += healthIncrease.Value;
                        args.baseDamageAdd += CalculateDamageBonus(sender, count);
                    }
                }
            };
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