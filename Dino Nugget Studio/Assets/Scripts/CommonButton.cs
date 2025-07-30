using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CommonButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private UnityEngine.UI.Image image;
    private TextMeshProUGUI text;
    private bool isHovering = false;

    [SerializeField] private float scaleFactor = 1.2f; // How much to enlarge the button
    [SerializeField] private float scaleSpeed = 5f; // Speed of the scale animation
    private Vector3 originalScale;

    private void Awake()
    {
        image = GetComponent<UnityEngine.UI.Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        originalScale = transform.localScale;
    }

    private void Update()
    {
        // Smoothly scale the button based on hover state
        Vector3 targetScale = isHovering ? originalScale * scaleFactor : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        image.color = Color.black;
        text.color = Color.white;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        image.color = Color.white;
        text.color = Color.black;
    }

    private void OnDisable()
    {
        // Reset scale when disabled
        transform.localScale = originalScale;
        image.color = Color.white;
        text.color = Color.black;
        isHovering = false;
    }
}