using UnityEngine;

public class BreakablePot : MonoBehaviour, IInteractable
{
    [Header("References")]
    public Rigidbody rb;
    public ParticleSystem breakEffect;
    public ParticleSystem breakDust;
    public AudioSource audioSource;
    public AudioClip breakSound;

    [Header("Knock Settings")]
    public float knockForce = 2.5f;
    public float upwardForce = 0.4f;
    public float breakVelocity = 1.0f;

    private bool knocked = false;
    private bool broken = false;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = true;
        }
    }

    // Called by your existing InteractionManager
    public void Interact()
    {
        if (knocked || broken)
            return;

        KnockPot();
    }

    private void KnockPot()
    {
        knocked = true;

        if (rb == null)
            return;

        rb.isKinematic = false;

        // Push the pot forward
        Vector3 direction = transform.forward;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            direction = Vector3.forward;

        direction.Normalize();

        rb.AddForce(
            direction * knockForce +
            Vector3.up * upwardForce,
            ForceMode.Impulse
        );

        // Make the pot fall / roll
        rb.AddTorque(
            Random.insideUnitSphere * 2f,
            ForceMode.Impulse
        );
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!knocked || broken)
            return;

        bool hitFloor = false;

        // Check whether the collision surface is a floor
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);

            if (contact.normal.y > 0.5f)
            {
                hitFloor = true;
                break;
            }
        }

        if (!hitFloor)
            return;

        float impactSpeed = collision.relativeVelocity.magnitude;

        if (impactSpeed >= breakVelocity)
        {
            BreakPot(collision);
        }
    }

    private void BreakPot(Collision collision)
    {
        if (broken)
            return;

        broken = true;

        Debug.Log("POT BROKEN!");

        // --------------------------------------------------
        // BREAK SOUND
        // --------------------------------------------------

        if (audioSource != null && breakSound != null)
        {
            audioSource.PlayOneShot(breakSound);
        }
        else
        {
            Debug.LogWarning(
                "BreakablePot: AudioSource or BreakSound is not assigned."
            );
        }

        // --------------------------------------------------
        // BREAK EFFECT
        // --------------------------------------------------

        if (breakEffect != null)
        {
            breakEffect.transform.SetParent(null);

            breakEffect.transform.position = transform.position;
            breakEffect.transform.rotation = Quaternion.identity;

            breakEffect.gameObject.SetActive(true);

            breakEffect.Clear();
            breakEffect.Play();

            var main = breakEffect.main;

            Destroy(
                breakEffect.gameObject,
                main.duration + main.startLifetime.constantMax + 1f
            );
        }
        else
        {
            Debug.LogWarning(
                "BreakablePot: Break Effect is not assigned."
            );
        }

        // --------------------------------------------------
        // FLOOR DUST
        // --------------------------------------------------

        if (breakDust != null)
        {
            Vector3 dustPosition = transform.position;

            // Find the actual floor contact position
            if (collision.contactCount > 0)
            {
                dustPosition = collision.GetContact(0).point;
            }

            breakDust.transform.SetParent(null);

            breakDust.transform.position = dustPosition;
            breakDust.transform.rotation = Quaternion.identity;

            breakDust.gameObject.SetActive(true);

            breakDust.Clear();
            breakDust.Play();

            var dustMain = breakDust.main;

            Destroy(
                breakDust.gameObject,
                dustMain.duration +
                dustMain.startLifetime.constantMax +
                1f
            );
        }
        else
        {
            Debug.LogWarning(
                "BreakablePot: Break Dust is not assigned."
            );
        }

        // --------------------------------------------------
        // HIDE POT
        // --------------------------------------------------

        Renderer[] renderers =
            GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        // --------------------------------------------------
        // DISABLE COLLIDERS
        // --------------------------------------------------

        Collider[] colliders =
            GetComponentsInChildren<Collider>();

        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }

        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }
}