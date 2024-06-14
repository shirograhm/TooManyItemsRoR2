using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TooManyItems
{
    internal class Debris
    {
        public static ItemDef itemDef;

        internal static void Init()
        {
            GenerateItem();

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "DEBRIS";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.NoTier);

            itemDef.pickupIconSprite = Assets.bundle.LoadAsset<Sprite>("Debris.png");
            itemDef.canRemove = false;
            itemDef.hidden = false;
        }
    }
}
