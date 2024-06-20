using R2API;
using System.IO;
using UnityEngine;

namespace TooManyItems
{
    public static class AssetHandler
    {
        public static AssetBundle bundle;
        public const string bundleName = "tmiassets";

        public const uint LUNAR_REVIVE_TICKING_ID = 954093529;
        public const uint TROWEL_CONSUME_ID = 522946673;

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
            // Load sounds
            SoundAPI.SoundBanks.Add(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(TooManyItems.PInfo.Location), "TMI_SoundBank.bnk")));
        }
    }
}
