using UnityEngine;
using UnityEngine.SceneManagement;

// Simple AR portal that loads a scene when activated.
public class ARPortal : MonoBehaviour
{
    [Tooltip("The scene to load when this portal is tapped after placement.")]
    public string targetSceneName;

    private bool _transitioning = false;

    // Activate the portal once, then load the target scene.
    public void Activate()
    {
        if (_transitioning)
            return;

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[ARPortal] targetSceneName is not set.");
            return;
        }

        _transitioning = true;
        SceneManager.LoadScene(targetSceneName);
    }
}
