using UnityEngine;

public class SoccerKick : MonoBehaviour
{

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip kickSound;
    
    
    
    
    [Header("Settings")]
    public float kickRange = 1.5f;        // If the ball comes within this distance, kick it
    public float kickForce = 15f;
    public float upForce = 5f;
    public LayerMask ballLayer;


    private bool hasKicked = false;       //Prevent duplicate kicks
    private GameObject currentBall;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void Update()
    {
        CheckKickRange();
    }

    void CheckKickRange()
    {
        // Browse all soccer balls
        Collider[] hits = Physics.OverlapSphere(
            Camera.main.transform.position, kickRange, ballLayer);

        if (hits.Length > 0)
        {
            currentBall = hits[0].gameObject; //  nearest ball

            if (!hasKicked)
            {
                Kick(currentBall);
                hasKicked = true;
            }
        }
        else
        {
            // If you go out of bounds, the game resets -> so you can take the next shot
            hasKicked = false;
            currentBall = null;
        }
    }

    void Kick(GameObject ball)
    {
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.isKinematic = false;

        // shoot in the direction of the camera
        Vector3 kickDir = Camera.main.transform.forward;
        rb.AddForce(kickDir * kickForce + Vector3.up * upForce, ForceMode.Impulse);

        if (kickSound != null) audioSource.PlayOneShot(kickSound);

        Debug.Log($"[SoccerKick] Kicked: {ball.name}");

        // Ball reset in 2 seconds
        StartCoroutine(ResetBall(ball, ball.transform.position));
    }

    System.Collections.IEnumerator ResetBall(GameObject ball, Vector3 startPos)
    {
        yield return new WaitForSeconds(2f);
        
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        ball.transform.position = startPos;

        Debug.Log($"[SoccerKick] Reset: {ball.name}");
    }
}