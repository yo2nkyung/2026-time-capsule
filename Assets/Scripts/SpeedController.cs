
using UnityEngine;
using UnityEngine.UI;

public class SpeedController : MonoBehaviour
{
    public float minSpeed = 2f;
    public float maxSpeed = 15f;
    public float speedStep = 3f;
    public Text speedText; // UI Text to display current speed

    private TrackMover[] trackMovers;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        trackMovers = FindObjectsByType<TrackMover>(FindObjectsSortMode.None);
        UpdateText();
    }

    public void SpeedUp ()
    {
        foreach (var mover in trackMovers)
            mover.speed = Mathf.Clamp(mover.speed + speedStep, minSpeed, maxSpeed);
        UpdateText();
    }

    public void SpeedDown ()
    {
        foreach (var mover in trackMovers)
            mover.speed = Mathf.Clamp(mover.speed - speedStep, minSpeed, maxSpeed);
        UpdateText();
    }

    void UpdateText()
    {
        if (speedText != null && trackMovers.Length > 0)
            speedText.text = "Current Speed: " + (int)trackMovers[0].speed;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
