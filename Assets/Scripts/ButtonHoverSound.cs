using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
    public AudioManager audioManager;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (audioManager == null)
            audioManager = AudioManager.Instance;

        if (audioManager != null)
            audioManager.PlayButtonHover();
    }
}
