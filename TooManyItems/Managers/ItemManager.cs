using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TooManyItems.Managers
{
    public static class ItemManager
    {
        public static EquipmentDef GenerateEquipment(string name, float cooldown, bool isLunar = false, bool appearsInMultiPlayer = true, bool appearsInSinglePlayer = true, bool canBeRandomlyTriggered = true, bool enigmaCompatible = true, bool canDrop = true)
        {
            EquipmentDef equipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();

            equipmentDef.name = name.ToUpperInvariant();
            equipmentDef.AutoPopulateTokens();

            GameObject prefab = AssetManager.bundle.LoadAsset<GameObject>(name + ".prefab");
            if (prefab == null)
            {
                Log.Warning("Missing prefab file for equipment " + equipmentDef.name + ". Substituting default...");
                prefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            }
            ModelPanelParameters modelPanelParameters = prefab.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = prefab.transform;
            modelPanelParameters.cameraPositionTransform = prefab.transform;
            modelPanelParameters.maxDistance = 10f;
            modelPanelParameters.minDistance = 5f;

            equipmentDef.pickupIconSprite = AssetManager.bundle.LoadAsset<Sprite>(name + ".png");
            equipmentDef.pickupModelPrefab = prefab;

            equipmentDef.isLunar = isLunar;
            if (isLunar) equipmentDef.colorIndex = ColorCatalog.ColorIndex.LunarItem;

            equipmentDef.appearsInMultiPlayer = appearsInMultiPlayer;
            equipmentDef.appearsInSinglePlayer = appearsInSinglePlayer;
            equipmentDef.canBeRandomlyTriggered = canBeRandomlyTriggered;
            equipmentDef.enigmaCompatible = enigmaCompatible;
            equipmentDef.canDrop = canDrop;

            equipmentDef.cooldown = cooldown;

            // Add item to item dict
            ItemDisplayRuleDict displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomEquipment(equipmentDef, displayRules));

            return equipmentDef;
        }

        public static ItemDef GenerateItem(string name, ItemTag[] tags, ItemTier tier, List<DisplayRuleData> rules = null)
        {
            ItemDef itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = name.ToUpperInvariant();
            itemDef.AutoPopulateTokens();

            SetItemTier(itemDef, tier);

            GameObject prefab = AssetManager.bundle.LoadAsset<GameObject>(name + ".prefab");
            if (prefab == null)
            {
                Log.Warning("Missing prefab file for item " + itemDef.name + ". Substituting default...");
                prefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();
            }
            ModelPanelParameters modelPanelParameters = prefab.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = prefab.transform;
            modelPanelParameters.cameraPositionTransform = prefab.transform;
            modelPanelParameters.maxDistance = 10f;
            modelPanelParameters.minDistance = 5f;

            if (itemDef.tier == ItemTier.VoidBoss || itemDef.tier == ItemTier.VoidTier1 ||
                itemDef.tier == ItemTier.VoidTier2 || itemDef.tier == ItemTier.VoidTier3)
            {
                itemDef.requiredExpansion = TooManyItems.sotvDLC;
            }


            itemDef.pickupIconSprite = AssetManager.bundle.LoadAsset<Sprite>(name + ".png");
            itemDef.pickupModelPrefab = prefab;
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = tags;

            // Add item to item dict, with display rules
            ItemDisplayRuleDict displayRules = new(null);
            if (rules != null)
                foreach (var rule in rules)
                    displayRules.Add(rule.survivorName, GenerateItemDisplayRule(prefab, rule));

            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            return itemDef;
        }

        public static ItemDisplayRule GenerateItemDisplayRule(GameObject prefab, DisplayRuleData data)
        {
            return new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = prefab,
                childName = data.partName,
                localPos = data.localPos,
                localAngles = data.localAngles,
                localScale = data.localScale
            };
        }

        public class DisplayRuleData(string survivorModelName, string partName, Vector3 localPos, Vector3 localAngles, Vector3 localScale)
        {
            public string survivorName = survivorModelName;
            public string partName = partName;
            public Vector3 localPos = localPos;
            public Vector3 localAngles = localAngles;
            public Vector3 localScale = localScale;
        }

        public static BuffDef GenerateBuff(string name, Sprite sprite, bool canStack = false, bool isHidden = false, bool isDebuff = false, bool isCooldown = false)
        {
            BuffDef returnable = ScriptableObject.CreateInstance<BuffDef>();

            returnable.name = name;
            returnable.iconSprite = sprite;
            returnable.canStack = canStack;
            returnable.isHidden = isHidden;
            returnable.isDebuff = isDebuff;
            returnable.isCooldown = isCooldown;

            return returnable;
        }

        public static void SetItemTier(ItemDef itemDef, ItemTier tier)
        {
            if (tier == ItemTier.NoTier)
            {
                try
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    itemDef.deprecatedTier = tier;
#pragma warning restore CS0618 // Type or member is obsolete
                }
                catch (Exception e)
                {
                    Log.Warning(string.Format("Error setting deprecatedTier for {0}: {1}", itemDef.name, e));
                }
            }

            ItemTierCatalog.availability.CallWhenAvailable(() =>
            {
                if (itemDef) itemDef.tier = tier;
            });
        }
    }
}
