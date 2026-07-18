using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(AudioSource))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float crouchSpeed = 2f;

    [Header("Run Stamina")]
    public float maxRunStamina = 5f;
    public float runStaminaDrainRate = 1f;
    public float runStaminaRechargeRate = 1f;
    public float rechargeDelay = 1.5f;
    public Image runCooldownFill;

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
    public float groundSnapDistance = 0.35f;
    public float maxWalkableSlope = 55f;
    public float stepSearchDistance = 0.08f;

    [Header("Camera Collision")]
    public bool preventCameraClipping = true;
    public LayerMask cameraCollisionLayers = ~0;
    public float cameraCollisionRadius = 0.08f;
    public float cameraSkinWidth = 0.02f;
    public float safeNearClipPlane = 0.03f;

    [Header("Footsteps")]
    public AudioClip[] footstepClips;
    public float walkingStepInterval = 0.52f;
    public float runningStepInterval = 0.32f;
    public float footstepVolume = 0.55f;
    public Vector2 footstepPitchRange = new Vector2(0.92f, 1.08f);

    [Header("Torch")]
    public TorchSway torchSway;

    private const int CollisionIterations = 5;
    private const float Gravity = -28f;
    private const float TerminalFallSpeed = -35f;
    private const float HeightSnapEpsilon = 0.0005f;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private AudioSource footstepSource;
    private AudioClip generatedFootstepClip;
    private Camera playerCamera;

    private float footstepTimer;
    private float pendingYaw;
    private float verticalVelocity;
    private float currentCameraHeight;
    private Quaternion playerRotation;
    private Vector3 cameraBaseLocalPosition;
    private bool isCrouching;
    private bool runPressed;
    private bool crouchPressed;
    private bool groundedLastFixedUpdate;
    private Vector2 moveInput;
    private float currentRunStamina;
    private float rechargeTimer;
    private bool runExhausted;
    private bool inputLocked;
    private bool externalMovementActive;
    private bool hasExternalPose;
    private Vector3 externalMovePosition;
    private Quaternion externalMoveRotation;

    private readonly Collider[] standCheckHits = new Collider[8];
    private readonly RaycastHit[] movementHits = new RaycastHit[12];
    private readonly Collider[] overlapHits = new Collider[12];
    private readonly Collider[] recoveryHits = new Collider[12];
    private readonly RaycastHit[] groundHits = new RaycastHit[8];
    private readonly RaycastHit[] cameraCastHits = new RaycastHit[8];
    private readonly Collider[] cameraOverlapHits = new Collider[8];

    public void AddYawInput(float yawDegrees)
    {
        if (inputLocked)
        {
            return;
        }

        pendingYaw += yawDegrees;
    }

    public void SetMoveInput(Vector2 input)
    {
        if (inputLocked)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = input.sqrMagnitude < moveInputDeadZone * moveInputDeadZone
            ? Vector2.zero
            : Vector2.ClampMagnitude(input, 1f);
    }

    public void ToggleRun()
    {
        if (inputLocked)
        {
            return;
        }

        if (runExhausted || currentRunStamina <= 0f)
        {
            return;
        }

        runPressed = !runPressed;
    }

    public void SetCrouch(bool value)
    {
        if (inputLocked)
        {
            return;
        }

        crouchPressed = value;
    }

    private void SetInputLocked(bool locked)
    {
        inputLocked = locked;

        if (inputLocked)
        {
            moveInput = Vector2.zero;
            pendingYaw = 0f;
            runPressed = false;
            crouchPressed = false;
        }
    }

    public bool GetInputLocked()
    {
        return inputLocked;
    }

    public CapsuleCollider GetBodyCollider()
    {
        return capsule;
    }

    public void BeginExternalMovement()
    {
        externalMovementActive = true;
        hasExternalPose = true;
        externalMovePosition = rb.position;
        externalMoveRotation = playerRotation;

        SetInputLocked(true);
        isCrouching = false;
        verticalVelocity = 0f;

        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            rb.WakeUp();
        }
    }

    public void SetExternalMovementPose(Vector3 position, Quaternion rotation)
    {
        if (!externalMovementActive)
        {
            return;
        }

        playerRotation = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
        externalMovePosition = position;
        externalMoveRotation = playerRotation;
        hasExternalPose = true;
    }

    public void EndExternalMovement()
    {
        externalMovementActive = false;
        hasExternalPose = false;
        verticalVelocity = 0f;
        pendingYaw = 0f;
        moveInput = Vector2.zero;
        SetInputLocked(false);

        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            rb.WakeUp();
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        footstepSource = GetComponent<AudioSource>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = false;
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        rb.sleepThreshold = 0f;

        playerRotation = rb.rotation;

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

        if (cameraTransform != null)
        {
            cameraBaseLocalPosition = cameraTransform.localPosition;
            cameraBaseLocalPosition.y = standingCameraHeight;
            currentCameraHeight = standingCameraHeight;
            cameraTransform.localPosition = cameraBaseLocalPosition;
            playerCamera = cameraTransform.GetComponent<Camera>();

            if (playerCamera != null)
            {
                playerCamera.nearClipPlane = Mathf.Min(playerCamera.nearClipPlane, safeNearClipPlane);
            }
        }

        if (footstepClips == null || footstepClips.Length == 0)
        {
            generatedFootstepClip = CreateFootstepClip();
        }

        currentRunStamina = maxRunStamina;
        UpdateRunCooldownUI();
        GroundPlayerAtStartup();
    }

    private void GroundPlayerAtStartup()
    {
        Physics.SyncTransforms();

        Vector3 position = RecoverFromOverlaps(rb.position);
        float startupGroundDistance = Mathf.Max(groundSnapDistance, stepHeight, standingHeight);

        if (ProbeGround(position, startupGroundDistance, out _))
        {
            position = SnapToGround(position, startupGroundDistance, out bool snapped);

            if (snapped)
            {
                rb.position = position;
                transform.position = position;
                verticalVelocity = -2f;
                groundedLastFixedUpdate = true;
                Physics.SyncTransforms();
            }
        }
    }

    private void Update()
    {
        isCrouching = externalMovementActive ? false : crouchPressed || !CanStandUp();
        UpdateRunStamina();
        UpdateFootsteps();
    }

    private void FixedUpdate()
    {
        UpdateCrouchCollider(Time.fixedDeltaTime);

        if (externalMovementActive)
        {
            pendingYaw = 0f;
            verticalVelocity = 0f;
            groundedLastFixedUpdate = false;
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;

            if (hasExternalPose)
            {
                rb.MoveRotation(externalMoveRotation);
                rb.MovePosition(externalMovePosition);
            }
            else
            {
                rb.MoveRotation(playerRotation);
            }

            return;
        }

        if (Mathf.Abs(pendingYaw) > 0.001f)
        {
            playerRotation *= Quaternion.Euler(0f, pendingYaw, 0f);
            pendingYaw = 0f;
        }

        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        rb.MoveRotation(playerRotation);

        Vector3 position = RecoverFromOverlaps(rb.position);
        bool grounded = ProbeGround(position, groundSnapDistance + stepHeight, out RaycastHit groundHit);
        groundedLastFixedUpdate = grounded;

        float speed = GetCurrentSpeed();
        Vector3 localMove = new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 horizontalVelocity = playerRotation * localMove * speed;
        Vector3 horizontalMove = horizontalVelocity * Time.fixedDeltaTime;

        position = MoveHorizontal(position, horizontalMove, grounded);

        if (grounded && verticalVelocity <= 0f)
        {
            position = SnapToGround(position, Mathf.Max(groundSnapDistance, stepHeight), out bool snapped);
            verticalVelocity = snapped ? -2f : 0f;
        }
        else
        {
            verticalVelocity = Mathf.Max(TerminalFallSpeed, verticalVelocity + Gravity * Time.fixedDeltaTime);
            position = MoveVertical(position, verticalVelocity * Time.fixedDeltaTime, out bool hitGround);

            if (hitGround)
            {
                verticalVelocity = -2f;
                groundedLastFixedUpdate = true;
            }
        }

        rb.MovePosition(position);
    }

    private void LateUpdate()
    {
        UpdateCameraPosition(Time.deltaTime);
    }

    private Vector3 MoveHorizontal(Vector3 position, Vector3 movement, bool grounded)
    {
        if (movement.sqrMagnitude <= 0.0000001f)
        {
            return position;
        }

        Vector3 startPosition = position;
        Vector3 remaining = movement;

        for (int i = 0; i < CollisionIterations; i++)
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

            if (IsWalkable(hit.normal))
            {
                position += remaining;
                break;
            }

            if (grounded && TryStep(position, direction, distance, out Vector3 steppedPosition))
            {
                position = steppedPosition;
                remaining = movement - Flatten(position - startPosition);
                remaining.y = 0f;
                continue;
            }

            float travelDistance = Mathf.Max(0f, hit.distance - collisionSkinWidth);
            position += direction * travelDistance;

            Vector3 blockedMovement = remaining - direction * travelDistance;
            Vector3 slideNormal = Flatten(hit.normal);
            if (slideNormal.sqrMagnitude <= 0.0001f)
            {
                break;
            }

            remaining = Vector3.ProjectOnPlane(blockedMovement, slideNormal.normalized);
            remaining.y = 0f;
        }

        return position;
    }

    private Vector3 MoveVertical(Vector3 position, float verticalMove, out bool hitGround)
    {
        hitGround = false;
        if (Mathf.Abs(verticalMove) <= 0.0001f)
        {
            return position;
        }

        Vector3 direction = verticalMove > 0f ? Vector3.up : Vector3.down;
        float distance = Mathf.Abs(verticalMove);

        if (!CastPlayer(position, direction, distance + collisionSkinWidth, out RaycastHit hit))
        {
            return position + direction * distance;
        }

        float travelDistance = Mathf.Max(0f, hit.distance - collisionSkinWidth);
        if (direction == Vector3.down && IsWalkable(hit.normal))
        {
            hitGround = true;
        }

        return position + direction * travelDistance;
    }

    private bool TryStep(Vector3 position, Vector3 direction, float distance, out Vector3 steppedPosition)
    {
        steppedPosition = position;

        Vector3 horizontalDirection = Flatten(direction);
        if (horizontalDirection.sqrMagnitude <= 0.0001f)
        {
            return false;
        }

        horizontalDirection.Normalize();
        Vector3 raisedPosition = position + Vector3.up * stepHeight;

        if (!IsCapsuleClear(raisedPosition))
        {
            return false;
        }

        float stepForwardDistance = distance + stepSearchDistance;
        if (CastPlayer(raisedPosition, horizontalDirection, stepForwardDistance + collisionSkinWidth, out RaycastHit raisedHit))
        {
            if (!IsWalkable(raisedHit.normal))
            {
                stepForwardDistance = Mathf.Max(0f, raisedHit.distance - collisionSkinWidth);
            }
        }

        if (stepForwardDistance <= 0.0001f)
        {
            return false;
        }

        Vector3 forwardPosition = raisedPosition + horizontalDirection * stepForwardDistance;
        if (!CastPlayer(forwardPosition, Vector3.down, stepHeight + groundSnapDistance + collisionSkinWidth, out RaycastHit groundHit))
        {
            return false;
        }

        if (!IsWalkable(groundHit.normal))
        {
            return false;
        }

        float stepUpAmount = stepHeight - Mathf.Max(0f, groundHit.distance - collisionSkinWidth);
        if (stepUpAmount < 0.015f || stepUpAmount > stepHeight + 0.001f)
        {
            return false;
        }

        steppedPosition = forwardPosition + Vector3.down * Mathf.Max(0f, groundHit.distance - collisionSkinWidth);
        return IsCapsuleClear(steppedPosition);
    }

    private Vector3 SnapToGround(Vector3 position, float snapDistance, out bool snapped)
    {
        snapped = false;
        Vector3 castPosition = position + Vector3.up * collisionSkinWidth;

        if (!CastPlayer(castPosition, Vector3.down, snapDistance + collisionSkinWidth, out RaycastHit groundHit))
        {
            return position;
        }

        if (!IsWalkable(groundHit.normal))
        {
            return position;
        }

        float downDistance = Mathf.Max(0f, groundHit.distance - collisionSkinWidth);
        Vector3 targetPosition = position + Vector3.down * downDistance;

        if (!IsCapsuleClear(targetPosition))
        {
            return position;
        }

        snapped = true;
        return targetPosition;
    }

    private bool ProbeGround(Vector3 position, float distance, out RaycastHit bestHit)
    {
        GetCapsulePoints(position + Vector3.up * collisionSkinWidth, out Vector3 bottom, out Vector3 top, out float radius);
        int hitCount = Physics.CapsuleCastNonAlloc(
            bottom,
            top,
            radius,
            Vector3.down,
            groundHits,
            distance + collisionSkinWidth,
            groundLayers,
            QueryTriggerInteraction.Ignore);

        bestHit = default;
        float closestDistance = float.PositiveInfinity;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = groundHits[i];
            if (hit.collider == null || IsOwnCollider(hit.collider) || !IsWalkable(hit.normal))
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

    private Vector3 RecoverFromOverlaps(Vector3 position)
    {
        Vector3 recoveredPosition = position;

        for (int iteration = 0; iteration < 3; iteration++)
        {
            GetCapsulePoints(recoveredPosition, out Vector3 bottom, out Vector3 top, out float radius);
            int hitCount = Physics.OverlapCapsuleNonAlloc(
                bottom,
                top,
                radius,
                recoveryHits,
                groundLayers,
                QueryTriggerInteraction.Ignore);

            bool recovered = false;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = recoveryHits[i];
                if (hit == null || IsOwnCollider(hit))
                {
                    continue;
                }

                if (Physics.ComputePenetration(
                    capsule,
                    recoveredPosition,
                    playerRotation,
                    hit,
                    hit.transform.position,
                    hit.transform.rotation,
                    out Vector3 direction,
                    out float distance))
                {
                    float recoveryDistance = IsWalkable(direction)
                        ? distance
                        : distance + collisionSkinWidth;
                    recoveredPosition += direction * Mathf.Min(recoveryDistance, bodyRadius);
                    recovered = true;
                }
            }

            if (!recovered)
            {
                break;
            }
        }

        return recoveredPosition;
    }

    private bool CastPlayer(Vector3 position, Vector3 direction, float distance, out RaycastHit bestHit)
    {
        if (distance <= 0f)
        {
            bestHit = default;
            return false;
        }

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
            if (hit.collider == null || IsOwnCollider(hit.collider))
            {
                continue;
            }

            if (hit.distance <= 0f && Vector3.Dot(hit.normal, direction) >= 0f)
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
            if (hit != null && !IsOwnCollider(hit))
            {
                return false;
            }
        }

        return true;
    }

    private void UpdateCrouchCollider(float deltaTime)
    {
        float targetHeight = isCrouching ? crouchingHeight : standingHeight;
        capsule.height = Mathf.Lerp(capsule.height, targetHeight, 1f - Mathf.Exp(-crouchSmoothSpeed * deltaTime));
        if (Mathf.Abs(capsule.height - targetHeight) <= HeightSnapEpsilon)
        {
            capsule.height = targetHeight;
        }

        capsule.center = Vector3.up * (capsule.height * 0.5f);
    }

    private void UpdateCameraPosition(float deltaTime)
    {
        if (cameraTransform == null || !cameraTransform.IsChildOf(transform))
        {
            return;
        }

        if (playerCamera != null)
        {
            playerCamera.nearClipPlane = Mathf.Min(playerCamera.nearClipPlane, safeNearClipPlane);
        }

        float targetHeight = isCrouching ? crouchingCameraHeight : standingCameraHeight;
        currentCameraHeight = Mathf.Lerp(currentCameraHeight, targetHeight, 1f - Mathf.Exp(-crouchSmoothSpeed * deltaTime));
        if (Mathf.Abs(currentCameraHeight - targetHeight) <= HeightSnapEpsilon)
        {
            currentCameraHeight = targetHeight;
        }

        Vector3 desiredLocalPosition = cameraBaseLocalPosition;
        desiredLocalPosition.y = currentCameraHeight;

        Vector3 desiredWorldPosition = transform.TransformPoint(desiredLocalPosition);
        cameraTransform.position = preventCameraClipping
            ? ResolveCameraCollision(desiredLocalPosition, desiredWorldPosition)
            : desiredWorldPosition;
    }

    private Vector3 ResolveCameraCollision(Vector3 desiredLocalPosition, Vector3 desiredWorldPosition)
    {
        float radius = Mathf.Clamp(cameraCollisionRadius, 0.01f, Mathf.Max(0.01f, bodyRadius - cameraSkinWidth));
        Vector3 pivotWorldPosition = transform.TransformPoint(new Vector3(0f, desiredLocalPosition.y, 0f));
        Vector3 pivotToCamera = desiredWorldPosition - pivotWorldPosition;
        float distance = pivotToCamera.magnitude;
        Vector3 resolvedPosition = desiredWorldPosition;

        if (distance > 0.0001f)
        {
            Vector3 direction = pivotToCamera / distance;
            int hitCount = Physics.SphereCastNonAlloc(
                pivotWorldPosition,
                radius,
                direction,
                cameraCastHits,
                distance + cameraSkinWidth,
                cameraCollisionLayers,
                QueryTriggerInteraction.Ignore);

            float closestDistance = float.PositiveInfinity;
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = cameraCastHits[i];
                if (hit.collider == null || IsOwnCollider(hit.collider))
                {
                    continue;
                }

                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                }
            }

            if (closestDistance < float.PositiveInfinity)
            {
                resolvedPosition = pivotWorldPosition + direction * Mathf.Max(0f, closestDistance - cameraSkinWidth);
            }
        }

        for (int iteration = 0; iteration < 2; iteration++)
        {
            int overlapCount = Physics.OverlapSphereNonAlloc(
                resolvedPosition,
                radius,
                cameraOverlapHits,
                cameraCollisionLayers,
                QueryTriggerInteraction.Ignore);

            bool pushed = false;
            for (int i = 0; i < overlapCount; i++)
            {
                Collider hit = cameraOverlapHits[i];
                if (hit == null || IsOwnCollider(hit))
                {
                    continue;
                }

                Vector3 closestPoint = hit.ClosestPoint(resolvedPosition);
                Vector3 pushDirection = resolvedPosition - closestPoint;
                float pushDistance = radius - pushDirection.magnitude;

                if (pushDirection.sqrMagnitude < 0.000001f)
                {
                    pushDirection = (resolvedPosition - hit.bounds.center).normalized;
                    if (pushDirection.sqrMagnitude < 0.000001f)
                    {
                        pushDirection = transform.forward;
                    }
                }
                else
                {
                    pushDirection.Normalize();
                }

                if (pushDistance > 0f)
                {
                    resolvedPosition += pushDirection * (pushDistance + cameraSkinWidth);
                    pushed = true;
                }
            }

            if (!pushed)
            {
                break;
            }
        }

        return resolvedPosition;
    }

    private bool CanStandUp()
    {
        if (!isCrouching)
        {
            return true;
        }

        float radius = Mathf.Max(0.01f, bodyRadius - collisionSkinWidth);
        Vector3 bottom = rb.position + Vector3.up * radius;
        Vector3 top = rb.position + Vector3.up * (standingHeight - radius);
        int hitCount = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, standCheckHits, groundLayers, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = standCheckHits[i];
            if (hit != null && !IsOwnCollider(hit))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsGrounded()
    {
        return groundedLastFixedUpdate || ProbeGround(rb.position, groundSnapDistance + 0.05f, out _);
    }

    private bool IsWalkable(Vector3 normal)
    {
        return Vector3.Angle(normal, Vector3.up) <= maxWalkableSlope;
    }

    private bool IsOwnCollider(Collider other)
    {
        return other == capsule || other.transform == transform || other.transform.IsChildOf(transform);
    }

    private Vector3 Flatten(Vector3 value)
    {
        value.y = 0f;
        return value;
    }

    private void GetCapsulePoints(Vector3 position, out Vector3 bottom, out Vector3 top, out float radius)
    {
        radius = Mathf.Max(0.01f, capsule.radius - collisionSkinWidth);
        float halfHeight = Mathf.Max(capsule.height * 0.5f, radius);
        Vector3 center = position + playerRotation * capsule.center;
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
        return runPressed
            && IsMoving()
            && !isCrouching
            && !runExhausted
            && currentRunStamina > 0f;
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
        AudioClip clip = null;

        if (footstepClips != null && footstepClips.Length > 0)
        {
            clip = footstepClips[Random.Range(0, footstepClips.Length)];
        }
        else
        {
            clip = generatedFootstepClip;
        }

        if (clip == null)
        {
            return;
        }

        float pitch = Random.Range(footstepPitchRange.x, footstepPitchRange.y);
        footstepSource.pitch = IsRunning() ? pitch * 1.05f : pitch;
        footstepSource.PlayOneShot(clip, footstepVolume);

        if (torchSway != null)
        {
            torchSway.OnFootstep(IsRunning());
        }
    }

    private void UpdateRunStamina()
    {
        if (IsRunning())
        {
            currentRunStamina -= runStaminaDrainRate * Time.deltaTime;
            rechargeTimer = rechargeDelay;

            if (currentRunStamina <= 0f)
            {
                currentRunStamina = 0f;
                runPressed = false;
                runExhausted = true;
            }
        }
        else
        {
            if (rechargeTimer > 0f)
            {
                rechargeTimer -= Time.deltaTime;
            }
            else
            {
                currentRunStamina += runStaminaRechargeRate * Time.deltaTime;

                if (currentRunStamina >= maxRunStamina)
                {
                    currentRunStamina = maxRunStamina;
                    runExhausted = false;
                }
            }
        }

        UpdateRunCooldownUI();
    }

    private void UpdateRunCooldownUI()
    {
        if (runCooldownFill != null)
        {
            runCooldownFill.fillAmount = currentRunStamina / maxRunStamina;
        }
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

    public bool GetIsMoving()
    {
        return IsMoving();
    }

    public bool GetIsRunning()
    {
        return IsRunning();
    }

    public bool GetIsCrouching()
    {
        return isCrouching;
    }
}
