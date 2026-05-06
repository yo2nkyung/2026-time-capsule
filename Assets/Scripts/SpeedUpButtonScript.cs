using UnityEngine;

public class SpeedUpButtonScript : MonoBehaviour
{
    private SpeedController speedController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        speedController = FindAnyObjectByType<SpeedController>();
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
                Debug.Log("Speed Up button tapped");
                if (speedController != null)
                    speedController.SpeedUp();
            }
        }
    }
}
