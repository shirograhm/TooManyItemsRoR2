using RoR2;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            itemDef = ItemManager.GenerateItem("Amnesia", [ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.Utility], ItemTier.Lunar);
            depletedDef = ItemManager.GenerateItem("DepletedAmnesia", [ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.CommandArtifactBlacklist, ItemTag.SacrificeBlacklist, ItemTag.DevotionBlacklist], ItemTier.NoTier);

            Hooks();
        }

        public static void Hooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += (damageReport) =>
            {
                if (!NetworkServer.active) return;

                if (damageReport.victimBody && damageReport.victimBody.master)
                {
                    CharacterMaster master = damageReport.victimBody.master;
                    Vector3 deathPosition = damageReport.damageInfo.position;

                    int count = master.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        Vector3 vector = deathPosition;
                        if (master.killedByUnsafeArea)
                            vector = TeleportHelper.FindSafeTeleportDestination(deathPosition, master.bodyPrefab.GetComponent<CharacterBody>(), RoR2Application.rng) ?? deathPosition;

                        // Revive the player with invulnerability
                        master.Respawn(vector, Quaternion.Euler(0f, TooManyItems.RandGen.Next(0, 360), 0f), wasRevivedMidStage: true);
                        if (master.GetBody()) master.GetBody().AddTimedBuff(RoR2Content.Buffs.Immune, 3f);

                        // Reset state machines
                        GameObject rezEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/HippoRezEffect");
                        if (master.bodyInstanceObject)
                        {
                            EntityStateMachine[] components = master.bodyInstanceObject.GetComponents<EntityStateMachine>();
                            foreach (EntityStateMachine obj in components)
                            {
                                obj.initialStateType = obj.mainStateType;
                            }
                            if (rezEffectPrefab)
                            {
                                EffectManager.SpawnEffect(rezEffectPrefab, new EffectData { origin = vector, rotation = master.bodyInstanceObject.transform.rotation }, transmit: true);
                            }
                        }

                        // Consume stack of Amnesia and Randomize items
                        master.inventory.RemoveItemPermanent(itemDef, 1);
                        master.inventory.GiveItemPermanent(depletedDef, 1);
                        CharacterMasterNotificationQueue.PushItemTransformNotification(
                            master,
                            itemDef.itemIndex,
                            depletedDef.itemIndex,
                            CharacterMasterNotificationQueue.TransformationType.Default
                        );
                        // Dont apply await (run synchronously to avoid issues with death state)
                        RandomizePlayerItems(master);
                    }
                }
            };
        }

        public static async Task RandomizePlayerItems(CharacterMaster master)
        {
            Dictionary<ItemTier, int> tierCounts = [];

            var itemStacks = master.inventory.permanentItemStacks;
            int durationPerItem = 3000 / 2 / itemStacks.GetTotalItemStacks();

            for (int i = 0; i < ItemCatalog.itemCount; i++)
            {
                ItemIndex itemIndex = (ItemIndex)i;
                ItemTier itemTier = ItemCatalog.GetItemDef(itemIndex).tier;

                if (!Utilities.IsItemTierRandomizable(itemTier)) continue;
                if (itemIndex == itemDef.itemIndex || Utilities.IsItemIndexScrap(itemIndex)) continue;

                int count = itemStacks.GetStackValue(itemIndex);
                if (count > 0)
                {
                    if (!tierCounts.ContainsKey(itemTier)) tierCounts[itemTier] = 0;
                    tierCounts[itemTier] += count;
                    master.inventory.RemoveItemPermanent(itemIndex, count);
                    await Task.Delay(Math.Max(Mathf.RoundToInt(durationPerItem), 1));
                }
            }
            foreach (ItemTier tier in tierCounts.Keys)
            {
                if (!Utilities.IsItemTierRandomizable(tier)) continue;

                for (int i = 0; i < tierCounts[tier]; i++)
                {
                    master.inventory.GiveItemPermanent(Utilities.GetRandomItemOfTier(tier));
                    await Task.Delay(Math.Max(Mathf.RoundToInt(durationPerItem), 1));
                }
            }
        }
    }
}
