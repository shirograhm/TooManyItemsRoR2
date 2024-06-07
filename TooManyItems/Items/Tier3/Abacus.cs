using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class Abacus
    {
        public static ItemDef itemDef;
        public static BuffDef countedBuff;

        // Killing an enemy grants 1% (+1% per stack) crit chance until the next stage. Excess crit chance grants bonus crit damage.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Abacus",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_ABACUS_DESC"
            }
        );
        public static ConfigurableValue<float> critChancePerStack = new(
            "Item: Abacus",
            "Crit On Kill",
            1f,
            "Crit chance gained on kill per stack of item.",
            new List<string>()
            {
                "ITEM_ABACUS_DESC"
            }
        );

        internal static void Init()
        {
            GenerateItem();
            GenerateBuff();

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            ContentAddition.AddBuffDef(countedBuff);

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "ABACUS";
            itemDef.AutoPopulateTokens();
            
            Utils.SetItemTier(itemDef, ItemTier.Tier3);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Abacus.png");
            itemDef.pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("Abacus.prefab");
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage,

                ItemTag.OnKillEffect
            };
        }

        private static void GenerateBuff()
        {
            countedBuff = ScriptableObject.CreateInstance<BuffDef>();

            countedBuff.name = "Counted";
            countedBuff.iconSprite = Assets.bundle.LoadAsset<Sprite>("Counted.png");
            countedBuff.canStack = true;
            countedBuff.isHidden = false;
            countedBuff.isDebuff = false;
            countedBuff.isCooldown = false;
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
                    
                    if (sender.inventory.GetItemCount(itemDef) > 0 && sender.crit > 100.0f)
                    {
                        args.critDamageMultAdd += sender.crit / 100f - 1f;
                    }
                }
            };

            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, eventManager, damageReport) =>
            {
                orig(eventManager, damageReport);

                if (!NetworkServer.active) return;

                CharacterBody atkBody = damageReport.attackerBody;
                if (atkBody && atkBody.inventory)
                {
                    int count = atkBody.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++) atkBody.AddBuff(countedBuff);
                        Utils.ForceRecalculate(atkBody);
                    }
                }
            };
        }
    }
}
