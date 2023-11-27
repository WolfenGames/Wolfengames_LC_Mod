using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace WG_LC_MODS.Patches
{
    [HarmonyPatch]
    internal class InstaLoadPatch
    {
        [HarmonyPatch(typeof(PreInitSceneScript), "Awake")]
        [HarmonyPrefix]
        private static void SkipPreInitScene()
        {
            SceneManager.LoadScene("InitScene");
        }

        [HarmonyPatch(typeof(InitializeGame), "Start")]
        [HarmonyPrefix]
        private static void SkipInitScene()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
