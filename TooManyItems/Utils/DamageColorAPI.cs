using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TooManyItems
{
    class DamageColorAPI
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
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                if (registeredColorIndexList.Contains(colorIndex))
                {
                    return DamageColor.colors[(int)colorIndex];
                }
#pragma warning restore Publicizer001 // Accessing a member that was not originally public

                return orig(colorIndex);
            };
        }

        public static DamageColorIndex RegisterDamageColor(Color color)
        {
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
            int nextColorIndex = DamageColor.colors.Length;
            DamageColorIndex newDamageColorIndex = (DamageColorIndex)nextColorIndex;

            HG.ArrayUtils.ArrayAppend(ref DamageColor.colors, color);
            registeredColorIndexList.Add(newDamageColorIndex);
#pragma warning restore Publicizer001 // Accessing a member that was not originally public

            return newDamageColorIndex;
        }
    }
}
