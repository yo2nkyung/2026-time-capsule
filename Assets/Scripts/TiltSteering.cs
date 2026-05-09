using UnityEngine;

public class TiltSteering : MonoBehaviour
{
    public float tiltSensitivity = 3f;
    public float maxOffset = 3f; // Maximum horizontal movement range

    private Vector3 _originPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _originPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float tilt = Input.acceleration.x;
        float newX = _originPosition.x + tilt * tiltSensitivity * Time.deltaTime * 60f; // Calculate new horizontal position based on tilt
        newX = Mathf.Clamp(newX, _originPosition.x - maxOffset, _originPosition.x + maxOffset); // Clamp the horizontal position
        transform.position = new Vector3(newX, transform.position.y, transform.position.z); // Update the position
    }
}
