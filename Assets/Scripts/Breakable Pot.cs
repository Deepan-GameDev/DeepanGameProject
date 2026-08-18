using UnityEngine;

public class BreakablePot : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private AudioClip breakSound;

    [Header("Broken Replacement")]
    [Tooltip("Assign the broken-pot PREFAB here.")]
    [SerializeField] private GameObject replacementPrefab;

    [Header("Object After Break")]
    [Tooltip("This GameObject will appear when the pot breaks.")]
    [SerializeField] private GameObject objectAfterBreak;

    [Header("Break Physics")]
    [SerializeField] private float explosionForce = 2.5f;
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private float upwardForce = 0.35f;

    [Header("Broken Object Cleanup")]
    [SerializeField] private float brokenObjectLifetime = 2f;

    private bool broken;

    // ---------------------------------------------------------
    // UNITY
    // ---------------------------------------------------------

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        // Keep original intact pot fixed
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Keep the object hidden until the pot breaks
        if (objectAfterBreak != null)
        {
            objectAfterBreak.SetActive(false);
        }
    }

    // ---------------------------------------------------------
    // INTERACTION
    // ---------------------------------------------------------

    public void Interact()
    {
        if (broken)
        {
            return;
        }

        BreakObject();
    }

    // ---------------------------------------------------------
    // BREAK OBJECT
    // ---------------------------------------------------------

    private void BreakObject()
    {
        if (broken)
        {
            return;
        }

        broken = true;

        Vector3 potPosition = transform.position;
        Quaternion potRotation = transform.rotation;

        // -----------------------------------------------------
        // BREAK SOUND
        // -----------------------------------------------------

        if (breakSound != null)
        {
            AudioSource.PlayClipAtPoint(
                breakSound,
                potPosition
            );
        }

        // -----------------------------------------------------
        // SPAWN BROKEN POT
        // -----------------------------------------------------

        GameObject brokenPot = ShowBrokenPot(
            potPosition,
            potRotation
        );

        // -----------------------------------------------------
        // SHOW OBJECT AFTER BREAK
        // -----------------------------------------------------

        if (objectAfterBreak != null)
        {
            objectAfterBreak.SetActive(true);
        }

        // -----------------------------------------------------
        // DESTROY ORIGINAL INTACT POT
        // -----------------------------------------------------

        Destroy(gameObject);
    }

    // ---------------------------------------------------------
    // BROKEN POT
    // ---------------------------------------------------------

    private GameObject ShowBrokenPot(
        Vector3 position,
        Quaternion rotation)
    {
        if (replacementPrefab == null)
        {
            Debug.LogWarning(
                "BreakablePot: Replacement Prefab is not assigned."
            );

            return null;
        }

        GameObject brokenPot = Instantiate(
            replacementPrefab,
            position,
            rotation
        );

        brokenPot.SetActive(true);

        // -----------------------------------------------------
        // HIDE ONLY THE INTACT "Pot" CHILD
        // -----------------------------------------------------

        Transform intactPot =
            brokenPot.transform.Find("Pot");

        if (intactPot != null)
        {
            intactPot.gameObject.SetActive(false);
        }

        // -----------------------------------------------------
        // ENABLE BROKEN PIECE RENDERERS
        // -----------------------------------------------------

        Renderer[] renderers =
            brokenPot.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            if (renderer == null)
            {
                continue;
            }

            // Don't enable the hidden intact Pot
            if (intactPot != null &&
                renderer.transform.IsChildOf(intactPot))
            {
                continue;
            }

            renderer.enabled = true;
        }

        // -----------------------------------------------------
        // SETUP BROKEN PIECES
        // -----------------------------------------------------

        Transform root = brokenPot.transform;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform piece = root.GetChild(i);

            if (piece == null)
            {
                continue;
            }

            // Skip intact Pot
            if (intactPot != null &&
                piece == intactPot)
            {
                continue;
            }

            SetupBrokenPiece(
                piece,
                position
            );
        }

        // -----------------------------------------------------
        // DESTROY BROKEN POT AFTER 2 SECONDS
        // -----------------------------------------------------

        Destroy(
            brokenPot,
            brokenObjectLifetime
        );

        return brokenPot;
    }

    // ---------------------------------------------------------
    // BROKEN PIECE PHYSICS
    // ---------------------------------------------------------

    private void SetupBrokenPiece(
        Transform piece,
        Vector3 explosionPosition)
    {
        GameObject pieceObject = piece.gameObject;

        pieceObject.SetActive(true);

        // -----------------------------------------------------
        // COLLIDER
        // -----------------------------------------------------

        Collider pieceCollider =
            pieceObject.GetComponent<Collider>();

        if (pieceCollider == null)
        {
            // BoxCollider avoids problematic convex MeshCollider
            BoxCollider boxCollider =
                pieceObject.AddComponent<BoxCollider>();

            Renderer pieceRenderer =
                pieceObject.GetComponent<Renderer>();

            if (pieceRenderer != null)
            {
                Bounds bounds =
                    pieceRenderer.localBounds;

                boxCollider.center =
                    bounds.center;

                boxCollider.size =
                    bounds.size;
            }
        }

        // -----------------------------------------------------
        // RIGIDBODY
        // -----------------------------------------------------

        Rigidbody pieceRb =
            pieceObject.GetComponent<Rigidbody>();

        if (pieceRb == null)
        {
            pieceRb =
                pieceObject.AddComponent<Rigidbody>();
        }

        pieceRb.isKinematic = false;
        pieceRb.useGravity = true;

        // Mobile-friendly physics
        pieceRb.mass = 0.25f;
        pieceRb.linearDamping = 0.1f;
        pieceRb.angularDamping = 0.2f;

        // -----------------------------------------------------
        // EXPLOSION FORCE
        // -----------------------------------------------------

        pieceRb.AddExplosionForce(
            explosionForce,
            explosionPosition,
            explosionRadius,
            upwardForce,
            ForceMode.Impulse
        );

        // -----------------------------------------------------
        // RANDOM ROTATION
        // -----------------------------------------------------

        pieceRb.AddTorque(
            Random.insideUnitSphere * 1.5f,
            ForceMode.Impulse
        );
    }
}