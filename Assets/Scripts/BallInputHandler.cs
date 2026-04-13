using UnityEngine;

// Handles touch or mouse flicks to throw BallThrowable objects.
public class BallInputHandler : MonoBehaviour
{
    [Tooltip("How much the screen swipe's forward component biases the throw away from the camera.")]
    public float forwardBias = 0.6f;

    private Camera _arCamera;
    private BallThrowable _heldBall;

    // swipe state
    private Vector2 _touchStartPos;
    private Vector2 _prevTouchPos;
    private float _touchStartTime;
    private int _activeFingerId = -1;

    private void Start()
    {
        _arCamera = Camera.main;
    }

    private void Update()
    {
        if (_arCamera == null)
            _arCamera = Camera.main;

#if UNITY_EDITOR
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    // touch input on device
    private void HandleTouchInput()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (_activeFingerId == -1)
                        TryBeginGrab(touch.fingerId, touch.position);
                    break;
                case TouchPhase.Moved:
                    if (touch.fingerId == _activeFingerId && _heldBall != null)
                        OnDrag(touch.position);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (touch.fingerId == _activeFingerId)
                        OnRelease(touch.position);
                    break;
            }
        }
    }

    // mouse input for Editor testing
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
            TryBeginGrab(-99, Input.mousePosition);

        if (Input.GetMouseButton(0) && _heldBall != null)
            OnDrag(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
            OnRelease(Input.mousePosition);
    }

    // start a grab if the ray hit a ball or another interactable object
    private void TryBeginGrab(int fingerId, Vector2 screenPos)
    {
        Ray ray = _arCamera.ScreenPointToRay(screenPos);
        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        BallThrowable ball = hit.collider.GetComponent<BallThrowable>();
        if (ball == null)
        {
            hit.collider.GetComponent<PortalEntrance>()?.Enter();
            TimeCapsuleController capsule =
                hit.collider.GetComponent<TimeCapsuleController>() ??
                hit.collider.GetComponentInParent<TimeCapsuleController>();
            capsule?.OnTapped();
            return;
        }

        _activeFingerId = fingerId;
        _touchStartPos = screenPos;
        _prevTouchPos = screenPos;
        _touchStartTime = Time.time;

        if (_heldBall != null && _heldBall != ball)
            ReleaseWithVelocity(_heldBall, Vector3.zero);

        _heldBall = ball;
        _heldBall.PickUp();
    }

    // update the preview arc while dragging
    private void OnDrag(Vector2 screenPos)
    {
        if (_heldBall == null)
            return;

        Vector3 throwVelocity = ComputeThrowVelocity(screenPos);
        _heldBall.UpdateFlickPreview(throwVelocity);
        _prevTouchPos = screenPos;
    }

    // release the ball when touch/mouse lifts
    private void OnRelease(Vector2 screenPos)
    {
        if (_heldBall != null)
        {
            Vector3 throwVelocity = ComputeThrowVelocity(screenPos);
            ReleaseWithVelocity(_heldBall, throwVelocity);
        }

        _heldBall = null;
        _activeFingerId = -1;
    }

    // map a screen swipe to a world-space throw velocity
    private Vector3 ComputeThrowVelocity(Vector2 currentScreenPos)
    {
        float elapsed = Mathf.Max(Time.time - _touchStartTime, 0.016f);
        Vector2 swipeDelta = currentScreenPos - _touchStartPos;
        float swipeSpeed = swipeDelta.magnitude / elapsed;

        Vector2 swipeDir = swipeDelta.magnitude > 0.01f
            ? swipeDelta.normalized
            : Vector2.up;

        Vector3 worldDir = (_arCamera.transform.right   * swipeDir.x
                          + _arCamera.transform.up      * swipeDir.y
                          + _arCamera.transform.forward * forwardBias).normalized;

        float force = swipeSpeed * _heldBall.flickForceMultiplier;
        force = Mathf.Clamp(force, _heldBall.minThrowForce, _heldBall.maxThrowForce);

        return worldDir * force;
    }

    private static void ReleaseWithVelocity(BallThrowable ball, Vector3 velocity)
    {
        ball.FlickThrow(velocity);
    }
}
