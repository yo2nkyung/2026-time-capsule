using UnityEngine;

public class TiltSteering : MonoBehaviour
{
    public float tiltSensitivity = 3f;
    public float maxOffset = 3f; // Maximum horizontal movement range
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float tilt = Input.acceleration.x;
        float newX = transform.position.x + tilt * tiltSensitivity * Time.deltaTime;
        newX = Mathf.Clamp(newX, -maxOffset, maxOffset); // Clamp the horizontal position
        transform.position = new Vector3(newX, transform.position.y, transform.position.z); // Update the position
    }
}
