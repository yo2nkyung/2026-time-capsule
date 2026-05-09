using UnityEngine;
using UnityEngine.SceneManagement;

// Simple portal entrance that loads the target scene when tapped.
public class PortalEntrance : MonoBehaviour
{
    [Tooltip("Exact name of the scene to load. Must be added to Build Settings.")]
    public string targetScene = "soccer";

    private bool _transitioning = false;

    // called by BallInputHandler when the portal collider is tapped
    public void Enter()
    {
        if (_transitioning)
            return;

        _transitioning = true;
        HUDMessageController.Instance?.OnEnterMinigame();
        SceneManager.LoadScene(targetScene);
    }
}
