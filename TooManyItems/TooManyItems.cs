using BepInEx;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace TooManyItems
{
    // Dependencies
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency("com.xoxfaby.BetterUI", BepInDependency.DependencyFlags.SoftDependency)]

    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class TooManyItems : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "shirograhm";
        public const string PluginName = "TooManyItems";
        public const string PluginVersion = "0.1.4";

        public static System.Random rand = new();

        public static ExpansionDef voidDLC;
        public static AssetBundle MainAssets;

        public void Awake()
        {
            Log.Init(Logger);

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TooManyItems.tmiassets"))
            {
                if (stream != null)
                {
                    MainAssets = AssetBundle.LoadFromStream(stream);

                    Log.Message("Successfully loaded assets.");

                }
                else
                {
                    Log.Error("Assets failed to load.");
                }
            }

            voidDLC = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset").WaitForCompletion();
            RoR2.ItemCatalog.availability.CallWhenAvailable(InjectVoidItems);

            GenericGameEvents.Init();
            ConfigOptions.Init();
            DamageColorAPI.Init();

            RoR2.ItemCatalog.availability.CallWhenAvailable(Integrations.Init);

            //Red Items
            BloodDice.Init();
            IronHeart.Init();
            RustyTrowel.Init();
            SoulRing.Init();

            // Green Items
            BrokenMask.Init();
            GlassMarble.Init();
            HereticSeal.Init();
            Hoodie.Init();

            // White Items
            BottleCap.Init();
            DebitCard.Init();
            EdibleGlue.Init();
            HolyWater.Init();
            //InstructionManual.Init();
            MilkCarton.Init();
            MagnifyingGlass.Init();
            Photodiode.Init();
            RedBlueGlasses.Init();
            RubberDucky.Init();

            // Lunar
            AncientCoin.Init();
            CarvingBlade.Init();
            Crucifix.Init();

            // Equipment
            BuffTotem.Init();
            TatteredScroll.Init();

            Log.Message("Finished initializations.");
        }

        private void InjectVoidItems()
        {
            On.RoR2.Items.ContagiousItemManager.Init += (orig) =>
            {
                List<ItemDef.Pair> newVoidPairs = new List<ItemDef.Pair>();

                // 3D Glasses => Instakill Glasses
                newVoidPairs.Add(new ItemDef.Pair()
                {
                    itemDef1 = RedBlueGlasses.itemDef,
                    itemDef2 = DLC1Content.Items.CritGlassesVoid
                });

                var key = DLC1Content.ItemRelationshipTypes.ContagiousItem;
                Debug.Log(key);
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                var voidPairs = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem];
                ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = voidPairs.Union(newVoidPairs).ToArray();
#pragma warning restore Publicizer001 // Accessing a member that was not originally public

                Debug.Log("Injected void item transformations.");

                orig();
            };
        }

        private void Update()
        {
            if (!NetworkServer.active) return;

            if (Input.GetKeyDown(KeyCode.F4))
            {
                DropItem(AncientCoin.itemDef);
                DropItem(BloodDice.itemDef);
                DropItem(BottleCap.itemDef);
                DropItem(BrokenMask.itemDef);
                DropItem(CarvingBlade.itemDef);
                DropItem(Crucifix.itemDef);
                DropItem(DebitCard.itemDef);
                DropItem(EdibleGlue.itemDef);
                DropItem(GlassMarble.itemDef);
                DropItem(HereticSeal.itemDef);
                DropItem(HolyWater.itemDef);
                DropItem(Hoodie.itemDef);
                //DropItem(InstructionManual.itemDef);
                DropItem(IronHeart.itemDef);
                DropItem(MilkCarton.itemDef);
                DropItem(MagnifyingGlass.itemDef);
                DropItem(Photodiode.itemDef);
                DropItem(RedBlueGlasses.itemDef);
                DropItem(RubberDucky.itemDef);
                DropItem(RustyTrowel.itemDef);
                DropItem(SoulRing.itemDef);
            }
            if (Input.GetKeyDown(KeyCode.F5))
            {
                // Single key testing
                //DropItem(TatteredScroll.equipmentDef);
                DropItem(BuffTotem.equipmentDef);
            }
        }

        private void DropItem(ItemDef def)
        {
            var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

            Log.Info($"Dropping {def.nameToken} at coordinates {transform.position}");
            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(def.itemIndex), transform.position, transform.forward * 20f);
        }

        private void DropItem(EquipmentDef def)
        {
            var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

            Log.Info($"Dropping {def.nameToken} at coordinates {transform.position}");
            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(def.equipmentIndex), transform.position, transform.forward * 20f);
        }

        private void DropItem(MiscPickupDef def)
        {
            var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

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
