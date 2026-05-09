using UnityEngine;

// Keeps lost memory objects disabled when ARScene starts.
// Restored objects can be re-enabled by their minigame logic.
public class ARSceneMemoryRestorer : MonoBehaviour
{
    [System.Serializable]
    public struct MemoryObjectEntry
    {
        [Tooltip("Must match gameObject.name of the memory object in the scene (e.g. '2026 World Cup').")]
        public string objectName;

        [Tooltip("The minigame scene name linked to this object (e.g. 'soccer').")]
        public string linkedMinigameScene;

        [Tooltip("Direct scene reference to the memory object GameObject.")]
        public GameObject memoryObject;
    }

    [Tooltip("All three memory objects and their linked minigame scene names.")]
    public MemoryObjectEntry[] memoryObjects;

    private void Start()
    {
        if (MemoryObjectData.LostObjects.Count == 0)
            return;

        foreach (MemoryObjectEntry entry in memoryObjects)
        {
            if (entry.memoryObject == null)
                continue;

            if (MemoryObjectData.IsLost(entry.linkedMinigameScene))
                entry.memoryObject.SetActive(false);
        }
    }
}
