using RoR2;
using TooManyItems.Managers;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems.Items.Tier1
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
            ["ITEM_EDIBLEGLUE_DESC"]
        );
        public static ConfigurableValue<float> slowRadiusInitialStack = new(
            "Item: Edible Glue",
            "Slow Radius Initial Stack",
            20f,
            "Slow radius amount for the first stack of item.",
            ["ITEM_EDIBLEGLUE_DESC"]
        );
        public static ConfigurableValue<float> slowRadiusPerExtraStack = new(
            "Item: Edible Glue",
            "Slow Radius Extra Stacks",
            10f,
            "Slow radius amount for each additional stack of item.",
            ["ITEM_EDIBLEGLUE_DESC"]
        );
        public static ConfigurableValue<float> slowDuration = new(
            "Item: Edible Glue",
            "Glue Duration",
            4f,
            "Slow duration.",
            ["ITEM_EDIBLEGLUE_DESC"]
        );

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("EdibleGlue", [ItemTag.Utility, ItemTag.OnKillEffect, ItemTag.CanBeTemporary], ItemTier.Tier1);

            Hooks();
        }

        public static void Hooks()
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
                            radius = Utilities.GetLinearStacking(slowRadiusInitialStack.Value, slowRadiusPerExtraStack.Value, itemCount)
                        }.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

                        foreach (HurtBox hurtbox in hurtboxes)
                        {
                            HealthComponent hc = hurtbox.healthComponent;
                            if (hc && hc.body && hc.body.teamComponent && hc.body.teamComponent.teamIndex != atkBody.teamComponent.teamIndex)
                            {
                                hc.body.AddTimedBuff(RoR2Content.Buffs.Slow60, slowDuration.Value);
                            }
                        }
                    }
                }
            };
        }
    }
}
