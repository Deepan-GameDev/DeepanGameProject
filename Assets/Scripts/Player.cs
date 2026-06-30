using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(AudioSource))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float crouchSpeed = 2f;

    [Header("Body")]
    public float standingHeight = 1.85f;
    public float crouchingHeight = 1.15f;
    public float bodyRadius = 0.3f;
    public float crouchSmoothSpeed = 10f;
    public Transform cameraTransform;
    public float standingCameraHeight = 1.6f;
    public float crouchingCameraHeight = 0.95f;
    public LayerMask groundLayers = ~0;
    public float moveInputDeadZone = 0.08f;
    public float collisionSkinWidth = 0.03f;
    public float stepHeight = 0.45f;
    public float groundSnapDistance = 0.25f;

    [Header("Footsteps")]
    public AudioClip footstepClip;
    public float walkingStepInterval = 0.52f;
    public float runningStepInterval = 0.32f;
    public float footstepVolume = 0.55f;
    public Vector2 footstepPitchRange = new Vector2(0.92f, 1.08f);

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private AudioSource footstepSource;
    private AudioClip generatedFootstepClip;
    private float footstepTimer;
    private float pendingYaw;
    private bool isCrouching;
    private bool runPressed;
    private bool crouchPressed;
    private Vector2 moveInput;
    private readonly Collider[] standCheckHits = new Collider[8];
    private readonly RaycastHit[] movementHits = new RaycastHit[8];
    private readonly Collider[] overlapHits = new Collider[8];

    public void AddYawInput(float yawDegrees)
    {
        pendingYaw += yawDegrees;
    }

  public void SetMoveInput(Vector2 input)
{
    moveInput = input.sqrMagnitude < moveInputDeadZone * moveInputDeadZone
        ? Vector2.zero
        : Vector2.ClampMagnitude(input, 1f);

}

public void ToggleRun()
{
    runPressed = !runPressed;
}

public void SetCrouch(bool value)
{
    crouchPressed = value;
}

    

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        footstepSource = GetComponent<AudioSource>();
        if (footstepSource == null)
        {
            footstepSource = gameObject.AddComponent<AudioSource>();
        }

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        capsule.radius = bodyRadius;
        capsule.height = standingHeight;
        capsule.center = Vector3.up * (standingHeight * 0.5f);

        footstepSource.playOnAwake = false;
        footstepSource.loop = false;
        footstepSource.spatialBlend = 0f;
        footstepSource.volume = footstepVolume;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (footstepClip == null)
        {
            generatedFootstepClip = CreateFootstepClip();
        }
    }

  void Update()
{
    isCrouching = crouchPressed || !CanStandUp();

    UpdateCrouch();

    UpdateFootsteps();
}    
  void FixedUpdate()
    {
        Quaternion targetRotation = rb.rotation;
        if (Mathf.Abs(pendingYaw) > 0.001f)
        {
            targetRotation *= Quaternion.Euler(0f, pendingYaw, 0f);
            rb.MoveRotation(targetRotation);
            pendingYaw = 0f;
        }

        float speed = GetCurrentSpeed();
        Vector3 localMove = new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 worldMove = targetRotation * localMove * speed * Time.fixedDeltaTime;
        MoveWithCollision(worldMove);
    }

    private void MoveWithCollision(Vector3 movement)
    {
        Vector3 remaining = movement;
        Vector3 position = rb.position;

        for (int i = 0; i < 3; i++)
        {
            float distance = remaining.magnitude;
            if (distance <= 0.0001f)
            {
                break;
            }

            Vector3 direction = remaining / distance;
            if (!CastPlayer(position, direction, distance + collisionSkinWidth, out RaycastHit hit))
            {
                position += remaining;
                break;
            }

            if (TryStep(position, direction, distance, out Vector3 steppedPosition))
            {
                position = steppedPosition;
                remaining = movement - (position - rb.position);
                remaining.y = 0f;
                continue;
            }

            float travelDistance = Mathf.Max(0f, hit.distance - collisionSkinWidth);
            position += direction * travelDistance;

            Vector3 blockedMovement = remaining - direction * travelDistance;
            remaining = Vector3.ProjectOnPlane(blockedMovement, hit.normal);
            remaining.y = 0f;
        }

        rb.MovePosition(position);
    }

    private bool TryStep(Vector3 position, Vector3 direction, float distance, out Vector3 steppedPosition)
    {
        steppedPosition = position;

        Vector3 raisedPosition = position + Vector3.up * stepHeight;
        if (!IsCapsuleClear(raisedPosition))
        {
            return false;
        }

        if (CastPlayer(raisedPosition, direction, distance + collisionSkinWidth, out RaycastHit raisedHit))
        {
            distance = Mathf.Max(0f, raisedHit.distance - collisionSkinWidth);
            if (distance <= 0.0001f)
            {
                return false;
            }
        }

        Vector3 forwardPosition = raisedPosition + direction * distance;
        if (!CastPlayer(forwardPosition, Vector3.down, stepHeight + groundSnapDistance, out RaycastHit groundHit))
        {
            return false;
        }

        float groundAngle = Vector3.Angle(groundHit.normal, Vector3.up);
        if (groundAngle > 55f)
        {
            return false;
        }

        steppedPosition = forwardPosition + Vector3.down * Mathf.Max(0f, groundHit.distance - collisionSkinWidth);
        return IsCapsuleClear(steppedPosition);
    }

    private bool CastPlayer(Vector3 position, Vector3 direction, float distance, out RaycastHit bestHit)
    {
        GetCapsulePoints(position, out Vector3 bottom, out Vector3 top, out float radius);
        int hitCount = Physics.CapsuleCastNonAlloc(
            bottom,
            top,
            radius,
            direction,
            movementHits,
            distance,
            groundLayers,
            QueryTriggerInteraction.Ignore);

        bestHit = default;
        float closestDistance = float.PositiveInfinity;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = movementHits[i];
            if (hit.collider == null || hit.collider == capsule)
            {
                continue;
            }

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                bestHit = hit;
            }
        }

        return closestDistance < float.PositiveInfinity;
    }

    private bool IsCapsuleClear(Vector3 position)
    {
        GetCapsulePoints(position, out Vector3 bottom, out Vector3 top, out float radius);
        int hitCount = Physics.OverlapCapsuleNonAlloc(
            bottom,
            top,
            radius,
            overlapHits,
            groundLayers,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = overlapHits[i];
            if (hit != null && hit != capsule)
            {
                return false;
            }
        }

        return true;
    }

    private void GetCapsulePoints(Vector3 position, out Vector3 bottom, out Vector3 top, out float radius)
    {
        radius = Mathf.Max(0.01f, capsule.radius - collisionSkinWidth);
        float halfHeight = Mathf.Max(capsule.height * 0.5f, radius);
        Vector3 center = position + transform.rotation * capsule.center;
        float pointOffset = halfHeight - radius;
        bottom = center + Vector3.down * pointOffset;
        top = center + Vector3.up * pointOffset;
    }

    private float GetCurrentSpeed()
    {
        if (isCrouching)
        {
            return crouchSpeed;
        }

        return IsRunning() ? runSpeed : walkSpeed;
    }

    private bool IsMoving()
    {
        return moveInput.sqrMagnitude > 0.01f;
    }

    private bool IsRunning()
    {
        return runPressed && IsMoving() && !isCrouching;
    }

    private bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.15f;
        float distance = Mathf.Max(0.35f, bodyRadius + 0.15f);
        return Physics.Raycast(origin, Vector3.down, distance, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private bool CanStandUp()
    {
        if (!isCrouching)
        {
            return true;
        }

        float radius = bodyRadius * 0.95f;
        Vector3 bottom = transform.position + Vector3.up * radius;
        Vector3 top = transform.position + Vector3.up * (standingHeight - radius);
        int hitCount = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, standCheckHits, groundLayers, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = standCheckHits[i];
            if (hit != null && hit != capsule)
            {
                return false;
            }
        }

        return true;
    }

    private void UpdateCrouch()
    {
        float targetHeight = isCrouching ? crouchingHeight : standingHeight;
        capsule.height = Mathf.Lerp(capsule.height, targetHeight, crouchSmoothSpeed * Time.deltaTime);
        capsule.center = Vector3.up * (capsule.height * 0.5f);

        if (cameraTransform != null && cameraTransform.IsChildOf(transform))
        {
            float targetCameraHeight = isCrouching ? crouchingCameraHeight : standingCameraHeight;
            Vector3 localPosition = cameraTransform.localPosition;
            localPosition.y = Mathf.Lerp(localPosition.y, targetCameraHeight, crouchSmoothSpeed * Time.deltaTime);
            cameraTransform.localPosition = localPosition;
        }
    }

    private void UpdateFootsteps()
    {
        if (!IsMoving() || isCrouching || !IsGrounded())
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer -= Time.deltaTime;
        if (footstepTimer > 0f)
        {
            return;
        }

        PlayFootstep();
        footstepTimer = IsRunning() ? runningStepInterval : walkingStepInterval;
    }

    private void PlayFootstep()
    {
        AudioClip clip = footstepClip != null ? footstepClip : generatedFootstepClip;
        if (clip == null)
        {
            return;
        }

        float pitch = Random.Range(footstepPitchRange.x, footstepPitchRange.y);
        footstepSource.pitch = IsRunning() ? pitch * 1.05f : pitch;
        footstepSource.PlayOneShot(clip, footstepVolume);
    }

    private AudioClip CreateFootstepClip()
    {
        const int sampleRate = 22050;
        const float length = 0.13f;
        int sampleCount = Mathf.RoundToInt(sampleRate * length);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float fade = Mathf.Exp(-t * 32f);
            float thump = Mathf.Sin(2f * Mathf.PI * 85f * t) * 0.8f;
            float tap = Mathf.Sin(2f * Mathf.PI * 210f * t) * 0.22f;
            float texture = Random.Range(-0.18f, 0.18f);
            samples[i] = (thump + tap + texture) * fade * 0.55f;
        }

        AudioClip clip = AudioClip.Create("Generated Footstep", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
