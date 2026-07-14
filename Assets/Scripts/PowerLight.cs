using UnityEngine;

[RequireComponent(typeof(Light))]
public class PowerLight : MonoBehaviour
{
    private Light lightComponent;

    private void Awake()
    {
        lightComponent = GetComponent<Light>();
        lightComponent.enabled = false;
    }

    public void TurnOn()
    {
        lightComponent.enabled = true;
    }
}