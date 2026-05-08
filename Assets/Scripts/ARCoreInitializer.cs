using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

public class ARCoreInitializer : MonoBehaviour
{
    public GameObject arSessionObject; // connect AR Session GameObject from inspector

    IEnumerator Start()
    {
        var manager = XRGeneralSettings.Instance?.Manager;
        if (manager == null) yield break;

        manager.InitializeLoaderSync();
        yield return null; // wait for one frame

        if (manager.activeLoader == null)
        {
            Debug.LogError("ARCore loader initialize failed");
            yield break;
        }

        manager.StartSubsystems();
        Debug.Log("ARCore initialize success: " + manager.activeLoader.name);
        
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Debug.Log("AR Session SetActive(true) called");
        if (arSessionObject != null)
        {
            Debug.Log("AR Session SetActive(true) called");
            arSessionObject.SetActive(true); // now activate ARSession
        }
    }
}