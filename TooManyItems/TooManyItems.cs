using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.ExpansionManagement;
using System.Collections.Generic;
using System.Linq;
using TooManyItems.Extensions;
using TooManyItems.Items.Equip;
using TooManyItems.Items.Equip.Lunar;
using TooManyItems.Items.Lunar;
using TooManyItems.Items.Tier1;
using TooManyItems.Items.Tier2;
using TooManyItems.Items.Tier3;
using TooManyItems.Items.Void;
using TooManyItems.Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TooManyItems
{
    // Dependencies
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    // Soft Dependencies
    [BepInDependency(LookingGlass.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    // Compatibility
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class TooManyItems : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "shirograhm";
        public const string PluginName = "TooManyItems";
        public const string PluginVersion = "0.6.14";

        public static PluginInfo PInfo { get; private set; }

        public static System.Random RandGen = new();

        public static ExpansionDef sotvDLC;
        public static ExpansionDef sotsDLC;

        public void Awake()
        {
            PInfo = Info;

            sotvDLC = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset").WaitForCompletion();
            sotsDLC = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC2/Common/DLC2.asset").WaitForCompletion();

            Log.Init(Logger);
            AssetManager.Init();
            GameEventManager.Init();
            ConfigOptions.Init();
            DamageColorManager.Init();
            Utilities.Init();

            // Red Items
            if (Abacus.isEnabled.Value)
                Abacus.Init();
            if (BloodDice.isEnabled.Value)
                BloodDice.Init();
            if (GlassMarbles.isEnabled.Value)
                GlassMarbles.Init();
            if (Horseshoe.isEnabled.Value)
                Horseshoe.Init();
            if (IronHeart.isEnabled.Value)
                IronHeart.Init();
            if (Permafrost.isEnabled.Value)
                Permafrost.Init();
            if (RustedTrowel.isEnabled.Value)
                RustedTrowel.Init();

            // Green Items
            if (BrassKnuckles.isEnabled.Value)
                BrassKnuckles.Init();
            if (BrokenMask.isEnabled.Value)
                BrokenMask.Init();
            if (Epinephrine.isEnabled.Value)
                Epinephrine.Init();
            if (HereticSeal.isEnabled.Value)
                HereticSeal.Init();
            if (HolyWater.isEnabled.Value)
                HolyWater.Init();
            if (Hoodie.isEnabled.Value)
                Hoodie.Init();
            if (MagnifyingGlass.isEnabled.Value)
                MagnifyingGlass.Init();
            if (SoulRing.isEnabled.Value)
                SoulRing.Init();

            // White Items
            if (BottleCap.isEnabled.Value)
                BottleCap.Init();
            if (BreadLoaf.isEnabled.Value)
                BreadLoaf.Init();
            if (DebitCard.isEnabled.Value)
                DebitCard.Init();
            if (EdibleGlue.isEnabled.Value)
                EdibleGlue.Init();
            if (MilkCarton.isEnabled.Value)
                MilkCarton.Init();
            if (PaperPlane.isEnabled.Value)
                PaperPlane.Init();
            if (Photodiode.isEnabled.Value)
                Photodiode.Init();
            if (PropellerHat.isEnabled.Value)
                PropellerHat.Init();
            if (RedBlueGlasses.isEnabled.Value)
                RedBlueGlasses.Init();
            if (RubberDucky.isEnabled.Value)
                RubberDucky.Init();
            if (Thumbtack.isEnabled)
                Thumbtack.Init();

            // Lunar
            if (AncientCoin.isEnabled.Value)
                AncientCoin.Init();
            if (Amnesia.isEnabled.Value)
                Amnesia.Init();
            if (CarvingBlade.isEnabled.Value)
                CarvingBlade.Init();
            if (Crucifix.isEnabled.Value)
                Crucifix.Init();
            if (SpiritStone.isEnabled.Value)
                SpiritStone.Init();
            if (DoubleDown.isEnabled.Value)
                DoubleDown.Init();

            // Void
            if (ShadowCrest.isEnabled.Value)
                ShadowCrest.Init();
            if (VoidHeart.isEnabled.Value)
                VoidHeart.Init();

            // Equipment
            if (BuffTotem.isEnabled.Value)
                BuffTotem.Init();
            if (TatteredScroll.isEnabled.Value)
                TatteredScroll.Init();
            if (Chalice.isEnabled.Value)
                Chalice.Init();
            if (Vanity.isEnabled.Value)
                Vanity.Init();

            ItemCatalog.availability.CallWhenAvailable(Integrations.Init);
            ItemCatalog.availability.CallWhenAvailable(InjectVoidItemTramsforms);

            Log.Message("Finished initializations.");
        }

        private void InjectVoidItemTramsforms()
        {
            On.RoR2.Items.ContagiousItemManager.Init += (orig) =>
            {
                List<ItemDef.Pair> newVoidPairs = [];

                // 3D Glasses => Instakill Glasses
                if (RedBlueGlasses.isEnabled)
                    newVoidPairs.Add(new ItemDef.Pair() { itemDef1 = RedBlueGlasses.itemDef, itemDef2 = DLC1Content.Items.CritGlassesVoid });
                // Thumbtack => Needletick
                if (Thumbtack.isEnabled)
                    newVoidPairs.Add(new ItemDef.Pair() { itemDef1 = Thumbtack.itemDef, itemDef2 = DLC1Content.Items.BleedOnHitVoid });
                // Iron Heart => Defiled Heart
                if (IronHeart.isEnabled && VoidHeart.isEnabled)
                    newVoidPairs.Add(new ItemDef.Pair() { itemDef1 = IronHeart.itemDef, itemDef2 = VoidHeart.itemDef });
                // Seal of the Heretic => Shadow Crest
                if (HereticSeal.isEnabled && ShadowCrest.isEnabled)
                    newVoidPairs.Add(new ItemDef.Pair() { itemDef1 = HereticSeal.itemDef, itemDef2 = ShadowCrest.itemDef });

                ItemRelationshipType key = DLC1Content.ItemRelationshipTypes.ContagiousItem;
                Debug.Log(key);

                ItemDef.Pair[] voidPairs = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem];
                ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = [.. voidPairs.Union(newVoidPairs)];

                Debug.Log("Injected void item transformations.");
                orig();
            };
        }
    }
}
