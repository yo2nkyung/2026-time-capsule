using UnityEngine;

// Attach to each memory object in ARScene.
// Handles tap input, shows the HUD message, marks the object lost, and disables it.
public class MemoryObjectTap : MonoBehaviour
{
    [Tooltip("The minigame scene name that restores this object (must match the PortalEntrance.targetScene value).")]
    public string linkedMinigameScene;

    private const string LostMemoryMessage =
        "Oh no! you lost your memory object, open portals to win it back!";

    private void OnEnable()
    {
        // If the object is already marked lost, keep it disabled.
        if (MemoryObjectData.IsLost(linkedMinigameScene))
            gameObject.SetActive(false);
    }

    // Called by BallInputHandler when this object's collider is tapped.
    public void OnTapped()
    {
        HUDMessageController.Instance?.ShowMessage(LostMemoryMessage);
        MemoryObjectData.MarkLost(linkedMinigameScene);
        gameObject.SetActive(false);
    }
}
