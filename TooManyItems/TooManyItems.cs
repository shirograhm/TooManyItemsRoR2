using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.ExpansionManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

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
        public const string PluginVersion = "0.4.0";

        public static PluginInfo PInfo { get; private set; }

        public static System.Random rand = new();

        public static ExpansionDef voidDLC;

        public void Awake()
        {
            PInfo = Info;
            voidDLC = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset").WaitForCompletion();

            Log.Init(Logger);
            Assets.Init();
            GenericGameEvents.Init();
            ConfigOptions.Init();
            DamageColorAPI.Init();

            ItemCatalog.availability.CallWhenAvailable(Integrations.Init);
            ItemCatalog.availability.CallWhenAvailable(InjectVoidItems);

            //Red Items
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
            if (RustyTrowel.isEnabled.Value)
                RustyTrowel.Init();

            // Green Items
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
            if (RedBlueGlasses.isEnabled.Value)
                RedBlueGlasses.Init();
            if (RubberDucky.isEnabled.Value)
                RubberDucky.Init();

            // Lunar
            if (AncientCoin.isEnabled.Value)
                AncientCoin.Init();
            if (CarvingBlade.isEnabled.Value)
                CarvingBlade.Init();
            if (Crucifix.isEnabled.Value)
                Crucifix.Init();
            if (SpiritStone.isEnabled.Value)
                SpiritStone.Init();

            // Void
            if (ShadowCrest.isEnabled.Value)
                ShadowCrest.Init();
            if (IronHeartVoid.isEnabled.Value)
                IronHeartVoid.Init();

            // Equipment
            if (Vanity.isEnabled.Value)
                Vanity.Init();
            if (BuffTotem.isEnabled.Value)
                BuffTotem.Init();
            if (Chalice.isEnabled.Value)
                Chalice.Init();
            if (TatteredScroll.isEnabled.Value)
                TatteredScroll.Init();

            Log.Message("Finished initializations.");
        }

        private void InjectVoidItems()
        {
            On.RoR2.Items.ContagiousItemManager.Init += (orig) =>
            {
                List<ItemDef.Pair> newVoidPairs = new List<ItemDef.Pair>
                {
                    // 3D Glasses => Instakill Glasses
                    new ItemDef.Pair()
                    {
                        itemDef1 = RedBlueGlasses.itemDef,
                        itemDef2 = DLC1Content.Items.CritGlassesVoid
                    },
                    // Iron Heart => Defiled Heart
                    new ItemDef.Pair()
                    {
                        itemDef1 = IronHeart.itemDef,
                        itemDef2 = IronHeartVoid.itemDef
                    },
                    // Seal of the Heretic => Shadow Crest
                    new ItemDef.Pair()
                    {
                        itemDef1 = HereticSeal.itemDef,
                        itemDef2 = ShadowCrest.itemDef
                    }
                };

                ItemRelationshipType key = DLC1Content.ItemRelationshipTypes.ContagiousItem;
                Debug.Log(key);

#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                ItemDef.Pair[] voidPairs = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem];
                ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = voidPairs.Union(newVoidPairs).ToArray();
#pragma warning restore Publicizer001 // Accessing a member that was not originally public

                Debug.Log("Injected void item transformations.");
                orig();
            };
        }

        private void Update()
        {
            if (!NetworkServer.active) return;

            if (Input.GetKeyDown(KeyCode.F2))
            {
                DropItem(Abacus.itemDef);
                DropItem(BloodDice.itemDef);
                DropItem(GlassMarbles.itemDef);
                DropItem(Horseshoe.itemDef);
                DropItem(IronHeart.itemDef);
                DropItem(Permafrost.itemDef);
                DropItem(RustyTrowel.itemDef);
                
                DropItem(BrokenMask.itemDef);
                DropItem(Epinephrine.itemDef);
                DropItem(HereticSeal.itemDef);
                DropItem(HolyWater.itemDef);
                DropItem(Hoodie.itemDef);
                DropItem(MagnifyingGlass.itemDef);
                DropItem(SoulRing.itemDef);
                
                DropItem(BottleCap.itemDef);
                DropItem(BreadLoaf.itemDef);
                DropItem(DebitCard.itemDef);
                DropItem(EdibleGlue.itemDef);
                DropItem(MilkCarton.itemDef);
                DropItem(PaperPlane.itemDef);
                DropItem(Photodiode.itemDef);
                DropItem(RedBlueGlasses.itemDef);
                DropItem(RubberDucky.itemDef);
                
                DropItem(AncientCoin.itemDef);
                DropItem(CarvingBlade.itemDef);
                DropItem(Crucifix.itemDef);
                DropItem(SpiritStone.itemDef);

                DropItem(IronHeartVoid.itemDef);
                DropItem(ShadowCrest.itemDef);

                DropItem(Vanity.equipmentDef);
                DropItem(BuffTotem.equipmentDef);
                DropItem(Chalice.equipmentDef);
                DropItem(TatteredScroll.equipmentDef);
            }
        }

        private void DropItem(ItemDef def)
        {
            foreach (PlayerCharacterMasterController controller in PlayerCharacterMasterController.instances)
            {
                Transform transform = controller.master.GetBodyObject().transform;

                Log.Info($"Dropping {def.nameToken} at coordinates {transform.position}");
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(def.itemIndex), transform.position, transform.forward * 20f);
            }
        }

        private void DropItem(EquipmentDef def)
        {
            Transform transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

            Log.Info($"Dropping {def.nameToken} at coordinates {transform.position}");
            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(def.equipmentIndex), transform.position, transform.forward * 20f);
        }

        private void DropItem(MiscPickupDef def)
        {
            Transform transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

            Log.Info($"Dropping {def.nameToken} at coordinates {transform.position}");
            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(def.miscPickupIndex), transform.position, transform.forward * 20f);
        }

        public struct GenericCharacterInfo
        {
            public GameObject gameObject;
            public CharacterBody body;
            public CharacterMaster master;
            public TeamComponent teamComponent;
            public HealthComponent healthComponent;
            public Inventory inventory;
            public TeamIndex teamIndex;
            public Vector3 aimOrigin;

            public GenericCharacterInfo(CharacterBody body)
            {
                this.body = body;
                gameObject = body ? body.gameObject : null;
                master = body ? body.master : null;
                teamComponent = body ? body.teamComponent : null;
                healthComponent = body ? body.healthComponent : null;
                inventory = master ? master.inventory : null;
                teamIndex = teamComponent ? teamComponent.teamIndex : TeamIndex.Neutral;
                aimOrigin = body ? body.aimOrigin : UnityEngine.Random.insideUnitSphere.normalized;
            }
        }
    }
}
