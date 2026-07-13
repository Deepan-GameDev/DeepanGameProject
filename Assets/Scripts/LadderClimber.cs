using System.Collections;
using UnityEngine;

public class LadderClimber : MonoBehaviour
{
    [Header("Ladder Points")]
    public Transform bottomPoint;
    public Transform topPoint;

    [Header("Player References")]
    public Player player;
    public MobileLook mobileLook;

    [Header("Movement")]
    public float alignSpeed = 3.5f;
    public float climbSpeed = 2f;
    public float rotationSpeed = 540f;
    public float finishDistance = 0.035f;
    public float stuckTimeout = 0.75f;
    public float maxClimbDuration = 12f;

    private Coroutine climbRoutine;
    private bool isClimbing;
    private readonly Collider[] ladderColliders = new Collider[16];
    private int ladderColliderCount;

    public bool IsClimbing
    {
        get { return isClimbing; }
    }

    private void Awake()
    {
        CacheLadderColliders();
    }

    public void StartClimb()
    {
        if (isClimbing || player == null || bottomPoint == null || topPoint == null)
        {
            return;
        }

        climbRoutine = StartCoroutine(ClimbRoutine());
    }

    private IEnumerator ClimbRoutine()
    {
        isClimbing = true;
        bool cleanupNeeded = false;

        Vector3 playerStartPosition = player.transform.position;
        Vector3 bottom = bottomPoint.position;
        Vector3 top = topPoint.position;
        bool climbUp = IsCloserToBottom(playerStartPosition, bottom, top);
        Vector3 startPoint = climbUp ? bottom : top;
        Vector3 endPoint = climbUp ? top : bottom;
        Quaternion ladderFacing = GetFacingRotation(playerStartPosition, bottom, top);

        try
        {
            player.BeginExternalMovement();
            SetLookLocked(true);
            SetLadderCollisionIgnored(true);
            cleanupNeeded = true;

            yield return MoveToPose(startPoint, ladderFacing, alignSpeed, true);

            if (!isClimbing)
            {
                yield break;
            }

            yield return MoveToPose(endPoint, ladderFacing, climbSpeed, false);

            if (isClimbing)
            {
                player.SetExternalMovementPose(endPoint, ladderFacing);
                yield return new WaitForFixedUpdate();
            }
        }
        finally
        {
            if (cleanupNeeded)
            {
                SetLadderCollisionIgnored(false);
                SetLookLocked(false);
                player.EndExternalMovement();
            }

            isClimbing = false;
            climbRoutine = null;
        }
    }

    private IEnumerator MoveToPose(Vector3 targetPosition, Quaternion targetRotation, float speed, bool rotate)
    {
        float elapsed = 0f;
        float noProgressTimer = 0f;
        float previousDistance = Vector3.Distance(player.transform.position, targetPosition);
        float previousAngle = Quaternion.Angle(player.transform.rotation, targetRotation);

        while (isClimbing)
        {
            elapsed += Time.fixedDeltaTime;

            if (elapsed > maxClimbDuration)
            {
                isClimbing = false;
                yield break;
            }

            Vector3 currentPosition = player.transform.position;
            Quaternion currentRotation = player.transform.rotation;
            Vector3 nextPosition = Vector3.MoveTowards(
                currentPosition,
                targetPosition,
                Mathf.Max(0.01f, speed) * Time.fixedDeltaTime);

            Quaternion nextRotation = rotate
                ? Quaternion.RotateTowards(currentRotation, targetRotation, rotationSpeed * Time.fixedDeltaTime)
                : targetRotation;

            player.SetExternalMovementPose(nextPosition, nextRotation);
            yield return new WaitForFixedUpdate();

            float distance = Vector3.Distance(player.transform.position, targetPosition);
            float angle = Quaternion.Angle(player.transform.rotation, targetRotation);
            bool rotationDone = !rotate || Quaternion.Angle(player.transform.rotation, targetRotation) <= 0.75f;

            if (distance <= finishDistance && rotationDone)
            {
                yield break;
            }

            bool movedCloser = distance < previousDistance - 0.002f;
            bool rotatedCloser = !rotate || angle < previousAngle - 0.25f;

            if (!movedCloser && !rotatedCloser)
            {
                noProgressTimer += Time.fixedDeltaTime;
                if (noProgressTimer >= stuckTimeout)
                {
                    isClimbing = false;
                    yield break;
                }
            }
            else
            {
                noProgressTimer = 0f;
            }

            previousDistance = distance;
            previousAngle = angle;
        }
    }

    private bool IsCloserToBottom(Vector3 playerPosition, Vector3 bottom, Vector3 top)
    {
        float distanceToBottom = (playerPosition - bottom).sqrMagnitude;
        float distanceToTop = (playerPosition - top).sqrMagnitude;
        return distanceToBottom <= distanceToTop;
    }

    private Quaternion GetFacingRotation(Vector3 playerPosition, Vector3 bottom, Vector3 top)
    {
        Vector3 ladderLine = top - bottom;
        Vector3 closestPoint = GetClosestPointOnLineSegment(playerPosition, bottom, top);
        Vector3 faceDirection = closestPoint - playerPosition;
        faceDirection.y = 0f;

        if (faceDirection.sqrMagnitude <= 0.0001f)
        {
            faceDirection = transform.position - playerPosition;
            faceDirection.y = 0f;
        }

        if (faceDirection.sqrMagnitude <= 0.0001f)
        {
            faceDirection = -transform.forward;
            faceDirection.y = 0f;
        }

        if (faceDirection.sqrMagnitude <= 0.0001f)
        {
            faceDirection = Vector3.ProjectOnPlane(-ladderLine, Vector3.up);
        }

        if (faceDirection.sqrMagnitude <= 0.0001f)
        {
            faceDirection = player.transform.forward;
            faceDirection.y = 0f;
        }

        return Quaternion.LookRotation(faceDirection.normalized, Vector3.up);
    }

    private Vector3 GetClosestPointOnLineSegment(Vector3 point, Vector3 start, Vector3 end)
    {
        Vector3 line = end - start;
        float lengthSquared = line.sqrMagnitude;

        if (lengthSquared <= 0.0001f)
        {
            return start;
        }

        float t = Vector3.Dot(point - start, line) / lengthSquared;
        return start + line * Mathf.Clamp01(t);
    }

    private void CacheLadderColliders()
    {
        ladderColliderCount = 0;
        Collider[] colliders = GetComponentsInChildren<Collider>();

        for (int i = 0; i < colliders.Length && ladderColliderCount < ladderColliders.Length; i++)
        {
            if (colliders[i] != null && !colliders[i].isTrigger)
            {
                ladderColliders[ladderColliderCount] = colliders[i];
                ladderColliderCount++;
            }
        }
    }

    private void SetLadderCollisionIgnored(bool ignored)
    {
        CapsuleCollider playerCollider = player != null ? player.GetBodyCollider() : null;
        if (playerCollider == null)
        {
            return;
        }

        for (int i = 0; i < ladderColliderCount; i++)
        {
            if (ladderColliders[i] != null)
            {
                Physics.IgnoreCollision(playerCollider, ladderColliders[i], ignored);
            }
        }
    }

    private void SetLookLocked(bool locked)
    {
        if (mobileLook != null)
        {
            mobileLook.SetLookLocked(locked);
        }
    }

    private void OnDisable()
    {
        if (climbRoutine != null)
        {
            StopCoroutine(climbRoutine);
            climbRoutine = null;
        }

        if (isClimbing)
        {
            SetLadderCollisionIgnored(false);
            SetLookLocked(false);

            if (player != null)
            {
                player.EndExternalMovement();
            }
        }

        isClimbing = false;
    }
}
