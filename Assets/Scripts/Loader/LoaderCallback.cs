using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoaderCallback : MonoBehaviour
{
    private bool isFirstUpdate = true;

    private void Update()
    {
        if(isFirstUpdate)
        {
            //Debug.Log("exec on first update");
            isFirstUpdate = false;
            Loader.LoaderCallback();
        }
    }
}
