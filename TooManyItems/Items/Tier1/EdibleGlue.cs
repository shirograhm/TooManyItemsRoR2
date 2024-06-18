﻿using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class EdibleGlue
    {
        public static ItemDef itemDef;

        // On kill, slow nearby enemies.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Edible Glue",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_EDIBLEGLUE_DESC"
            }
        );
        public static ConfigurableValue<float> slowRadiusPerStack = new(
            "Item: Edible Glue",
            "Slow Radius",
            8f,
            "Slow radius amount for each stack of item.",
            new List<string>()
            {
                "ITEM_EDIBLEGLUE_DESC"
            }
        );
        public static ConfigurableValue<float> slowDuration = new(
            "Item: Edible Glue",
            "Glue Duration",
            4f,
            "Slow duration.",
            new List<string>()
            {
                "ITEM_EDIBLEGLUE_DESC",
            }
        );

        internal static void Init()
        {
            GenerateItem();

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "EDIBLEGLUE";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier1);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("EdibleGlue.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("GlueBottle.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility,

                ItemTag.OnKillEffect
            };
        }

        public static void Hooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += (damageReport) =>
            {
                if (!NetworkServer.active) return;

                CharacterBody atkBody = damageReport.attackerBody;
                if (atkBody && atkBody.inventory)
                {
                    int count = atkBody.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        HurtBox[] hurtboxes = new SphereSearch
                        {
                            mask = LayerIndex.entityPrecise.mask,
                            origin = atkBody.corePosition,
                            queryTriggerInteraction = QueryTriggerInteraction.Collide,
                            radius = slowRadiusPerStack.Value * count
                        }.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

                        foreach (HurtBox hurtbox in hurtboxes)
                        {
                            HealthComponent hc = hurtbox.healthComponent;
                            if (hc && hc.body && hc.body.teamComponent && hc.body.teamComponent.teamIndex != atkBody.teamComponent.teamIndex)
                            {
                                hc.body.AddTimedBuff(RoR2Content.Buffs.Slow80, slowDuration.Value);
                            }
                        }
                    }
                }
            };
        }
    }
}
