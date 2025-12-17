using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class EdibleGlue : BaseItem
    {
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
        public static ConfigurableValue<float> slowRadiusInitialStack = new(
            "Item: Edible Glue",
            "Slow Radius Initial Stack",
            20f,
            "Slow radius amount for the first stack of item.",
            new List<string>()
            {
                "ITEM_EDIBLEGLUE_DESC"
            }
        );
        public static ConfigurableValue<float> slowRadiusPerExtraStack = new(
            "Item: Edible Glue",
            "Slow Radius Extra Stacks",
            10f,
            "Slow radius amount for each additional stack of item.",
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
            GenerateItem(
                "EDIBLEGLUE",
                "GlueBottle.prefab",
                "EdibleGlue.png",
                ItemTier.Tier1,
                [
                    ItemTag.Utility,
                    ItemTag.OnKillEffect,
                    ItemTag.CanBeTemporary
                ]
            );

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            Hooks();
        }

        public static new void Hooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += (damageReport) =>
            {
                if (!NetworkServer.active) return;

                CharacterBody atkBody = damageReport.attackerBody;
                if (atkBody && atkBody.inventory)
                {
                    int itemCount = atkBody.inventory.GetItemCountEffective(itemDef);
                    if (itemCount > 0)
                    {
                        HurtBox[] hurtboxes = new SphereSearch
                        {
                            mask = LayerIndex.entityPrecise.mask,
                            origin = atkBody.corePosition,
                            queryTriggerInteraction = QueryTriggerInteraction.Collide,
                            radius = Utils.GetLinearStacking(slowRadiusInitialStack.Value, slowRadiusPerExtraStack.Value, itemCount)
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
