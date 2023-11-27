using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TerminalApi;
using WG_LC_MODS.Patches;

namespace WG_LC_MODS
{
    [BepInPlugin("WG_LC_MODS", "WolfenGamesMods", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("WG_LC_MODS");

        private static Plugin PluginInstance;

        internal static ManualLogSource Log;

        public static Plugin GetInstance()
        {
            return PluginInstance;
        }

        void Awake()
        {
            if (PluginInstance == null)
            {
                PluginInstance = this;
            }

            Log = BepInEx.Logging.Logger.CreateLogSource("WG_LC_MODS");

            Log.LogInfo("WolfenGamesMods loaded");

            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(HudManagerPatch));
            harmony.PatchAll(typeof(InstaLoadPatch));
        }
    }
}
