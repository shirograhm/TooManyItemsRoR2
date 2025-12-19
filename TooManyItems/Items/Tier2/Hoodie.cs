using R2API;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TooManyItems
{
    internal class Hoodie
    {
        public static ItemDef itemDef;
        public static BuffDef hoodieBuffActive;
        public static BuffDef hoodieBuffCooldown;

        // The next timed buff received has its duration increased. Recharges over time.
        public static ConfigurableValue<bool> isEnabled = new(
            "Item: Fleece Hoodie",
            "Enabled",
            true,
            "Whether or not the item is enabled.",
            new List<string>()
            {
                "ITEM_HOODIE_DESC"
            }
        );
        public static ConfigurableValue<float> durationIncrease = new(
            "Item: Fleece Hoodie",
            "Duration Increase",
            40f,
            "Buff duration percentage multiplier per stack.",
            new List<string>()
            {
                "ITEM_HOODIE_DESC"
            }
        );
        public static ConfigurableValue<float> rechargeTime = new(
            "Item: Fleece Hoodie",
            "Recharge Time",
            10f,
            "Time this item takes to recharge.",
            new List<string>()
            {
                "ITEM_HOODIE_DESC"
            }
        );
        public static ConfigurableValue<float> rechargeTimeReductionPerStack = new(
            "Item: Fleece Hoodie",
            "Recharge Time Reduction",
            15f,
            "Percent of recharge time removed for every additional stack of this item.",
            new List<string>()
            {
                "ITEM_HOODIE_DESC"
            }
        );
        public static ConfigurableValue<string> customIgnoredBuffNames = new(
            "Item: Fleece Hoodie",
            "Custom Ignored Buff Names",
            "",
            "Buffs/debuffs that this item should not affect. Use the internal name of the buff for this to work correctly (e.g. bdMedkitHeal), and separate them all with commas.",
            new List<string>()
            {
                "ITEM_HOODIE_DESC"
            }
        );
        public static float durationIncreasePercent = durationIncrease.Value / 100f;
        public static float rechargeTimeReductionPercent = rechargeTimeReductionPerStack.Value / 100f;

        public static List<BuffDef> ignoredBuffDefs = new List<BuffDef>();

        internal static void Init()
        {
            GenerateItem();
            GenerateBuff();

            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            ContentAddition.AddBuffDef(hoodieBuffActive);
            ContentAddition.AddBuffDef(hoodieBuffCooldown);

            Hooks();
        }

        private static void GenerateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = "HOODIE";
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, ItemTier.Tier2);

            GameObject prefab = AssetHandler.bundle.LoadAsset<GameObject>("Hoodie.prefab");
            ModelPanelParameters modelPanelParameters = prefab.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = prefab.transform;
            modelPanelParameters.cameraPositionTransform = prefab.transform;
            modelPanelParameters.maxDistance = 10f;
            modelPanelParameters.minDistance = 5f;

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>("Hoodie.png");
            itemDef.pickupModelPrefab = prefab;
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility,

                ItemTag.CanBeTemporary
            };
        }

        private static void GenerateBuff()
        {
            hoodieBuffActive = ScriptableObject.CreateInstance<BuffDef>();

            hoodieBuffActive.name = "Hoodie Active";
            hoodieBuffActive.iconSprite = AssetHandler.bundle.LoadAsset<Sprite>("HoodieActive.png");
            hoodieBuffActive.canStack = false;
            hoodieBuffActive.isHidden = false;
            hoodieBuffActive.isDebuff = false;
            hoodieBuffActive.isCooldown = false;

            hoodieBuffCooldown = ScriptableObject.CreateInstance<BuffDef>();

            hoodieBuffCooldown.name = "Hoodie Cooldown";
            hoodieBuffCooldown.iconSprite = AssetHandler.bundle.LoadAsset<Sprite>("HoodieCooldown.png");
            hoodieBuffCooldown.canStack = false;
            hoodieBuffCooldown.isHidden = false;
            hoodieBuffCooldown.isDebuff = false;
            hoodieBuffCooldown.isCooldown = true;
        }

        public static float CalculateHoodieCooldown(int itemCount)
        {
            return rechargeTime.Value * Mathf.Pow(1 - rechargeTimeReductionPercent, itemCount - 1);
        }

        public static void Hooks()
        {
            RoR2Application.onLoad += () =>
            {
                // Buffs from the base game + DLCs that this item shouldn't affect
                ignoredBuffDefs.Add(RoR2Content.Buffs.MedkitHeal);
                ignoredBuffDefs.Add(RoR2Content.Buffs.HiddenInvincibility);
                ignoredBuffDefs.Add(RoR2Content.Buffs.VoidFogMild);
                ignoredBuffDefs.Add(RoR2Content.Buffs.VoidFogStrong);
                ignoredBuffDefs.Add(DLC1Content.Buffs.VoidRaidCrabWardWipeFog);

                // Append custom ignored buffs/debuffs from config
                foreach (var ignoredBuffName in customIgnoredBuffNames.Value.Split(','))
                {
                    var buffIndex = BuffCatalog.FindBuffIndex(ignoredBuffName.Trim());
                    if (buffIndex != BuffIndex.None)
                    {
                        ignoredBuffDefs.Add(BuffCatalog.GetBuffDef(buffIndex));
                        Log.Message("Successfully added " + ignoredBuffName.Trim() + " to Fleece Hoodie ignored buffs list.");
                    }
                }

                // Ignore dot effects that use Buffs to keep track of them
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                foreach (BuffDef dotDebuff in DotController.dotDefs.Where(x => x.associatedBuff != null).Select(x => x.associatedBuff).Distinct())
                    ignoredBuffDefs.Add(dotDebuff);
                foreach (BuffDef dotDebuff in DotController.dotDefs.Where(x => x.terminalTimedBuff != null).Select(x => x.terminalTimedBuff).Distinct())
                    ignoredBuffDefs.Add(dotDebuff);
#pragma warning restore Publicizer001 // Accessing a member that was not originally public
            };

            On.RoR2.CharacterBody.FixedUpdate += (orig, self) =>
            {
                orig(self);

                if (self && self.inventory)
                {
                    if (!self.HasBuff(hoodieBuffActive) && !self.HasBuff(hoodieBuffCooldown) && self.inventory.GetItemCountEffective(itemDef) > 0)
                    {
                        self.AddBuff(hoodieBuffActive);
                    }
                }
            };

            On.RoR2.CharacterBody.AddTimedBuffAuthority += (orig, self, buffIndex, duration) =>
            {
                if (buffIndex != BuffIndex.None)
                {
                    duration = ApplyHoodieBuffToBuff(self, BuffCatalog.GetBuffDef(buffIndex), duration);
                }
                orig(self, buffIndex, duration);
            };

            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += (orig, self, buffDef, duration) =>
            {
                duration = ApplyHoodieBuffToBuff(self, buffDef, duration);
                orig(self, buffDef, duration);
            };

            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float_int += (orig, self, buffDef, duration, stackCount) =>
            {
                duration = ApplyHoodieBuffToBuff(self, buffDef, duration);
                orig(self, buffDef, duration, stackCount);
            };

            On.RoR2.CharacterBody.AddTimedBuff_BuffIndex_float += (orig, self, buffIndex, duration) =>
            {
                if (buffIndex != BuffIndex.None)
                {
                    duration = ApplyHoodieBuffToBuff(self, BuffCatalog.GetBuffDef(buffIndex), duration);
                }
                orig(self, buffIndex, duration);
            };

            On.RoR2.CharacterBody.AddTimedBuffDontRefreshDuration += (orig, self, buffDef, duration, maxStacks) =>
            {
                duration = ApplyHoodieBuffToBuff(self, buffDef, duration);
                orig(self, buffDef, duration, maxStacks);
            };
        }

        private static float ApplyHoodieBuffToBuff(CharacterBody self, BuffDef buffDef, float duration)
        {
            if (self && self.inventory)
            {
                if (!buffDef.isDebuff && !buffDef.isCooldown && self.HasBuff(hoodieBuffActive) && !ignoredBuffDefs.Contains(buffDef))
                {
                    int count = self.inventory.GetItemCountEffective(itemDef);
                    duration *= 1 + durationIncreasePercent * count;

                    self.RemoveBuff(hoodieBuffActive);
                    self.AddTimedBuff(hoodieBuffCooldown, CalculateHoodieCooldown(count));
                }
            }

            return duration;
        }
    }
}
