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
    public KeyCode runKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Body")]
    public float standingHeight = 1.85f;
    public float crouchingHeight = 1.15f;
    public float bodyRadius = 0.35f;
    public float crouchSmoothSpeed = 10f;
    public Transform cameraTransform;
    public float standingCameraHeight = 1.6f;
    public float crouchingCameraHeight = 0.95f;
    public LayerMask groundLayers = ~0;

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

    public void AddYawInput(float yawDegrees)
    {
        pendingYaw += yawDegrees;
    }

  public void SetMoveInput(Vector2 input)
{
    moveInput = Vector2.ClampMagnitude(input, 1f);

}

public void ToggleRun()
{
    runPressed = !runPressed;
    Debug.Log("ToggleRun = " + runPressed + " Frame: " + Time.frameCount);
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
{
    isCrouching = crouchPressed || !CanStandUp();

    UpdateCrouch();

    UpdateFootsteps();
}

    isCrouching = crouchPressed || !CanStandUp();

    UpdateCrouch();

  //  if (!IsMoving())
 //   {
  //      runPressed = false;
  //  }

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
        rb.MovePosition(rb.position + worldMove);
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

