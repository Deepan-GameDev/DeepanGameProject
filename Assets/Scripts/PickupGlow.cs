using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PickupGlow : MonoBehaviour
{
    private Transform player;

    public float glowDistance = 2f;
    public Color glowColor = Color.white;
    public float glowIntensity = 2f;

    private Renderer rend;
    private Material materialInstance;
    private Color originalEmission;
    private bool isGlowing;

    void Awake()
    {
        rend = GetComponent<Renderer>();

        materialInstance = rend.material;

        materialInstance.EnableKeyword("_EMISSION");

        if (materialInstance.HasProperty("_EmissionColor"))
        {
            originalEmission =
                materialInstance.GetColor("_EmissionColor");
        }
    }

    void Start()
    {
        GameObject playerObj =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null)
            return;

        float distance =
            Vector3.Distance(player.position, transform.position);

        if (distance <= glowDistance)
        {
            if (!isGlowing)
            {
                materialInstance.SetColor(
                    "_EmissionColor",
                    glowColor * glowIntensity);

                isGlowing = true;
            }
        }
        else
        {
            if (isGlowing)
            {
                materialInstance.SetColor(
                    "_EmissionColor",
                    originalEmission);

                isGlowing = false;
            }
        }
    }
}