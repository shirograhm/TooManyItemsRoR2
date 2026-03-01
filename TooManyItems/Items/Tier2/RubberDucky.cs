using R2API;
using RoR2;
using RoR2.Items;
using TooManyItems.Managers;
using UnityEngine;

namespace TooManyItems.Items.Tier2
{
    public class RubberDuckyItemBehaviour : BaseItemBodyBehavior
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        public static ItemDef GetItemDef()
        {
            return RubberDucky.itemDef;
        }

        public void FixedUpdate()
        {
            if (stack > 0) Utilities.ForceRecalculate(body);
        }
    }

    public class RubberDucky
    {
        public static ItemDef itemDef;
        public static BuffDef conversionBuff;

        // Gain armor. Upon activation of the teleporter, gain temporary BASE damage that scales with your armor.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Rubber Ducky",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            ["ITEM_RUBBERDUCKY_DESC"]
        );
        public static ConfigurableValue<int> armorPerStack = new(
            "Item: Rubber Ducky",
            "Armor",
            10,
            "Amount of flat armor gained per stack.",
            ["ITEM_RUBBERDUCKY_DESC"]
        );
        public static ConfigurableValue<int> armorPerExtraStack = new(
            "Item: Rubber Ducky",
            "Armor Extra Stacks",
            20,
            "Amount of flat armor gained per extra stack.",
            ["ITEM_RUBBERDUCKY_DESC"]
        );
        public static ConfigurableValue<float> armorConversion = new(
            "Item: Rubber Ducky",
            "Armor Conversion",
            20f,
            "Percent of armor granted as temporary BASE damage upon activation of the teleporter.",
            ["ITEM_RUBBERDUCKY_DESC"]
        );
        public static ConfigurableValue<float> conversionDuration = new(
            "Item: Rubber Ducky",
            "Conversion Duration",
            12f,
            "Duration of the damage buff from armor.",
            ["ITEM_RUBBERDUCKY_DESC"]
        );
        public static float percentArmorConversion = armorConversion.Value / 100f;

        internal static void Init()
        {
            itemDef = ItemManager.GenerateItem("RubberDucky", [ItemTag.Damage, ItemTag.Utility, ItemTag.CanBeTemporary], ItemTier.Tier2);

            conversionBuff = ItemManager.GenerateBuff("Conversion", AssetManager.bundle.LoadAsset<Sprite>("Conversion.png"));
            ContentAddition.AddBuffDef(conversionBuff);

            Hooks();
        }

        public static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender && sender.inventory)
                {
                    int count = sender.inventory.GetItemCountEffective(itemDef);
                    if (count > 0)
                    {
                        args.armorAdd += Utilities.GetLinearStacking(armorPerStack.Value, armorPerExtraStack.Value, count);

                        if (sender.HasBuff(conversionBuff))
                        {
                            args.baseDamageAdd += sender.armor * percentArmorConversion;
                        }
                    }
                }
            };

            TeleporterInteraction.onTeleporterBeginChargingGlobal += (teleporterInteraction) =>
            {
                foreach (CharacterBody body in CharacterBody.readOnlyInstancesList)
                {
                    if (body && body.inventory)
                    {
                        int count = body.inventory.GetItemCountEffective(itemDef);
                        if (count > 0)
                        {
                            body.AddTimedBuff(conversionBuff, conversionDuration.Value);
                        }
                    }
                }
            };
        }
    }
}
