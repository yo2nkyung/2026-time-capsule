using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARImageSpawner : MonoBehaviour
{
    private const string ScanPrompt = "Point your camera at the QR code to place the hub.";
    private const string PlacedPrompt = "Select a Minigame Portal or fill your Time Capsule!";
    [SerializeField]
    private GameObject placementPrefab;
    [SerializeField]
    private Vector3 rotationOffset = Vector3.zero;
    [SerializeField]
    private GameObject resetButton;


    private ARTrackedImageManager imageManager;
    private GameObject spawnedInstance;

    private void Awake()
    {
        imageManager = GetComponent<ARTrackedImageManager>();

        if (imageManager == null)
            Debug.LogError("[ARImageSpawner] No ARTrackedImageManager found on this GameObject. Add it to XR Origin.");
    }

    private void Start()
    {
        if (HUDMessageController.Instance != null)
            HUDMessageController.Instance.ResetRequested += Reset;

        // 이전에 허브 놓은 적 있으면 바로 스폰
        if (PlayerPrefs.GetInt("HubPlaced", 0) == 1)
        {
            Vector3 savedPos = new Vector3(
                PlayerPrefs.GetFloat("HubX"),
                PlayerPrefs.GetFloat("HubY"),
                PlayerPrefs.GetFloat("HubZ")
            );
            Quaternion savedRot = new Quaternion(
                PlayerPrefs.GetFloat("HubRotX"),
                PlayerPrefs.GetFloat("HubRotY"),
                PlayerPrefs.GetFloat("HubRotZ"),
                PlayerPrefs.GetFloat("HubRotW")
            );
            spawnedInstance = Instantiate(placementPrefab, savedPos, savedRot);
            HUDMessageController.Instance?.ShowMessage(PlacedPrompt);
            imageManager.enabled = false;
            HUDMessageController.Instance?.ShowResetButton(true);
        }
    }

    private void OnDestroy()
    {
        if (HUDMessageController.Instance != null)
            HUDMessageController.Instance.ResetRequested -= Reset;
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

        // ============================
        // Save Location
        PlayerPrefs.SetFloat("HubX", trackedImage.transform.position.x);
        PlayerPrefs.SetFloat("HubY", trackedImage.transform.position.y);
        PlayerPrefs.SetFloat("HubZ", trackedImage.transform.position.z);
        PlayerPrefs.SetFloat("HubRotX", spawnRotation.x);
        PlayerPrefs.SetFloat("HubRotY", spawnRotation.y);
        PlayerPrefs.SetFloat("HubRotZ", spawnRotation.z);
        PlayerPrefs.SetFloat("HubRotW", spawnRotation.w);
        PlayerPrefs.SetInt("HubPlaced", 1);
        PlayerPrefs.Save();
        // ============================

        HUDMessageController.Instance?.ShowMessage(PlacedPrompt);

        // Disable tracking — no longer needed after placement.
        imageManager.enabled = false;

        HUDMessageController.Instance?.ShowResetButton(true);
        return true;
    }

    public void Reset()
    {
        if (spawnedInstance != null)
        {
            Destroy(spawnedInstance);
            spawnedInstance = null;
        }

        // ============================
        // reset saved Location
        PlayerPrefs.DeleteKey("HubPlaced");
        PlayerPrefs.Save();
        // ============================

        HUDMessageController.Instance?.ShowResetButton(false);
        imageManager.enabled = true;
        HUDMessageController.Instance?.ShowMessage(ScanPrompt);
    }
}
