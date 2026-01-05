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
        public static ItemDef forgottenDef;

        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Amnesia",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_AMNESIA_DESC"]
        );
        public static ConfigurableValue<float> invulnerabilityDuration = new(
            "Item: Amnesia",
            "Invulnerability Duration",
            3f,
            "Duration of invulnerability after revival in seconds.",
            ["ITEM_AMNESIA_DESC"]
        );
        public static ConfigurableValue<bool> canRerollTier1 = new(
            "Item: Amnesia",
            "Rerolls White Items",
            true,
            "Whether or not this item rerolls white items.",
            ["ITEM_AMNESIA_DESC"]
        );
        public static ConfigurableValue<bool> canRerollTier2 = new(
            "Item: Amnesia",
            "Rerolls Green Items",
            true,
            "Whether or not this item rerolls green items.",
            ["ITEM_AMNESIA_DESC"]
        );
        public static ConfigurableValue<bool> canRerollTier3 = new(
            "Item: Amnesia",
            "Rerolls Red Items",
            true,
            "Whether or not this item rerolls red items.",
            ["ITEM_AMNESIA_DESC"]
        );
        public static ConfigurableValue<bool> canRerollBoss = new(
            "Item: Amnesia",
            "Rerolls Boss Items",
            true,
            "Whether or not this item rerolls boss items.",
            ["ITEM_AMNESIA_DESC"]
        );
        public static ConfigurableValue<bool> canRerollVoidTier1 = new(
            "Item: Amnesia",
            "Rerolls Voided White Items",
            true,
            "Whether or not this item rerolls voided white items.",
            ["ITEM_AMNESIA_DESC"]
        );
        public static ConfigurableValue<bool> canRerollVoidTier2 = new(
            "Item: Amnesia",
            "Rerolls Voided Green Items",
            true,
            "Whether or not this item rerolls voided green items.",
            ["ITEM_AMNESIA_DESC"]
        );
        public static ConfigurableValue<bool> canRerollVoidTier3 = new(
            "Item: Amnesia",
            "Rerolls Voided Red Items",
            true,
            "Whether or not this item rerolls voided red items.",
            ["ITEM_AMNESIA_DESC"]
        );
        public static ConfigurableValue<bool> canRerollLunar = new(
            "Item: Amnesia",
            "Rerolls Lunar Items",
            true,
            "Whether or not this item rerolls lunar items.",
            ["ITEM_AMNESIA_DESC"]
        );
        public static ConfigurableValue<float> forgottenRerollChance = new(
            "Item: Amnesia",
            "Forget Item Chance",
            20f,
            "Percent chance the rerolled stack is forgotten instead.",
            ["ITEM_AMNESIA_DESC"]
        );
        public static float percentForgottenRerollChance = forgottenRerollChance.Value / 100f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("Amnesia", [ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.Utility], ItemTier.Lunar);
            depletedDef = ItemManager.GenerateItem("DepletedAmnesia", [ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.CommandArtifactBlacklist, ItemTag.SacrificeBlacklist, ItemTag.DevotionBlacklist], ItemTier.NoTier);
            forgottenDef = ItemManager.GenerateItem("ForgottenItem", [ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.CommandArtifactBlacklist, ItemTag.SacrificeBlacklist, ItemTag.DevotionBlacklist], ItemTier.NoTier);
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

                    int count = master.inventory.GetItemCountPermanent(itemDef);
                    if (count > 0)
                    {
                        Vector3 vector = deathPosition;
                        if (master.killedByUnsafeArea)
                            vector = TeleportHelper.FindSafeTeleportDestination(deathPosition, master.bodyPrefab.GetComponent<CharacterBody>(), RoR2Application.rng) ?? deathPosition;

                        // Revive the player with invulnerability
                        master.Respawn(vector, Quaternion.Euler(0f, TooManyItems.RandGen.Next(0, 360), 0f), wasRevivedMidStage: true);
                        if (master.GetBody()) master.GetBody().AddTimedBuff(RoR2Content.Buffs.Immune, invulnerabilityDuration.Value);

                        // Reset state machines
                        GameObject rezEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/HippoRezEffect");
                        if (master.bodyInstanceObject)
                        {
                            EntityStateMachine[] components = master.bodyInstanceObject.GetComponents<EntityStateMachine>();
                            foreach (EntityStateMachine obj in components)
                            {
                                obj.initialStateType = obj.mainStateType;
                            }
                            if (rezEffectPrefab) EffectManager.SpawnEffect(rezEffectPrefab, new EffectData { origin = vector, rotation = master.bodyInstanceObject.transform.rotation }, transmit: true);
                        }

                        // Send chat message
                        Chat.SubjectFormatChatMessage subjectFormatChatMessage = new()
                        {
                            subjectAsCharacterBody = master.GetBody(),
                            baseToken = "AMNESIA_REVIVE_MESSAGE",
                            paramTokens = [master.playerControllerId.ToString()]
                        };
                        Chat.SendBroadcastChat(subjectFormatChatMessage);

                        // Consume stack of Amnesia and Randomize items
                        master.inventory.RemoveItemPermanent(itemDef, 1);
                        master.inventory.GiveItemPermanent(depletedDef, 1);
                        CharacterMasterNotificationQueue.PushItemTransformNotification(
                            master,
                            itemDef.itemIndex,
                            depletedDef.itemIndex,
                            CharacterMasterNotificationQueue.TransformationType.Default
                        );
                        // Dont apply await (run asynchronously to avoid issues with death state)
                        RandomizePlayerItems(master);
                    }
                }
            };
        }

        public static async Task RandomizePlayerItems(CharacterMaster master)
        {
            List<(ItemTier, int)> stackCounts = [];

            ItemCollection itemStacks = master.inventory.permanentItemStacks;
            int immuneDurationMillis = Mathf.RoundToInt(invulnerabilityDuration.Value * 1000f);
            int durationPerItem = immuneDurationMillis / itemStacks.GetTotalItemStacks();

            for (int i = 0; i < ItemCatalog.itemCount; i++)
            {
                ItemIndex itemIndex = (ItemIndex)i;
                ItemTier itemTier = ItemCatalog.GetItemDef(itemIndex).tier;

                // check for configs
                if (!IsItemTierRandomizable(itemTier)) continue;
                // Don't reroll scrap or the Amnesia item itself
                if (itemIndex == itemDef.itemIndex || Utilities.IsItemIndexScrap(itemIndex)) continue;

                int count = itemStacks.GetStackValue(itemIndex);
                if (count > 0)
                {
                    master.inventory.RemoveItemPermanent(itemIndex, count);
                    await Task.Delay(Math.Max(Mathf.RoundToInt(durationPerItem / 2), 1));
                    // Store the stack info for later
                    stackCounts.Add((itemTier, count));
                }
            }
            foreach ((ItemTier, int) entry in stackCounts)
            {
                ItemIndex newItemIndex = Utilities.GetRandomItemOfTier(entry.Item1);

                int forgottenCount = 0;
                for (int i = 0; i < entry.Item2; i++)
                {
                    if (Util.CheckRoll(forgottenRerollChance.Value))
                    {
                        forgottenCount++;
                        CharacterMasterNotificationQueue.PushItemTransformNotification(
                            master,
                            newItemIndex,
                            forgottenDef.itemIndex,
                            CharacterMasterNotificationQueue.TransformationType.Default
                        );
                        master.inventory.GiveItemPermanent(forgottenDef.itemIndex, 1);
                        await Task.Delay(Math.Max(Mathf.RoundToInt(durationPerItem / 2), 1));
                    }
                }
                master.inventory.GiveItemPermanent(newItemIndex, entry.Item2 - forgottenCount);
                await Task.Delay(Math.Max(Mathf.RoundToInt(durationPerItem / 2), 1));
            }
        }

        public static bool IsItemTierRandomizable(ItemTier tier)
        {
            return tier switch
            {
                ItemTier.Tier1 => canRerollTier1.Value,
                ItemTier.Tier2 => canRerollTier2.Value,
                ItemTier.Tier3 => canRerollTier3.Value,
                ItemTier.Lunar => canRerollLunar.Value,
                ItemTier.VoidTier1 => canRerollVoidTier1.Value,
                ItemTier.VoidTier2 => canRerollVoidTier2.Value,
                ItemTier.VoidTier3 => canRerollVoidTier3.Value,
                ItemTier.Boss => canRerollBoss.Value,
                _ => false,
            };
        }
    }
}
