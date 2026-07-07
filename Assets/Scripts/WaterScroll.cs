using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WaterScroll : MonoBehaviour
{
    [Header("Water Movement")]
    public Vector2 scrollSpeed = new Vector2(0.02f, 0.01f);

    private Renderer waterRenderer;
    private Material waterMaterial;
    private Vector2 currentOffset;

    void Awake()
    {
        waterRenderer = GetComponent<Renderer>();
        waterMaterial = waterRenderer.material;
    }

    void Update()
    {
        currentOffset += scrollSpeed * Time.deltaTime;

        waterMaterial.SetTextureOffset(
            "_BaseMap",
            currentOffset
        );

        waterMaterial.SetTextureOffset(
            "_BumpMap",
            currentOffset
        );
    }
}