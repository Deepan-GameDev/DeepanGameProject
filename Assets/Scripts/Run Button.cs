using UnityEngine;
using UnityEngine.EventSystems;

public class RunButton : MonoBehaviour, IPointerClickHandler
{
    public Player player;

    public void OnPointerClick(PointerEventData eventData)
    {
        player.ToggleRun();
    }
}