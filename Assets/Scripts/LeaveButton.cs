using UnityEngine;
using UnityEngine.SceneManagement;

public class LeaveButton : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The scene name of this minigame. Must match the linkedMinigameScene on the corresponding MemoryObjectTap.")]
    private string _minigameScene;

    private void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);
            RectTransform rect = GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, touch.position))
            {
                Debug.Log("Leave button tapped, returning to main menu...");
                MemoryObjectData.MarkRestored(_minigameScene);
                SceneManager.LoadScene("ARScene");
            }
        }
    }
}
