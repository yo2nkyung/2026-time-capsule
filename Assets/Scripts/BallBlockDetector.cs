using UnityEngine;

public class BallBlockDetector : MonoBehaviour
{
    public GoalkeeperMode goalkeeperMode;
    private bool resolved = false;

    private void OnTriggerEnter(Collider other)
    {
        if (resolved) return;
        if (other.CompareTag("Player"))
        {
            resolved = true;
            goalkeeperMode.RegisterSave();

            Rigidbody rb = GetComponent<Rigidbody>();

            Vector3 bounceDirection = transform.position - other.transform.position;
            bounceDirection.y += 0.5f;
            bounceDirection.Normalize();

            rb.linearVelocity = bounceDirection * 8f;
        }


        if (other.CompareTag("MissZone"))
        {
            resolved = true;

            goalkeeperMode.RegisterMiss();
        }
    }
}
