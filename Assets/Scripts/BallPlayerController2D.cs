using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpringJoint2D))]
[RequireComponent(typeof(LineRenderer))]
public class BallPlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float rollTorque = 35f;
    public float airControlForce = 7f;
    public float maxHorizontalSpeed = 10f;
    public float maxAngularSpeed = 900f;

    [Header("Ground Check")]
    public float groundProbeRadius = 0.18f;
    public float groundProbeDepth = 0.08f;

    [Header("Hook")]
    public float maxHookDistance = 3.8f;
    public float retractSpeed = 6f;
    public float extendSpeed = 6f;
    public float minHookDistance = 1.1f;
    public float retractAssistForce = 5f;
    public LayerMask collisionMask = ~0;

    [Header("Braking")]
    public float groundLinearBrake = 10f;
    public float groundAngularBrake = 10f;
    public float stopVelocityThreshold = 0.15f;
    public float stopAngularThreshold = 10f;

    [Header("Audio")]
    public AudioSource rollAudioSource;
    public AudioSource sfxhookAudioSource;
    public AudioSource sfxdetachAudioSource;
    public float minRollSpeedForSound = 0.25f;
    [Header("VFX")]
    public GameObject hookAttachVfx;
    public GameObject hookDetachVfx;

    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private SpringJoint2D springJoint;
    private LineRenderer ropeRenderer;
    private Camera mainCamera;
    private readonly Collider2D[] groundHits = new Collider2D[8];

    private float moveInput;
    private bool retractHeld;
    private bool isGrounded;
    private bool isHooked;
    private bool extendHeld;
    private Vector2 anchorPoint;
    private float inputLockedUntil;
    private Vector3 respawnPosition;
    public bool IsGrounded => isGrounded;
    public bool IsHooked => isHooked;
    public Vector2 AnchorPoint => anchorPoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        springJoint = GetComponent<SpringJoint2D>();
        ropeRenderer = GetComponent<LineRenderer>();

        springJoint.enabled = false;
        springJoint.autoConfigureConnectedAnchor = false;
        springJoint.autoConfigureDistance = false;
        springJoint.frequency = 5.5f;
        springJoint.dampingRatio = 0.15f;
        springJoint.enableCollision = true;

        ropeRenderer.positionCount = 2;
        ropeRenderer.enabled = false;
        ropeRenderer.useWorldSpace = true;
        ropeRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ropeRenderer.receiveShadows = false;
        ropeRenderer.widthMultiplier = 0.08f;
        ropeRenderer.numCapVertices = 4;
        ropeRenderer.material = new Material(Shader.Find("Sprites/Default"));
        ropeRenderer.startColor = new Color(0.97f, 0.87f, 0.39f, 1f);
        ropeRenderer.endColor = new Color(0.97f, 0.87f, 0.39f, 1f);
        ropeRenderer.sortingOrder = 10;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        respawnPosition = transform.position;
    }

    private void Update()
{
    if (Time.unscaledTime < inputLockedUntil)
    {
        moveInput = 0f;
        retractHeld = false;
        extendHeld = false;
        return;
    }

    moveInput = Input.GetAxisRaw("Horizontal");

    retractHeld = Input.GetMouseButton(0);
    extendHeld = Input.GetMouseButton(1);

    if (Input.GetKeyDown(KeyCode.Space))
    {
        if (isHooked)
        {
            DetachHook();
        }
        else
        {
            TryAttachHook();
        }
    }

    UpdateRopeVisual();
    UpdateRollingAudio();
    }
    public void LockInputForSeconds(float seconds)
    {
        inputLockedUntil = Time.unscaledTime + seconds;
    }
    private void FixedUpdate()
    {
        if (Time.unscaledTime < inputLockedUntil)
        {
            return;
        }

        isGrounded = CheckGrounded();

        float groundedTorqueScale = isGrounded ? 1f : 0.2f;
        rb.AddTorque(-moveInput * rollTorque * groundedTorqueScale, ForceMode2D.Force);

        if (!isGrounded)
        {
            rb.AddForce(Vector2.right * moveInput * airControlForce, ForceMode2D.Force);
        }

        Vector2 velocity = rb.velocity;
        if (!isHooked)
        {
            velocity.x = Mathf.Clamp(velocity.x, -maxHorizontalSpeed, maxHorizontalSpeed);
        }
        rb.velocity = velocity;
        rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -maxAngularSpeed, maxAngularSpeed);

        if (isHooked)
        {
            if (retractHeld)
            {
                springJoint.distance = Mathf.Max(minHookDistance, springJoint.distance - retractSpeed * Time.fixedDeltaTime);

                Vector2 toAnchor = anchorPoint - rb.position;
                if (toAnchor.sqrMagnitude > 0.001f)
                {
                    rb.AddForce(toAnchor.normalized * retractAssistForce, ForceMode2D.Force);
                }
            }

            if (extendHeld)
            {
                springJoint.distance = Mathf.Min(maxHookDistance, springJoint.distance + extendSpeed * Time.fixedDeltaTime);
            }
        }
        if (isGrounded && !isHooked && Mathf.Abs(moveInput) < 0.01f)
        {
            Vector2 bvelocity = rb.velocity;
            bvelocity.x = Mathf.MoveTowards(bvelocity.x, 0f, groundLinearBrake * Time.fixedDeltaTime);
            rb.velocity = bvelocity;

            rb.angularVelocity = Mathf.MoveTowards(rb.angularVelocity, 0f, groundAngularBrake * Time.fixedDeltaTime);

            if (Mathf.Abs(rb.velocity.x) < stopVelocityThreshold)
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }

            if (Mathf.Abs(rb.angularVelocity) < stopAngularThreshold)
            {
                rb.angularVelocity = 0f;
            }
        }
    }

    private bool CheckGrounded()
    {
        float radius = circleCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
        Vector2 probeCenter = rb.position + Vector2.down * (radius + groundProbeDepth);
        int hitCount = Physics2D.OverlapCircleNonAlloc(probeCenter, groundProbeRadius, groundHits, collisionMask);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = groundHits[i];
            if (hit == null || hit == circleCollider || hit.isTrigger)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private void TryAttachHook()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("No MainCamera found.");
                return;
            }
        }

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 origin = rb.position;
        Vector2 direction = (Vector2)(mouseWorld - (Vector3)rb.position);

        if (direction.sqrMagnitude < 0.001f)
        {
            Debug.Log("Hook failed: direction too small.");
            return;
        }

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction.normalized, maxHookDistance, collisionMask);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];

            if (hit.collider == null)
                continue;

            if (hit.collider == circleCollider)
                continue;

            if (hit.collider.isTrigger)
                continue;

            HookableSurface2D hookable = hit.collider.GetComponentInParent<HookableSurface2D>();
            if (hookable == null)
            {
                Debug.Log("Hook hit something not hookable: " + hit.collider.name);
                continue;
            }

            anchorPoint = hit.point;
            springJoint.connectedBody = null;
            springJoint.connectedAnchor = anchorPoint;
            springJoint.distance = Vector2.Distance(origin, anchorPoint);
            springJoint.enabled = true;
            isHooked = true;
            ropeRenderer.enabled = true;

            Debug.Log("Hook attached to: " + hit.collider.name);
            if (sfxhookAudioSource != null)
            {
                sfxhookAudioSource.Play();
            }
            if (hookAttachVfx != null)
            {
                Instantiate(hookAttachVfx, anchorPoint, Quaternion.identity);
            }
            return;
        }
        Debug.Log("Hook failed: no valid hookable surface found.");

    }

    private void DetachHook()
    {
        if (!isHooked)
            return;
        isHooked = false;
        springJoint.enabled = false;
        ropeRenderer.enabled = false;
        if (sfxdetachAudioSource != null)
        {
            sfxdetachAudioSource.Play();
        }
        if (hookDetachVfx != null)
        {
            Instantiate(hookDetachVfx, anchorPoint, Quaternion.identity);
        }
        return;
    }

    private void UpdateRopeVisual()
    {
        if (!isHooked)
        {
            return;
        }

        ropeRenderer.SetPosition(0, transform.position);
        ropeRenderer.SetPosition(1, anchorPoint);
    }

    private void OnDisable()
    {
        if (ropeRenderer != null)
        {
            ropeRenderer.enabled = false;
        }

        if (springJoint != null)
        {
            springJoint.enabled = false;
        }

        isHooked = false;
    }
    public void RespawnAtBottom()
    {
        DetachHook();

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        transform.position = respawnPosition;
        rb.position = respawnPosition;

        LockInputForSeconds(0.15f);
    }

    private void UpdateRollingAudio()
    {
        if (rollAudioSource == null )
            return;

        float speed = Mathf.Abs(rb.velocity.x);

        if (isGrounded && speed > minRollSpeedForSound)
        {
            if (!rollAudioSource.isPlaying)
            {
                rollAudioSource.Play();
            }
        }
        else
        {
            if (rollAudioSource.isPlaying)
            {
                rollAudioSource.Stop();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.92f, 0.3f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, maxHookDistance);
    }
}