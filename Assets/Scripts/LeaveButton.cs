using UnityEngine;
using UnityEngine.SceneManagement;


public class LeaveButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);
            RectTransform rect = GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, touch.position))
            {
                Debug.Log("Leave button tapped, returning to main menu...");
                SceneManager.LoadScene("ARScene"); // Load the main menu scene (index 0)
            }
        }
    }
}
