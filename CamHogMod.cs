using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using BepInEx.Configuration;

[assembly: AssemblyVersion("0.0.0.1")]
[assembly: AssemblyInformationalVersion("0.0.0.1")]

namespace CamHogMod
{
    [BepInPlugin("CamHog", "CamHog", "0.0.0.1")]
    public class CamHogMod : BaseUnityPlugin
    {
        public static ConfigEntry<bool> Enabled;
        public static ConfigEntry<int> PlayerDistance;
        public static ConfigEntry<float> Opacity;

        public static ConfigEntry<KeyCode> ToggleKey;
        public static ConfigEntry<KeyCode> OpacityUp;
        public static ConfigEntry<KeyCode> OpacityDown;
        public static ConfigEntry<KeyCode> DistanceUp;
        public static ConfigEntry<KeyCode> DistanceDown;

        public static float circleTime = 0;
        public const float defaultCircleTime = 1;

        void Awake()
        {
            Debug.Log("All Eyes on ME, plz!");
            new Harmony("CamHog").PatchAll();

            Enabled = Config.Bind("General", "Enabled", true);
            PlayerDistance = Config.Bind("General", "PlayerDistance", 5, "Distance in which other players are faded, -1 for always fade");
            Opacity = Config.Bind("General", "Opacity", 0.30f, "Set the alpha/transparency of other Players. [1: solid, ..., 0: invisible]");

            ToggleKey = Config.Bind("INPUT", "ToggleKey", KeyCode.O, "Keybinding: Toggle on or off");
            OpacityUp = Config.Bind("INPUT", "OpacityUp", KeyCode.Equals, "Keybinding: increase opacity (Equals is Plus for some reason)");
            OpacityDown = Config.Bind("INPUT", "OpacityDown", KeyCode.Minus, "Keybinding: decrease opacity");
            DistanceUp = Config.Bind("INPUT", "DistanceUp", KeyCode.Period, "Keybinding: increase distance");
            DistanceDown = Config.Bind("INPUT", "DistanceDown", KeyCode.Comma, "Keybinding: decrease distance");
        }

        static bool isTooClose(Character offender)
        {
            if (LobbyManager.instance != null)
            {
                foreach (Character cha in Resources.FindObjectsOfTypeAll<Character>())
                {
                    if (cha != null && isLocal(cha))
                    {
                        float dis = Vector3.Distance(offender.transform.position, cha.transform.position);

                        if (dis <= PlayerDistance.Value)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        static bool isLocal(Character cha)
        {
            bool isLocalPlayer = false;

            if (cha.associatedLobbyPlayer != null)
            {
                isLocalPlayer = cha.associatedLobbyPlayer.isLocalPlayer;
            }

            if (cha.associatedGamePlayer != null)
            {
                isLocalPlayer = cha.associatedGamePlayer.isLocalPlayer;
            }

            return isLocalPlayer;
        }

        static bool shouldFade(Character cha)
        {
            return Enabled.Value && cha.picked && !isLocal(cha) && isTooClose(cha);
        }

        public static void DrawDebugCircle(Character cha)
        {
            LineRenderer lineRenderer = cha.gameObject.GetComponent<LineRenderer>();

            if (!cha.picked || !isLocal(cha) || PlayerDistance.Value <= 1)
            {
                if (lineRenderer != null)
                {
                    lineRenderer.enabled = false;
                }
                return;
            }

            Vector3 position = cha.transform.position;
            float radius = PlayerDistance.Value - 1;
            float theta_scale = 0.1f;  // Circle resolution


            if (lineRenderer == null)
            {
                lineRenderer = cha.gameObject.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.blue;
                lineRenderer.startWidth = 0.2f;
                lineRenderer.endWidth = 0.2f;
                lineRenderer.positionCount = ( (int) (2 * Mathf.PI / theta_scale) + 1);
                lineRenderer.useWorldSpace = false;
            }

            if (circleTime <= 0)
            {
                lineRenderer.enabled = false;
                return;
            }
            lineRenderer.enabled = true;

            circleTime -= Time.deltaTime;

            int i = 0;
            for (float theta = 0; theta < 2 * Mathf.PI; theta += theta_scale)
            {
                var x = radius * Mathf.Cos(theta);
                var y = radius * Mathf.Sin(theta);

                Vector3 pos = new Vector3(x, y, 0);
                lineRenderer.SetPosition(i, pos);
                i += 1;
            }

        }



        [HarmonyPatch(typeof(Character), nameof(Character.Update))]
        static class CharacterUpdatePatch
        {
            static void Postfix(Character __instance)
            {
                if (shouldFade(__instance))
                {
                    __instance.sprite.SetAlpha(Opacity.Value);
                    if (__instance.HasJetpack)
                    {
                        __instance.JetPackSR.SetAlpha(Opacity.Value);
                    }
                    __instance.nameTag.maxOpacity = Opacity.Value;
                }

                DrawDebugCircle(__instance);
            }
        }

        [HarmonyPatch(typeof(CharacterOpacityController), nameof(CharacterOpacityController.Update))]
        static class CharacterOpacityControllerUpdatePatch
        {
            static void Postfix(CharacterOpacityController __instance)
            {
                if (shouldFade(__instance.character))
                {
                    __instance.currentOpacity = Opacity.Value;
                } else
                {
                    __instance.currentOpacity = 1f;
                }
            }
        }

        
    }
}