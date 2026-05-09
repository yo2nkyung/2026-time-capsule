using UnityEngine;

public class ReticleRaycast : MonoBehaviour
{
    public float maxDistance = 10f;
    public LayerMask interactableLayer;
    public Outline currentOutline;
    GameObject currentHitObject;
    Vector3 currentHitPoint;
    GameObject lastDestroyedObject = null;

    [SerializeField] private Transform player;
    [SerializeField] private float playerRadius = 0.3f;
    [SerializeField] private float playerHeight = 1.6f;
    [SerializeField] private LayerMask blockingMask;

    private Rigidbody movingCube1 = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        bool isHit = Physics.Raycast(
            transform.position,
            transform.forward,
            out hit,
            maxDistance,
            interactableLayer
        );

        Outline newOutline = null;



        if (isHit)
        {
            newOutline = hit.collider.GetComponentInParent<Outline>();
            currentHitObject = hit.collider.gameObject;
            currentHitPoint = hit.point;
        }
        else
        {
            currentHitObject = null;
        }

        if (newOutline != currentOutline)
        {
            if (currentOutline != null) currentOutline.enabled = false;
            currentOutline = newOutline;
            if (currentOutline != null) currentOutline.enabled = true;
        }

        // button X <- hold
        if (Input.GetKey(KeyCode.JoystickButton2))
        {
            if (currentHitObject != null)
            {
                if (currentHitObject.name == "Cube1")
                {
                    // move in one direction
                    // currentHitObject.transform.Translate(Vector3.right * 7f * Time.deltaTime, Space.World);
                    Rigidbody rigidbody = currentHitObject.GetComponent<Rigidbody>(); if (rigidbody != null)
                    {
                        movingCube1 = rigidbody;
                        rigidbody.linearVelocity = new Vector3(2f, rigidbody.linearVelocity.y, 0f);
                    }

                }
                if (currentHitObject.name == "Cube2")
                {
                    // rotate in any one direction
                    currentHitObject.transform.Rotate(Vector3.up * 45f * Time.deltaTime);
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.JoystickButton2))
        {
            if (movingCube1 != null) {
                movingCube1.linearVelocity = Vector3.zero;
                movingCube1.angularVelocity = Vector3.zero;
                movingCube1 = null;
            }
        }
        if (!Input.GetKey(KeyCode.JoystickButton2))
        {
            if (currentHitObject != null && currentHitObject.name == "Cube1")
            {
                Rigidbody rigidbody = currentHitObject.GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    rigidbody.linearVelocity = Vector3.zero;
                }
            }

        }

        // button Y
        if (Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            if (currentHitObject != null)
            {
                // destroy and spawn
                // If the hit is a Cube/Sphere: lastDestroyedObject = that object, SetActive(false)
                if (currentHitObject.name.StartsWith("Cube") || currentHitObject.name.StartsWith("Sphere"))
                {
                    lastDestroyedObject = currentHitObject;
                    lastDestroyedObject.SetActive(false);
                }
                // pointing floor: spawn
                else if (currentHitObject.name == "Plane")
                {
                    if (lastDestroyedObject != null)
                    {
                        float offset = 1f;
                        Collider col = lastDestroyedObject.GetComponent<Collider>();
                        if (col != null)
                        {
                            offset = col.bounds.extents.y;
                        }
                        Vector3 spawnPosition = currentHitPoint + Vector3.up * offset;
                        // Vector3 spawnPosition = currentHitPoint;
                        spawnPosition.y = 0.5f;

                        lastDestroyedObject.transform.position = spawnPosition;
                        lastDestroyedObject.SetActive(true);

                        Rigidbody rigidbody = lastDestroyedObject.GetComponent<Rigidbody>();
                        if (rigidbody != null)
                        {
                            rigidbody.linearVelocity = Vector3.zero;
                            rigidbody.angularVelocity = Vector3.zero;
                            rigidbody.Sleep();
                        }

                        lastDestroyedObject = null;
                    }
                }
            }
        }
        // Button A
        if (Input.GetKeyDown(KeyCode.JoystickButton10))
        {
            if (currentHitObject != null && currentHitObject.name == "Plane")
            {
                Vector3 targetPosition = currentHitPoint;
                float offset = 0f;
                targetPosition += Vector3.up * offset;

                Vector3 p1 = targetPosition + Vector3.up * playerRadius;
                Vector3 p2 = targetPosition + Vector3.up * (playerHeight - playerRadius);

                bool blocked = Physics.CheckCapsule(p1, p2, playerRadius, blockingMask);

                if (!blocked)
                {
                    player.position = targetPosition;
                }
            }
        }



        // button B


    }
}
