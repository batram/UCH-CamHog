using HarmonyLib;
using UnityEngine;

namespace CamHogMod.Patches
{
    [HarmonyPatch(typeof(GameState), nameof(GameState.Update))]
    static class GameStateUpdatePatch
    {
        static void Prefix(GameState __instance)
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                if (Input.GetKeyDown(CamHogMod.ToggleKey.Value))
                {
                    ToggleFocus();
                }
                if (Input.GetKeyDown(CamHogMod.OpacityUp.Value))
                {
                    IncreaseOpacity();
                }
                if (Input.GetKeyDown(CamHogMod.OpacityDown.Value))
                {
                    DecreaseOpacity();
                }
                if (Input.GetKeyDown(CamHogMod.DistanceUp.Value))
                {
                    IncreaseDistance();
                }
                if (Input.GetKeyDown(CamHogMod.DistanceDown.Value))
                {
                    DecreaseDistance();
                }
            }
        }

        static void ToggleFocus()
        {
            CamHogMod.Enabled.Value = !CamHogMod.Enabled.Value;
            DisplayMessage("CamHogMod: " + (CamHogMod.Enabled.Value ? "Enabled" : "Disabled"));
        }

        static void IncreaseDistance()
        {
            CamHogMod.PlayerDistance.Value += 1;
            CamHogMod.circleTime = CamHogMod.defaultCircleTime;
            DisplayMessage("Distance up: " + CamHogMod.PlayerDistance.Value);
        }
        static void DecreaseDistance()
        {
            if (CamHogMod.PlayerDistance.Value >= 0)
            {
                CamHogMod.PlayerDistance.Value -= 1;
                CamHogMod.circleTime = CamHogMod.defaultCircleTime;
            }
            DisplayMessage("Distance down: " + CamHogMod.PlayerDistance.Value);
        }

        static void IncreaseOpacity()
        {
            if (CamHogMod.Opacity.Value < 1f)
            {
                CamHogMod.Opacity.Value += 0.1f;
            }
            DisplayMessage("Opacity up: " + CamHogMod.Opacity.Value.ToString("0.00"));
        }
        static void DecreaseOpacity()
        {
            if (CamHogMod.Opacity.Value > 0f)
            {
                CamHogMod.Opacity.Value -= 0.1f;
            }
            DisplayMessage("Opacity down: " + CamHogMod.Opacity.Value.ToString("0.00"));
        }

        static void DisplayMessage(string message)
        {
            UserMessageManager.Instance.UserMessage(message, 2, 0, false);
        }
    }
}