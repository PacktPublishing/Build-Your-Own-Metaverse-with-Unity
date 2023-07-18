using UnityEngine;
using UnityEngine.InputSystem.UI;

public class UISystemManager : MonoBehaviour
{
    void Awake()
    {
        try
        {
            var headsetType = OVRPlugin.GetSystemHeadsetType();

            // Using the headsetType variable, we can detect if we are running the project in virtual reality glasses.If it returns None, it means that we are not running in virtual reality.
            if (headsetType == OVRPlugin.SystemHeadset.None)
            {
                // Access the InputSystemUI component and disable it.
                GetComponent<InputSystemUIInputModule>().enabled = true;
            }
            else
            {
                // If we forget to manually activate this Component, we activate it
                GetComponent<InputSystemUIInputModule>().enabled = false;
            }
        }
        catch (System.Exception ex)
        {
            // It is possible that on some platforms and in Unity Editor the OpenVR library is not found and causes an error, in that case we will also activate the classic UI system.
            GetComponent<InputSystemUIInputModule>().enabled = true;
        }
    }
}
