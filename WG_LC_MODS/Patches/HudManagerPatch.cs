using System.Collections;
using System.Linq;
using System.Reflection;
using DunGen;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace WG_LC_MODS.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HudManagerPatch
    {
        private static GameObject _totalCounter;
        private static TextMeshProUGUI _textMesh;
        
        private static GameObject _ValueCounter1;
        private static TextMeshProUGUI _Value1;
        
        private static GameObject _ValueCounter2;
        private static TextMeshProUGUI _Value2;
        
        private static GameObject _ValueCounter3;
        private static TextMeshProUGUI _Value3;

        private static GameObject _ValueCounter4;
        private static TextMeshProUGUI _Value4;

        private const float DisplayTime = 5f;

        [HarmonyPatch(typeof(HUDManager), "Awake")]
        [HarmonyPostfix]
        private static void CreateValueCounter()
        {
            for (int i = 0; i < 4; i++)
            {
                CreateValueHotbarItem(i + 1);
            }
        }


        [HarmonyPatch(typeof(HUDManager), "Update")]
        [HarmonyPostfix]
        private static void assignCounters(HUDManager __instance)
        {
            var LPC = GameNetworkManager.Instance.localPlayerController;
            if (LPC == null)
            {
                Plugin.Log.LogWarning("LPC is null!");
                return;
            }
            
            if (!LPC.IsOwner)
                return;

            for (int i = 0; i < 4; i++)
            {
                var item = LPC.ItemSlots[i];
                switch (i)
                {
                    case 0:
                        if (item == null)
                        {
                            _Value1.text = "0";
                            continue;
                        }
                        _Value1.text = $"{item.scrapValue:F0}";
                        break;
                    case 1:
                        if (item == null)
                        {
                            _Value2.text = "0";
                            continue;
                        }
                        _Value2.text = $"{item.scrapValue:F0}";
                        break;
                    case 2:
                        if (item == null)
                        {
                            _Value3.text = "0";
                            continue;
                        }
                        _Value3.text = $"{item.scrapValue:F0}";
                        break;
                    case 3:
                        if (item == null)
                        {
                            _Value4.text = "0";
                            continue;
                        }
                        _Value4.text = $"{item.scrapValue:F0}";
                        break;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HUDManager), "PingScan_performed")]
        private static void OnScan(HUDManager __instance, InputAction.CallbackContext context)
        {
            var playerPingingScan = Traverse.Create(__instance).Field("playerPingingScan").GetValue<float>();
            var CanPlayerScan = Traverse.Create(__instance).Method("CanPlayerScan").GetValue<bool>();
            if (GameNetworkManager.Instance.localPlayerController == null)
                return;

            if (!context.performed || !CanPlayerScan || playerPingingScan > -0.5f)
                return;
            // Only allow this special scan to work while inside the ship.
            if (!StartOfRound.Instance.inShipPhase && !GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
                return;
            if (!_totalCounter)
                CreateCopyValueCounter();
            float value = CalculateLootValue();
            _textMesh.text = $"{value:F0}";
            if (!_totalCounter.activeSelf)
                GameNetworkManager.Instance.StartCoroutine(ShipLootCoroutine());
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HUDManager), "Update")]
        private static void UpdateWeight(ref TextMeshProUGUI ___weightCounter, ref Animator ___weightCounterAnimator)
        {
            float num = Mathf.RoundToInt(Mathf.Clamp((GameNetworkManager.Instance.localPlayerController.carryWeight - 1f) * 0.4535f, 0f, 100f) * 105f);
            float num2 = Mathf.RoundToInt(Mathf.Clamp(GameNetworkManager.Instance.localPlayerController.carryWeight - 1f, 0f, 100f) * 105f);
            ___weightCounter.text = $"{num} kg";
            ___weightCounterAnimator.SetFloat("weight", num2 / 130f);
        }

        private static IEnumerator ShipLootCoroutine()
        {
            _totalCounter.SetActive(true);
            yield return new WaitForSeconds(DisplayTime);
            _totalCounter.SetActive(false);
        }

        private static float CalculateLootValue()
        {
            GameObject ship = GameObject.Find("/Environment/HangarShip");
            // Get all objects that can be picked up from inside the ship. Also remove items which technically have
            // scrap value but don't actually add to your quota.
            var loot = ship.GetComponentsInChildren<GrabbableObject>()
                .Where(obj => obj.name != "ClipboardManual" && obj.name != "StickyNoteItem").ToList();
            Plugin.Log.LogDebug("Calculating total ship scrap value.");
            loot.Do(scrap => Plugin.Log.LogDebug($"{scrap.name} - ${scrap.scrapValue}"));
            return loot.Sum(scrap => scrap.scrapValue);
        }

        private static void CreateCopyValueCounter()
        {
            GameObject valueCounter = GameObject.Find("/Systems/UI/Canvas/IngamePlayerHUD/BottomMiddle/ValueCounter");
            if (!valueCounter)
                Plugin.Log.LogError("Failed to find ValueCounter object to copy!");
            _totalCounter = Object.Instantiate(valueCounter.gameObject, valueCounter.transform.parent, false);
            _totalCounter.transform.Translate(0f, 1f, 0f);
            Vector3 pos = _totalCounter.transform.localPosition;
            _totalCounter.transform.localPosition = new Vector3(pos.x + 50f, -50f, pos.z);
            _textMesh = _totalCounter.GetComponentInChildren<TextMeshProUGUI>();
        }

        private static void CreateValueHotbarItem(int index)
        {
            GameObject valueCounter = GameObject.Find($"/Systems/UI/Canvas/IngamePlayerHUD/BottomMiddle/ValueCounter");
            if (!valueCounter)
                Plugin.Log.LogError("Failed to find ValueCounter object to copy!");

            TextMeshProUGUI textMesh = null;
            Vector3 pos = Vector3.zero;

            switch (index)
            {
                case 1:
                    _ValueCounter1 = Object.Instantiate(valueCounter.gameObject, valueCounter.transform.parent, false);
                    // Remove second child from the value counter, which is the background.
                    Object.Destroy(_ValueCounter1.transform.GetChild(1).gameObject);
                    _ValueCounter1.transform.Translate(0f, 1f, 0f);
                    pos = _ValueCounter1.transform.localPosition;
                    _ValueCounter1.transform.localPosition = new Vector3(-115f, -70f, pos.z);
                    textMesh = _ValueCounter1.GetComponentInChildren<TextMeshProUGUI>();
                    textMesh.text = "0";
                    textMesh.alignment = TextAlignmentOptions.Center;
                    textMesh.color = Color.red;
                    _ValueCounter1.SetActive(true);
                    _Value1 = textMesh;
                    break;
                case 2:
                    _ValueCounter2 = Object.Instantiate(valueCounter.gameObject, valueCounter.transform.parent, false);
                    Object.Destroy(_ValueCounter2.transform.GetChild(1).gameObject);
                    _ValueCounter2.transform.Translate(0f, 1f, 0f);
                    pos = _ValueCounter2.transform.localPosition;
                    _ValueCounter2.transform.localPosition = new Vector3(-70, -70f, pos.z);
                    textMesh = _ValueCounter2.GetComponentInChildren<TextMeshProUGUI>();
                    textMesh.text = "0";
                    textMesh.alignment = TextAlignmentOptions.Center;
                    textMesh.color = Color.red;
                    _ValueCounter2.SetActive(true);
                    _Value2 = textMesh;
                    break;
                case 3:
                    _ValueCounter3 = Object.Instantiate(valueCounter.gameObject, valueCounter.transform.parent, false);
                    Object.Destroy(_ValueCounter3.transform.GetChild(1).gameObject);
                    _ValueCounter3.transform.Translate(0f, 1f, 0f);
                    pos = _ValueCounter3.transform.localPosition;
                    _ValueCounter3.transform.localPosition = new Vector3(-25f, -70f, pos.z);
                    textMesh = _ValueCounter3.GetComponentInChildren<TextMeshProUGUI>();
                    textMesh.text = "0";
                    textMesh.alignment = TextAlignmentOptions.Center;
                    textMesh.color = Color.red;
                    _ValueCounter3.SetActive(true);
                    _Value3 = textMesh;
                    break;
                case 4:
                    _ValueCounter4 = Object.Instantiate(valueCounter.gameObject, valueCounter.transform.parent, false);
                    Object.Destroy(_ValueCounter4.transform.GetChild(1).gameObject);
                    _ValueCounter4.transform.Translate(0f, 1f, 0f);
                    pos = _ValueCounter4.transform.localPosition;
                    _ValueCounter4.transform.localPosition = new Vector3(19, -70f, pos.z);
                    textMesh = _ValueCounter4.GetComponentInChildren<TextMeshProUGUI>();
                    textMesh.text = "0";
                    textMesh.alignment = TextAlignmentOptions.Center;
                    textMesh.color = Color.red;
                    _ValueCounter4.SetActive(true);
                    _Value4 = textMesh;
                    break;
            }
        }
    }
}
