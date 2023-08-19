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

public class ScriptOptions
{
    //Auto Refresh


    //kAutoRefresh has two posible values
    //0 = Auto Refresh Disabled
    //1 = Auto Refresh Enabled


    //This is called when you click on the 'Tools/Auto Refresh' and toggles its value
    [MenuItem("Tools/Auto Refresh")]
    static void AutoRefreshToggle()
    {
        var status = EditorPrefs.GetInt("kAutoRefresh");
        if (status == 1)
            EditorPrefs.SetInt("kAutoRefresh", 0);
        else
            EditorPrefs.SetInt("kAutoRefresh", 1);
    }


    //This is called before 'Tools/Auto Refresh' is shown to check the current value
    //of kAutoRefresh and update the checkmark
    [MenuItem("Tools/Auto Refresh", true)]
    static bool AutoRefreshToggleValidation()
    {
        var status = EditorPrefs.GetInt("kAutoRefresh");
        if (status == 1)
            Menu.SetChecked("Tools/Auto Refresh", true);
        else
            Menu.SetChecked("Tools/Auto Refresh", false);
        return true;
    }
}