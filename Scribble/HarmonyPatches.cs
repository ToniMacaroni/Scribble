using HarmonyLib;
using IPA.Utilities;
using UnityEngine;
using UnityEngine.XR;
using VRUIControls;

namespace Scribble
{
    //[HarmonyPatch("OnEnable")]
    //[HarmonyPatch(typeof(VRPointer))]
    //public class VRPointerOnEnablePatch
    //{
    //    public static void Postfix(VRPointer __instance)
    //    {
    //        VRController leftController = __instance.GetField<VRController, VRPointer>("_leftVRController");
    //        VRController rightController = __instance.GetField<VRController, VRPointer>("_rightVRController");
    //        if (leftController.GetComponent<BrushBehaviour>() == null)
    //        {
    //            var brush = leftController.gameObject.AddComponent<BrushBehaviour>();
    //            brush.SaberType = SaberType.SaberA;
    //            brush.Pointer = __instance;
    //        }

    //        if (rightController.GetComponent<BrushBehaviour>() == null)
    //        {
    //            var brush = rightController.gameObject.AddComponent<BrushBehaviour>();
    //            brush.SaberType = SaberType.SaberB;
    //            brush.Pointer = __instance;
    //        }
    //    }
    //}
}
