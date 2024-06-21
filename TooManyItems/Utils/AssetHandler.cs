using R2API;
using System.IO;
using UnityEngine;

namespace TooManyItems
{
    public static class AssetHandler
    {
        public static AssetBundle bundle;
        public const string bundleName = "tmiassets";

        public static string AssetBundlePath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(TooManyItems.PInfo.Location), bundleName);
            }
        }

        public static void Init()
        {
            bundle = AssetBundle.LoadFromFile(AssetBundlePath);
        }
    }
}
