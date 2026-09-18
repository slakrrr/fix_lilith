using BepInEx;
using BepInEx.Unity.IL2CPP;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;


[BepInPlugin("com.slakr.fixlilith", "FixLilith", "1.0.0")]
public class FixLilithPlugin : BasePlugin {
    public override void Load() {
        var harmony = new Harmony("com.slakr.fixlilith");
        harmony.PatchAll(typeof(SetLayeredWindowAttributesPatch));
        harmony.PatchAll(typeof(VirtualScreenBoundsPatch));
        // harmony.PatchAll(typeof(PrefsPatch));
        harmony.PatchAll(typeof(NoRestartPatch));
        harmony.PatchAll(typeof(NeedsBootstrapPatch));
    }
}

// make virtual screen slightly smaller
// to avoid gnome recognize it as fullscreen app
[HarmonyPatch]
static class VirtualScreenBoundsPatch {
    static MethodBase TargetMethod() =>
        AccessTools.Method(AccessTools.TypeByName("WindowsPlatformService"),
                           "GetVirtualScreenBounds");

    static void Postfix(ref UnityEngine.RectInt __result) {
        __result.height -= 12;
    }
}

// somehow game would restart after we change
// virtual screen bound, which would cause
// bepinex injection fails
// so we simply 'disable' retart
[HarmonyPatch]
static class NeedsBootstrapPatch {
    static MethodBase TargetMethod() =>
        AccessTools.Method(AccessTools.TypeByName("WindowsVirtualScreenBootstrapper"),
                           "NeedsBootstrap");
    static bool Prefix(ref bool __result) { __result = false; return false; }
}
[HarmonyPatch]
public class NoRestartPatch {
    static MethodBase TargetMethod() =>
        AccessTools.Method(AccessTools.TypeByName("WindowsVirtualScreenBootstrapper"),
                           "RestartToApplyDisplayScope");
    static bool Prefix(string reason) { return false; }
}

// // seemingly useless, but i would keep this just for case
// [HarmonyPatch]
// static class PrefsPatch {
//     static MethodBase TargetMethod() =>
//         AccessTools.Method(AccessTools.TypeByName("WindowsVirtualScreenBootstrapper"),
//                            "WriteScreenmanagerPrefs");

//     static void Prefix(ref int virtualWidth, ref int virtualHeight) {
//         virtualHeight -= 12;
//     }
// }


// game would call SetLayeredWindowAttributes(...) multiple times
// with same parameters, which will cause flickering
// so we simply intercept the same call
[HarmonyPatch]
public class SetLayeredWindowAttributesPatch {
    private static bool   m_inited;
    private static IntPtr m_last_hwnd;
    private static int    m_last_crKey;
    private static byte   m_last_bAlpha;
    private static int    m_last_dwFlags;

    static MethodBase TargetMethod() {
        var type   = AccessTools.TypeByName("WindowsNativeAPI");
        var method = AccessTools.Method(type, "SetLayeredWindowAttributes");
        return method;
    }

    static bool Prefix(IntPtr hwnd, int crKey, byte bAlpha, int dwFlags) {
        if(is_same_call(hwnd, crKey, bAlpha, dwFlags)) return false;
        m_inited       = true;
        m_last_hwnd    = hwnd;
        m_last_crKey   = crKey;
        m_last_bAlpha  = bAlpha;
        m_last_dwFlags = dwFlags;
        return true;
    }

    private static bool is_same_call(IntPtr hwnd, int crKey, byte bAlpha, int dwFlags) {
        return 
            m_inited && 
            m_last_hwnd    == hwnd &&
            m_last_crKey   == crKey &&
            m_last_bAlpha  == bAlpha &&
            m_last_dwFlags == dwFlags;
    }
}