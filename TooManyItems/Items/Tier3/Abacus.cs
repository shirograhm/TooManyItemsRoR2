using R2API;
using RoR2;
using TooManyItems.Managers;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems.Items.Tier3
{
    internal class Abacus
    {
        public static ItemDef itemDef;
        public static BuffDef countedBuff;

        // Killing an enemy grants crit chance until the next stage. Excess crit chance grants bonus crit damage.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Abacus",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_ABACUS_DESC"]
        );
        public static ConfigurableValue<float> critChancePerStack = new(
            "Item: Abacus",
            "Crit On Kill",
            1f,
            "Crit chance gained on kill per stack of item.",
            ["ITEM_ABACUS_DESC"]
        );

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("Abacus", [ItemTag.AIBlacklist, ItemTag.Damage, ItemTag.OnKillEffect, ItemTag.CanBeTemporary], ItemTier.Tier3);

            countedBuff = ItemManager.GenerateBuff("Counted", AssetManager.bundle.LoadAsset<Sprite>("Counted.png"), canStack: true);
            ContentAddition.AddBuffDef(countedBuff);

            Hooks();
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    // Process buffs regardless of item in inventory or not
                    int buffCount = sender.GetBuffCount(countedBuff);
                    args.critAdd += buffCount * critChancePerStack.Value;

                    // Give bonus crit damage if item is in inventory
                    if (sender.inventory.GetItemCountEffective(itemDef) > 0 && sender.crit > 100.0f)
                    {
                        args.critDamageMultAdd += sender.crit / 100f - 1f;
                    }
                }
            };

            GlobalEventManager.onCharacterDeathGlobal += (damageReport) =>
            {
                if (!NetworkServer.active) return;

                CharacterBody atkBody = damageReport.attackerBody;
                if (atkBody && atkBody.inventory)
                {
                    int count = atkBody.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++) atkBody.AddBuff(countedBuff);
                        Utilities.ForceRecalculate(atkBody);
                    }
                }
            };
        }
    }
}
