using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
public class ARImageSpawner : MonoBehaviour
{
    private const string ScanPrompt = "Point your camera at the QR code to place the hub.";
    private const string PlacedPrompt = "Select a Minigame Portal or fill your Time Capsule!";

    [SerializeField]
    [Tooltip("The prefab to instantiate when the QR code image is first detected.")]
    private GameObject placementPrefab;

    [SerializeField]
    [Tooltip("Optional rotation offset applied on top of the image's tracked pose (Euler degrees).")]
    private Vector3 rotationOffset = Vector3.zero;

    [SerializeField]
    [Tooltip("The reset button GameObject in the HUD. Shown after placement, hidden again after reset.")]
    private GameObject resetButton;

    private ARTrackedImageManager imageManager;
    private GameObject spawnedInstance;

    private void Awake()
    {
        imageManager = GetComponent<ARTrackedImageManager>();

        if (imageManager == null)
            Debug.LogError("[ARImageSpawner] No ARTrackedImageManager found on this GameObject. Add it to XR Origin.");
    }

    private void OnEnable()
    {
        if (placementPrefab == null)
        {
            Debug.LogError("[ARImageSpawner] Placement Prefab is not assigned in the Inspector.");
            return;
        }

        if (imageManager != null)
            imageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    private void OnDisable()
    {
        if (imageManager != null)
            imageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }
    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (ARTrackedImage trackedImage in args.added)
        {
            if (TrySpawn(trackedImage))
                return;
        }

        foreach (ARTrackedImage trackedImage in args.updated)
        {
            if (TrySpawn(trackedImage))
                return;
        }
    }
    private bool TrySpawn(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState == TrackingState.None)
            return false;

        // Single-spawn guard — already placed.
        if (spawnedInstance != null)
            return false;

        Quaternion spawnRotation = trackedImage.transform.rotation * Quaternion.Euler(rotationOffset);
        spawnedInstance = Instantiate(placementPrefab, trackedImage.transform.position, spawnRotation);

        HUDMessageController.Instance?.ShowMessage(PlacedPrompt);

        // Disable tracking — no longer needed after placement.
        imageManager.enabled = false;

        resetButton?.SetActive(true);
        return true;
    }
    public void Reset()
    {
        if (spawnedInstance != null)
        {
            Destroy(spawnedInstance);
            spawnedInstance = null;
        }

        resetButton?.SetActive(false);
        imageManager.enabled = true;
        HUDMessageController.Instance?.ShowMessage(ScanPrompt);
    }
}
