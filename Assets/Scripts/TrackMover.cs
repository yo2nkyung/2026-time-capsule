using UnityEngine;

public class TrackMover : MonoBehaviour
{
    public float speed = 5f;
    public float trackLength = 20f;
    public bool loopTrack = true;
    private float startZ;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startZ = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        transform.Translate(0, 0, -speed * Time.deltaTime);

        if (loopTrack && transform.position.z < startZ - trackLength)
        {
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y,
                startZ + trackLength); 
        }
    }
}
