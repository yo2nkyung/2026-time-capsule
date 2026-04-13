using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// Detects screen touches and places a prefab

public class ARPlaceCube : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Drag the AR Raycast Manager component from the XR Origin here.")]
    private ARRaycastManager arRaycastManager;

    [SerializeField]
    [Tooltip("The prefab to instantiate on the detected surface (e.g. Prefab Parent with a cube child).")]
    private GameObject placementPrefab;

    // Placement cooldown — prevents spawning dozens of objects on a single held tap.
    private const float PlacementCooldown = 0.25f;

    private ARPlaneManager arPlaneManager;
    private bool isPlacing = false;
    private GameObject spawnedInstance;

    private static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();

    private void Awake()
    {
        arPlaneManager = GetComponent<ARPlaneManager>();
    }

    private void Update()
    {
        if (isPlacing)
            return;

        Vector2 inputPosition = Vector2.zero;
        bool hasInput = false;

#if UNITY_EDITOR
        // Mouse click for Editor testing.
        if (Input.GetMouseButtonDown(0))
        {
            inputPosition = Input.mousePosition;
            hasInput = true;
        }
#else
        // Single finger tap on device.
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            inputPosition = Input.GetTouch(0).position;
            hasInput = true;
        }
#endif

        if (hasInput)
            PlaceObject(inputPosition);
    }

    // Casts an AR ray, If a plane is hit, the prefab is instantiated at the hit pose
    private void PlaceObject(Vector2 screenPosition)
    {
        if (arRaycastManager == null)
        {
            Debug.LogError("[ARPlaceCube] AR Raycast Manager is not assigned in the Inspector.");
            return;
        }

        if (placementPrefab == null)
        {
            Debug.LogError("[ARPlaceCube] Placement Prefab is not assigned in the Inspector.");
            return;
        }

        if (arRaycastManager.Raycast(screenPosition, Hits, TrackableType.AllTypes))
        {
            if (spawnedInstance != null)
                return;

            Pose hitPose = Hits[0].pose;
            spawnedInstance = Instantiate(placementPrefab, hitPose.position, hitPose.rotation);

            HidePlanes();

            // Update the HUD now that the hub is placed.
            HUDMessageController.Instance?.ShowMessage(
                "Select a Minigame Portal or fill your Time Capsule!");

            isPlacing = true;
            StartCoroutine(ResetPlacingAfterDelay(PlacementCooldown));
        }
    }

    // Hides tracked plane GameObjects and disables the ARPlaneManager
    private void HidePlanes()
    {
        if (arPlaneManager == null)
            return;

        foreach (ARPlane plane in arPlaneManager.trackables)
            plane.gameObject.SetActive(false);

        arPlaneManager.enabled = false;
    }

    // Waits for the cooldown duration then enables placement.
    private IEnumerator ResetPlacingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isPlacing = false;
    }
}
