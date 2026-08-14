using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Light))]
public class PowerLight : MonoBehaviour
{
    private Light lightComponent;
    private readonly List<EmissionSlot> emissionSlots = new();

    private struct EmissionSlot
    {
        public Renderer Renderer;
        public int MaterialIndex;
        public Color OnColor;
    }

    private void Awake()
    {
        lightComponent = GetComponent<Light>();
        CacheFixtureEmission();
        lightComponent.enabled = false;
        SetFixtureEmission(false);
    }

    public void TurnOn()
    {
        lightComponent.enabled = true;
        SetFixtureEmission(true);
    }

    private void CacheFixtureEmission()
    {
        Transform fixtureRoot = transform.parent != null ? transform.parent : transform;
        Renderer[] renderers = fixtureRoot.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material != null && material.HasProperty("_EmissionColor"))
                {
                    Color emission = material.GetColor("_EmissionColor");
                    if (emission.maxColorComponent > 0f)
                        emissionSlots.Add(new EmissionSlot { Renderer = renderer, MaterialIndex = i, OnColor = emission });
                }
            }
        }
    }

    private void SetFixtureEmission(bool isOn)
    {
        foreach (EmissionSlot slot in emissionSlots)
        {
            if (slot.Renderer == null)
                continue;

            var block = new MaterialPropertyBlock();
            slot.Renderer.GetPropertyBlock(block, slot.MaterialIndex);
            block.SetColor("_EmissionColor", isOn ? slot.OnColor : Color.black);
            slot.Renderer.SetPropertyBlock(block, slot.MaterialIndex);
        }
    }
}
