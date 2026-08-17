using UnityEngine;

public class BreakablePot : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip breakSound;

    [Header("Broken Replacement")]
    [Tooltip("Assign the broken-pot PREFAB here.")]
    [SerializeField] private GameObject replacementPrefab;

    [Header("Break Physics")]
    [SerializeField] private float explosionForce = 2.5f;
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private float upwardForce = 0.35f;

    private bool broken;

    private Renderer[] originalRenderers;
    private Collider[] originalColliders;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        CacheOriginalComponents();
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
    // BREAK
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

        // Break sound
        if (audioSource != null && breakSound != null)
        {
            audioSource.PlayOneShot(breakSound);
        }

        // Spawn broken pieces
        ShowBrokenPot(potPosition, potRotation);

        // Hide original intact pot
        HideOriginal();

        // Keep original pot locked
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    // ---------------------------------------------------------
    // BROKEN POT
    // ---------------------------------------------------------

    private void ShowBrokenPot(
        Vector3 position,
        Quaternion rotation)
    {
        if (replacementPrefab == null)
        {
            Debug.LogWarning(
                "BreakablePot: Replacement Prefab is not assigned."
            );

            return;
        }

        GameObject brokenPot = Instantiate(
            replacementPrefab,
            position,
            rotation
        );

        brokenPot.SetActive(true);

        // Make all renderers visible
        Renderer[] renderers =
            brokenPot.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = true;
            }
        }

        // Setup every broken piece
        Transform root = brokenPot.transform;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform piece = root.GetChild(i);

            if (piece == null)
            {
                continue;
            }

            SetupBrokenPiece(
                piece,
                position
            );
        }
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
            MeshFilter meshFilter =
                pieceObject.GetComponent<MeshFilter>();

            if (meshFilter != null &&
                meshFilter.sharedMesh != null)
            {
                MeshCollider meshCollider =
                    pieceObject.AddComponent<MeshCollider>();

                meshCollider.sharedMesh =
                    meshFilter.sharedMesh;

                meshCollider.convex = true;
            }
            else
            {
                pieceObject.AddComponent<BoxCollider>();
            }
        }

        // -----------------------------------------------------
        // RIGIDBODY
        // -----------------------------------------------------

        Rigidbody pieceRb =
            pieceObject.GetComponent<Rigidbody>();

        if (pieceRb == null)
        {
            pieceRb = pieceObject.AddComponent<Rigidbody>();
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

        // Random rotation
        pieceRb.AddTorque(
            Random.insideUnitSphere * 1.5f,
            ForceMode.Impulse
        );
    }

    // ---------------------------------------------------------
    // ORIGINAL POT
    // ---------------------------------------------------------

    private void CacheOriginalComponents()
    {
        originalRenderers =
            GetComponentsInChildren<Renderer>(true);

        originalColliders =
            GetComponentsInChildren<Collider>(true);
    }

    private void HideOriginal()
    {
        if (originalRenderers != null)
        {
            for (int i = 0; i < originalRenderers.Length; i++)
            {
                if (originalRenderers[i] != null)
                {
                    originalRenderers[i].enabled = false;
                }
            }
        }

        if (originalColliders != null)
        {
            for (int i = 0; i < originalColliders.Length; i++)
            {
                if (originalColliders[i] != null)
                {
                    originalColliders[i].enabled = false;
                }
            }
        }
    }
}