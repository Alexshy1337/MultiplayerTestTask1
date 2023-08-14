using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AutoRefreshOptions
{

    [MenuItem("Tools/Lock Reload")]
    static void LockReload()
    {
        EditorApplication.LockReloadAssemblies();
    }

    [MenuItem("Tools/Unlock Reload")]
    static void UnlockReload()
    {
        EditorApplication.UnlockReloadAssemblies();
    }
}
