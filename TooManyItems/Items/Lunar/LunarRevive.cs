using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class LunarRevive
    {
        public static ItemDef itemDef;

        // Taking a fatal blow revives you at 20% (+20% per stack) HP. Lose 4 (+4 per stack) items each time you are revived. If you do not have 4 items to lose, die instead.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Sages Blessing",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_LUNARREVIVE_DESC"
            }
        );
        public static ConfigurableValue<float> reviveHealthPerStack = new(
            "Item: Sages Blessing",
            "Revive Health Per Stack",
            20f,
            "Percentage of health you spawn with when you revive.",
            new List<string>()
            {
                "ITEM_LUNARREVIVE_DESC"
            }
        );
        public static float reviveHealthPercent = reviveHealthPerStack.Value / 100f;

        public static ConfigurableValue<int> itemsLostPerStack = new(
            "Item: Sages Blessing",
            "Items Lost Per Stack",
            4,
            "Items lost when you revive.",
            new List<string>()
            {
                "ITEM_LUNARREVIVE_DESC"
            }
        );

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

            itemDef.name = "LUNAR_REVIVE";
            itemDef.nameToken = "LUNAR_REVIVE_NAME";
            itemDef.pickupToken = "LUNAR_REVIVE_PICKUP";
            itemDef.descriptionToken = "LUNAR_REVIVE_DESCRIPTION";
            itemDef.loreToken = "LUNAR_REVIVE_LORE";

            Utils.SetItemTier(itemDef, ItemTier.Lunar);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("LunarRevive.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("LunarRevive.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;
        }

        public static void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
            {
                orig(self, damageReport);
                if (!NetworkServer.active) return; 
                
                CharacterMaster master = damageReport.victimMaster;
                if (master && master.inventory)
                {
                    int itemCount = master.inventory.GetItemCount(itemDef);
                    if (itemCount > 0)
                    {
                        ItemIndex[] itemList = new List<ItemIndex>(master.inventory.itemAcquisitionOrder).ToArray();
                        // Make sure there are enough items to destroy
                        if (itemList.Length > itemsLostPerStack * itemCount)
                        {
                            master.Respawn(master.GetBody().footPosition, Quaternion.identity);
                            // Set health for revive
                            master.GetBody().healthComponent.health = master.GetBody().healthComponent.fullCombinedHealth * Utils.GetExponentialStacking(reviveHealthPercent, itemCount);
                            // Destroy items based on stack count
                            int itemsDestroyed = 0;
                            while (itemsDestroyed < itemsLostPerStack * itemCount)
                            {
                                ItemDef destroyDef = Utils.GetRandomItemDef();
                                bool isValidToDestroy = destroyDef.itemIndex != itemDef.itemIndex && destroyDef.tier != ItemTier.NoTier;
                                if (master.inventory.GetItemCount(destroyDef) > 0 && isValidToDestroy)
                                {
                                    master.inventory.RemoveItem(destroyDef);
                                    master.inventory.GiveItem(DLC1Content.Items.FragileDamageBonusConsumed);
                                    CharacterMasterNotificationQueue.SendTransformNotification(
                                        master,
                                        destroyDef.itemIndex,
                                        DLC1Content.Items.FragileDamageBonusConsumed.itemIndex,
                                        CharacterMasterNotificationQueue.TransformationType.Default
                                    );
                                    itemsDestroyed += 1;
                                }
                            }
                        }
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
