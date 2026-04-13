using UnityEngine;

public class TrackMover : MonoBehaviour
{

    public float speed = 5f;
    public float trackLength = 20f;
    private float startZ;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startZ = transform.position.z; // 초기 위치 저장
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(0, 0, -speed * Time.deltaTime);

        if (transform.position.z < startZ - trackLength)
        {
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y,
                startZ + trackLength);  // 정확히 한 칸 앞으로
        }
    }
}
