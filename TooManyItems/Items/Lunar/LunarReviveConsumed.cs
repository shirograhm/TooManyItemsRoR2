using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    internal class LunarReviveConsumed
    {
        public static ItemDef itemDef;

        // This item is given after a Lunar Revive. Lose 8% (+8% per stack) max health, exponentially. Double your stacks upon entering a new stage.
        public static ConfigurableValue<float> maxHealthLost = new(
            "Item: Sages Curse",
            "Health Lost",
            8f,
            "Percent max health lost per stack.",
            new List<string>()
            {
                "ITEM_LUNARREVIVECONSUMED_DESC"
            }
        );
        public static float maxHealthLostPercent = maxHealthLost.Value / 100f;

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

            itemDef.name = "LUNARREVIVECONSUMED";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.NoTier);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("LunarReviveConsumed.png");
            itemDef.canRemove = false;
            itemDef.hidden = false;
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int itemCount = sender.inventory.GetItemCount(itemDef);
                    if (itemCount > 0)
                    {
                        args.healthMultAdd -= Utils.GetExponentialStacking(maxHealthLostPercent, itemCount);
                    }
                }
            };

            On.RoR2.HealthComponent.GetHealthBarValues += (orig, self) =>
            {
                HealthComponent.HealthBarValues values = orig(self);
                if (self.body && self.body.inventory)
                {
                    int itemCount = self.body.inventory.GetItemCount(itemDef);
                    if (itemCount > 0)
                    {
                        values.curseFraction += (1f - values.curseFraction) * Utils.GetExponentialStacking(maxHealthLostPercent, itemCount);
                        values.healthFraction = self.health * (1f - values.curseFraction) / self.fullCombinedHealth;
                        values.shieldFraction = self.shield * (1f - values.curseFraction) / self.fullCombinedHealth;
                    }
                }
                return values;
            };

            Stage.onStageStartGlobal += (stage) =>
            {
                // Exit if stage is bazaar
                if (stage.sceneDef == SceneCatalog.GetSceneDefFromSceneName("bazaar"))
                    return;
               
                foreach (NetworkUser user in NetworkUser.readOnlyInstancesList)
                {
                    CharacterMaster master = user.masterController.master ?? user.master;
                    if (master && master.inventory)
                    {
                        int itemCount = master.inventory.GetItemCount(itemDef);
                        if (itemCount > 0)
                        {
                            master.inventory.GiveItem(itemDef, itemCount);
                        }
                    }
                }
            };
        }
    }
}
