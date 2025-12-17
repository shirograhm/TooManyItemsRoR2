using RoR2;
using UnityEngine;

namespace TooManyItems
{
    internal abstract class BaseItem
    {
        public static ItemDef itemDef;

        public static void GenerateItem(string internalName, string prefabName, string iconName, ItemTier tier, ItemTag[] tags)
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = internalName;
            itemDef.AutoPopulateTokens();

            Utils.SetItemTier(itemDef, tier);

            GameObject prefab = AssetHandler.bundle.LoadAsset<GameObject>(prefabName);
            ModelPanelParameters modelPanelParameters = prefab.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = prefab.transform;
            modelPanelParameters.cameraPositionTransform = prefab.transform;
            modelPanelParameters.maxDistance = 10f;
            modelPanelParameters.minDistance = 5f;

            itemDef.pickupIconSprite = AssetHandler.bundle.LoadAsset<Sprite>(iconName);
            itemDef.pickupModelPrefab = prefab;
            itemDef.canRemove = true;
            itemDef.hidden = false;

            itemDef.tags = tags;
        }

        public static void Hooks() { }
    }
}
