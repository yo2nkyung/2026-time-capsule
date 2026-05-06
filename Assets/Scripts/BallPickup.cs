using UnityEngine;
using System.Collections.Generic; 

public class BallPickup : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    // public AudioClip pickupSound;
    public AudioClip throwSound;


    [Header("Settings")]
    public float pickupRange = 3f;
    public float throwForceMultiplier = 3f;
    public float upForce = 4f;
    public LayerMask footballLayer;

    private GameObject heldBall = null;
    private Transform holdPoint;

    // for swipe detection
    private Vector2 touchStart;
    private bool isSwiping = false;




    private Dictionary<GameObject, Vector3> ballStartPositions = new Dictionary<GameObject, Vector3>();
    public float outOfBoundsY = -2f; // If ball fall below this height, it will respawn
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        holdPoint = new GameObject("HoldPoint").transform;
        holdPoint.SetParent(Camera.main.transform);
        holdPoint.localPosition = new Vector3(0, -0.2f, 1.5f);

        GameObject[] balls = GameObject.FindGameObjectsWithTag("Football");
        foreach (GameObject ball in balls)
        {
            ballStartPositions[ball] = ball.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (heldBall != null)
        {
            heldBall.transform.position = holdPoint.position;
            heldBall.transform.rotation = holdPoint.rotation;
        }

        HandleTouch();
        CheckBallsOutOfBounds();
    }

    void HandleTouch()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                touchStart = touch.position;
                isSwiping = false;
                break;

            case TouchPhase.Moved:
                // for swipe detection (upward 50px or more)
                if (touch.position.y - touchStart.y > 50f)
                    isSwiping = true;
                break;

            case TouchPhase.Ended:
                if (isSwiping && heldBall != null)
                {
                    // for swipe -> throw
                    float swipeDistance = touch.position.y - touchStart.y;
                    ThrowBall(swipeDistance);
                }
                else
                {
                    // for tap -> pickup or drop
                    if (heldBall == null)
                        TryPickup();
                    else
                        DropBall();
                }
                break;
        }
    }

    void TryPickup()
    {
        Ray ray = Camera.main.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, footballLayer))
        {
            if (hit.collider.CompareTag("Superbowl"))
            {
                heldBall = hit.collider.gameObject;
                heldBall.GetComponent<Rigidbody>().isKinematic = true;
                Debug.Log($"[BallPickup] Picked up: {heldBall.name}");
            }
        }
    }

    void ThrowBall(float swipeDistance)
    {
        if (heldBall == null) return;

        // Play the sound of a throw
        if (throwSound != null) audioSource.PlayOneShot(throwSound);

        Rigidbody rb = heldBall.GetComponent<Rigidbody>();
        rb.isKinematic = false;

        float force = Mathf.Clamp(swipeDistance * throwForceMultiplier, 5f, 30f);
        Vector3 throwDir = Camera.main.transform.forward;
        rb.AddForce(throwDir * force + Vector3.up * upForce, ForceMode.Impulse);

        Debug.Log($"[BallPickup] Threw with force: {force}");
        heldBall = null;
    }

    void DropBall()
    {
        if (heldBall == null) return;
        heldBall.GetComponent<Rigidbody>().isKinematic = false;
        Debug.Log($"[BallPickup] Dropped: {heldBall.name}");
        heldBall = null;
    }

    void CheckBallsOutOfBounds()
    {
        foreach (var entry in ballStartPositions)
        {
            GameObject ball = entry.Key;
            if (ball == null) continue;

            // The ball you are holding is excluded from the count
            if (ball == heldBall) continue;

            // Y-axis below the bounds or too far away
            if (ball.transform.position.y < outOfBoundsY)
            {
                ResetBall(ball);
            }
        }

        
    }

    void ResetBall(GameObject ball)
    {
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        ball.transform.position = ballStartPositions[ball];
        Debug.Log($"[BallPickup] Reset: {ball.name}");
    }
}
