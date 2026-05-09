using System.Collections.Generic;
using UnityEngine;

// Ball that can be picked up, previewed, and flick-thrown.
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BallThrowable : MonoBehaviour
{
    [Tooltip("Distance in front of the camera the ball hovers while held.")]
    public float holdDistance = 0.5f;

    [Tooltip("How fast the ball lerps to the hold position each frame.")]
    public float holdLerpSpeed = 12f;

    [Tooltip("Multiplier applied to the raw swipe velocity to produce throw force.")]
    public float flickForceMultiplier = 0.035f;

    [Tooltip("Minimum throw force so a slow release still sends the ball.")]
    public float minThrowForce = 2f;

    [Tooltip("Maximum throw force cap.")]
    public float maxThrowForce = 20f;

    [Tooltip("Number of points in the trajectory arc preview.")]
    public int trajectoryResolution = 30;

    [Tooltip("Time step between each trajectory preview point (seconds).")]
    public float trajectoryTimeStep = 0.05f;

    [Tooltip("World-space Y value below which the ball is considered lost and will respawn.")]
    public float fallThreshold = -5f;

    public bool IsHeld => _isHeld;

    private const float PickUpMessageDuration = 2f;

    private bool _isHeld = false;
    private Rigidbody _rb;
    private Camera _arCamera;
    private LineRenderer _lineRenderer;

    // home spot in the cabinet for respawning
    private Transform _cabinetParent;
    private Vector3 _cabinetLocalPosition;
    private Quaternion _cabinetLocalRotation;
    private bool _initialIsKinematic;
    private Vector3 _pendingThrowVelocity;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _cabinetParent = transform.parent;
        _cabinetLocalPosition = transform.localPosition;
        _cabinetLocalRotation = transform.localRotation;
        _initialIsKinematic = _rb.isKinematic;
        BuildLineRenderer();
    }

    private void Start()
    {
        _arCamera = Camera.main;
    }

    private void Update()
    {
        if (_arCamera == null)
            _arCamera = Camera.main;

        if (_isHeld)
        {
            MoveToHoldPosition();
            DrawTrajectory(_pendingThrowVelocity);
        }
        else if (transform.position.y < fallThreshold)
        {
            Respawn();
        }
    }

    // pick up the ball and show the trajectory preview
    public void PickUp()
    {
        if (_isHeld)
            return;

        _isHeld = true;
        transform.SetParent(null);
        _rb.isKinematic = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _pendingThrowVelocity = _arCamera != null
            ? _arCamera.transform.forward * minThrowForce
            : Vector3.forward * minThrowForce;
        _lineRenderer.enabled = true;

        HUDMessageController.Instance?.ShowMessage($"{gameObject.name} has been picked up", PickUpMessageDuration);
    }

    // update the live arc while the user drags
    public void UpdateFlickPreview(Vector3 worldThrowVelocity)
    {
        _pendingThrowVelocity = worldThrowVelocity;
    }

    // release the ball with the computed throw velocity
    public void FlickThrow(Vector3 worldThrowVelocity)
    {
        if (!_isHeld)
            return;

        _isHeld = false;
        _lineRenderer.enabled = false;
        _rb.isKinematic = false;
        _rb.linearVelocity = worldThrowVelocity;
    }

    // return the ball to its cabinet home
    private void Respawn()
    {
        _isHeld = false;
        _lineRenderer.enabled = false;
        _rb.isKinematic = _initialIsKinematic;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.SetParent(_cabinetParent);
        transform.localPosition = _cabinetLocalPosition;
        transform.localRotation = _cabinetLocalRotation;
    }

    private void MoveToHoldPosition()
    {
        Vector3 target = _arCamera.transform.position + _arCamera.transform.forward * holdDistance;
        transform.position = Vector3.Lerp(transform.position, target, holdLerpSpeed * Time.deltaTime);
    }

    // draw a simple projectile arc in front of the ball
    private void DrawTrajectory(Vector3 startVelocity)
    {
        Vector3 startPos = transform.position;
        Vector3 gravity = Physics.gravity;
        var points = new List<Vector3>(trajectoryResolution);

        for (int i = 0; i < trajectoryResolution; i++)
        {
            float t = i * trajectoryTimeStep;
            points.Add(startPos + startVelocity * t + 0.5f * gravity * t * t);
        }

        _lineRenderer.positionCount = points.Count;
        _lineRenderer.SetPositions(points.ToArray());
    }

    private void BuildLineRenderer()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.startWidth = 0.015f;
        _lineRenderer.endWidth = 0.005f;
        _lineRenderer.numCapVertices = 4;
        _lineRenderer.numCornerVertices = 4;

        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );

        _lineRenderer.colorGradient = gradient;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.enabled = false;
    }
}
