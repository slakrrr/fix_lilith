using BepInEx;
using BepInEx.Unity.IL2CPP;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;


[BepInPlugin("slakr.fixlilith", "FixLilith", "1.1.0")]
public class FixLilithPlugin : BasePlugin {
    public override void Load() {
        var harmony = new Harmony("slakr.fixlilith");
        harmony.PatchAll(typeof(SetLayeredWindowAttributesPatch));
        harmony.PatchAll(typeof(VirtualScreenBoundsPatch));
        // harmony.PatchAll(typeof(PrefsPatch));
        harmony.PatchAll(typeof(NoRestartPatch));
        harmony.PatchAll(typeof(NeedsBootstrapPatch));
    }
}

// make virtual screen slightly smaller
// to avoid GNOME/Mutter (and some other Wayland compositors?)
// recognizing it as a fullscreen app
// so it can coexist with other fullscreen windows
// and doesnt make GNOME hide its topbar
[HarmonyPatch]
static class VirtualScreenBoundsPatch {
    static MethodBase TargetMethod() =>
        AccessTools.Method(AccessTools.TypeByName("WindowsPlatformService"),
                           "GetVirtualScreenBounds");

    static void Postfix(ref UnityEngine.RectInt __result) {
        __result.height -= 12;
    }
}

// somehow the game would restart after we change virtual screen bounds,
// which would cause BepInEx injection failure
// so we simply 'disable' restart
// a bit crude but seemingly works fine
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

// seemingly useless, but i will keep this just in case
// // let the game write a smaller virtualHeight into registry
// [HarmonyPatch]
// static class PrefsPatch {
//     static MethodBase TargetMethod() =>
//         AccessTools.Method(AccessTools.TypeByName("WindowsVirtualScreenBootstrapper"),
//                            "WriteScreenmanagerPrefs");
//
//     static void Prefix(ref int virtualWidth, ref int virtualHeight) {
//         virtualHeight -= 12;
//     }
// }


// the game would call SetLayeredWindowAttributes(...) multiple times
// with the same parameters, which causes flickering
// (because thats essentially destroying and recreating surface repeatedly)
// have no idea why the game do that
// here we simply skip duplicated calls, seemingly works fine
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