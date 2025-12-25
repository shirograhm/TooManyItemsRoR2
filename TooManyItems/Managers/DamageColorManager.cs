using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems.Managers
{
    class DamageColorManager
    {
        public static List<DamageColorIndex> registeredColorIndexList = new();

        internal static void Init()
        {
            Hooks();
        }

        private static void Hooks()
        {
            On.RoR2.DamageColor.FindColor += (orig, colorIndex) =>
            {
                if (registeredColorIndexList.Contains(colorIndex))
                {
                    return DamageColor.colors[(int)colorIndex];
                }

                return orig(colorIndex);
            };
        }

        public static DamageColorIndex RegisterDamageColor(Color color)
        {
            int nextColorIndex = DamageColor.colors.Length;
            DamageColorIndex newDamageColorIndex = (DamageColorIndex)nextColorIndex;

            HG.ArrayUtils.ArrayAppend(ref DamageColor.colors, color);
            registeredColorIndexList.Add(newDamageColorIndex);

            return newDamageColorIndex;
        }
    }
}
