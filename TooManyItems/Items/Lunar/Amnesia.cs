using RoR2;
using System;
using TooManyItems.Managers;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems.Items.Lunar
{
    internal class Amnesia
    {
        public static ItemDef itemDef;
        public static ItemDef depletedDef;

        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Amnesia",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_LUNARREVIVE_DESC"]
        );

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("Amnesia", [ItemTag.AIBlacklist, ItemTag.Utility], ItemTier.Lunar);
            depletedDef = ItemManager.GenerateItem("DepletedAmnesia", [ItemTag.AIBlacklist, ItemTag.CommandArtifactBlacklist, ItemTag.SacrificeBlacklist, ItemTag.DevotionBlacklist], ItemTier.NoTier);

            Hooks();
        }

        public static void Hooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += (damageReport) =>
            {
                if (!NetworkServer.active) return;

                CharacterBody vicBody = damageReport.victimBody;
                if (vicBody && vicBody.inventory && vicBody.master)
                {
                    int count = vicBody.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        Vector3 vector = vicBody.footPosition;
                        if (vicBody.master.killedByUnsafeArea)
                            vector = TeleportHelper.FindSafeTeleportDestination(vicBody.footPosition, vicBody, RoR2Application.rng) ?? vicBody.footPosition;

                        // Consume stack of Amnesia
                        vicBody.master.inventory.RemoveItemPermanent(itemDef, 1);
                        vicBody.master.inventory.GiveItemPermanent(depletedDef, 1);
                        CharacterMasterNotificationQueue.PushItemTransformNotification(
                            vicBody.master,
                            itemDef.itemIndex,
                            depletedDef.itemIndex,
                            CharacterMasterNotificationQueue.TransformationType.Default
                        );
                        RandomizePlayerItems(vicBody.master);

                        vicBody.master.Respawn(vector, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), wasRevivedMidStage: true);
                        vicBody.AddTimedBuff(RoR2Content.Buffs.Immune, 3f);
                        GameObject healReviveEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/fxHealAndReviveGold");
                        if (vicBody.gameObject)
                        {
                            EntityStateMachine[] components = vicBody.gameObject.GetComponents<EntityStateMachine>();
                            foreach (EntityStateMachine obj in components)
                            {
                                obj.initialStateType = obj.mainStateType;
                            }
                            if (healReviveEffectPrefab)
                            {
                                EffectManager.SpawnEffect(healReviveEffectPrefab, new EffectData { origin = vector, rotation = vicBody.gameObject.transform.rotation }, transmit: true);
                                Util.PlaySound("Play_item_use_healAndRevive_activate", vicBody.gameObject);
                            }
                        }
                    }
                }
            };
        }

        public static void RandomizePlayerItems(CharacterMaster master)
        {
            int[] tierCounts = new int[Enum.GetValues(typeof(ItemTier)).Length];

            var itemStacks = master.inventory.permanentItemStacks;
            for (int i = 0; i < ItemCatalog.itemCount; i++)
            {
                ItemIndex itemIndex = (ItemIndex)i;

                if (!Utilities.IsItemTierRandomizable(ItemCatalog.GetItemDef(itemIndex).tier)) continue;
                if (itemIndex == itemDef.itemIndex || IsItemIndexScrap(itemIndex)) continue;

                int count = itemStacks.GetStackValue(itemIndex);
                if (count > 0)
                {
                    tierCounts[(int)ItemCatalog.GetItemDef(itemIndex).tier] += count;
                    master.inventory.RemoveItemPermanent(itemIndex, count);
                }
            }
            for (int i = 0; i < tierCounts.Length; i++)
            {
                if (!Utilities.IsItemTierRandomizable((ItemTier)i)) continue;

                for (int j = 0; j < tierCounts[i]; j++)
                    master.inventory.GiveItemPermanent(Utilities.GetRandomItemOfTier((ItemTier)i), 1);
            }
        }

        public static bool IsItemIndexScrap(ItemIndex itemIndex)
        {
            return itemIndex == RoR2Content.Items.ScrapWhite.itemIndex || itemIndex == RoR2Content.Items.ScrapGreen.itemIndex || itemIndex == RoR2Content.Items.ScrapRed.itemIndex;
        }
    }
}
