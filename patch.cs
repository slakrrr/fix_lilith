using BepInEx;
using BepInEx.Unity.IL2CPP;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;

// WindowsNativeAPI ::
// public static unsafe int
// SetLayeredWindowAttributes(IntPtr hwnd, int crKey, byte bAlpha, int dwFlags)

[BepInPlugin("com.slakr.fixlilith", "FixLilith", "1.0.0")]
public class FixLilithPlugin : BasePlugin {
    public override void Load() {
        var harmony = new Harmony("com.slakr.fixlilith");
        harmony.PatchAll(typeof(SetLayeredWindowAttributesPatch));
    }
}

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