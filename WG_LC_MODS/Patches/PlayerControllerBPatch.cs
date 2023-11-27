using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Object = UnityEngine.Object;

namespace WG_LC_MODS.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static GameObject _inventoryValue;
        private static TextMeshProUGUI _textMesh;

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        static void IncreaseSprintDuration(ref float ___movementSpeed, ref float ___sprintTime, ref float ___sprintMultiplier, ref float ___climbSpeed)
        {
            ___sprintTime = 5.0f * 1.5f;
            ___sprintMultiplier = 1.5f;
            ___climbSpeed = 4.0f * 1.5f;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void CurrentLootCounter(PlayerControllerB __instance)
        {
            if (__instance.playerUsername.StartsWith("Player #"))
                return;
            if (!__instance.IsOwner)
                return;
            if (!_inventoryValue)
                CopyValueCounter();
            float value = CalculateLootValue(__instance);
            if (!_inventoryValue.activeSelf)
                _inventoryValue.SetActive(true);
            _textMesh.text = $"{value:F0}";
        }

        private static float CalculateLootValue(PlayerControllerB __instance)
        {
            float value = 0.0f;
            var gos = __instance.ItemSlots.Where(obj => obj != null);
            value = gos.Sum(obj => obj.scrapValue);
            return value;
        }

        private static void CopyValueCounter()
        {
            GameObject valueCounter = GameObject.Find("/Systems/UI/Canvas/IngamePlayerHUD/BottomMiddle/ValueCounter");
            if (!valueCounter)
                Plugin.Log.LogError("Failed to find ValueCounter object to copy!");
            _inventoryValue = Object.Instantiate(valueCounter.gameObject, valueCounter.transform.parent, false);
            _inventoryValue.transform.Translate(0f, 1f, 0f);
            Vector3 pos = _inventoryValue.transform.localPosition;
            _inventoryValue.transform.localPosition = new Vector3(pos.x + 50f, -70f, pos.z);
            _textMesh = _inventoryValue.GetComponentInChildren<TextMeshProUGUI>();
            _textMesh.color = Color.blue;
        }

        [HarmonyPatch("DamagePlayer")]
        [HarmonyPostfix]
        private static void AnnounceDeath(PlayerControllerB __instance)
        {
            if (__instance.IsOwner && __instance.isPlayerDead)
            {
                string message = __instance.playerUsername + " has died!";
                // Make the message red.
                message = "<color=#ff0000ff>" + message + "</color>";

                HUDManager.Instance.AddTextToChatOnServer(message);
            }
                
        }
    }
}
