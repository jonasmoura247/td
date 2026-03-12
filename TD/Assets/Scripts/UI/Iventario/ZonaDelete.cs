using UnityEngine;
using UnityEngine.EventSystems;

public class ZonaDelete : MonoBehaviour
{
    public static ZonaDelete Instance;

    [Header("Debug")]
    [SerializeField] private bool logDetectado = false;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Verifica se o pointer está sobre esta zona. Usa raycast para maior precisão.
    /// </summary>
    public bool ContemPonto(Vector2 screenPos)
    {
        return ContemPointer(eventData: null, screenPos: screenPos);
    }

    /// <summary>
    /// Verifica usando o raycast do evento - mais confiável.
    /// </summary>
    public bool ContemPointer(PointerEventData eventData, Vector2 screenPos = default)
    {
        if (eventData != null && eventData.pointerCurrentRaycast.isValid)
        {
            var go = eventData.pointerCurrentRaycast.gameObject;
            if (go != null && (go == gameObject || go.transform.IsChildOf(transform)))
            {
                if (logDetectado) Debug.Log("[ZonaDelete] Drop detectado via raycast.");
                return true;
            }
        }

        // Fallback: conversão de coordenadas
        var rt = GetComponent<RectTransform>();
        if (rt == null) return false;

        Vector2 pos = (eventData != null) ? eventData.position : screenPos;
        Canvas canvas = GetComponentInParent<Canvas>();
        Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? canvas.worldCamera : null;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, pos, cam, out Vector2 local))
        {
            bool contem = rt.rect.Contains(local);
            if (logDetectado && contem) Debug.Log("[ZonaDelete] Drop detectado via coordenadas.");
            return contem;
        }
        return false;
    }
}
