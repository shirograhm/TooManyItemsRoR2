using System.IO;
using UnityEngine;

namespace TooManyItems.Managers
{
    public static class AssetManager
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
