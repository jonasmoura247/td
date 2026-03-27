using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ReliquiaDragSalao : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image imagem;

    private string idReliquia;
    private bool veioDeSlotEquipamento;
    private int indiceSlotEquipamentoOrigem = -1;
    private int indiceSlotDisponivelOrigem = -1;
    private Transform parentOriginal;
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private SalaoReliquiaGoblinUI controlador;
    private bool dragValido;
    private Vector2 salvoAnchorMin;
    private Vector2 salvoAnchorMax;
    private Vector2 salvoAnchoredPosition;
    private Vector2 salvoOffsetMin;
    private Vector2 salvoOffsetMax;
    private Vector3 salvoLocalScale;
    private Quaternion salvoLocalRotation;
    private Vector2 salvoPivot;
    private Vector2 salvoSizeDelta;
    private Vector2 offsetArrasteCanvas;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (imagem == null)
            imagem = GetComponent<Image>();
    }

    public void Configurar(SalaoReliquiaGoblinUI ui, string id, bool deSlotEquipamento, int indiceSlotEquipamento,
        int indiceSlotDisponivel = -1)
    {
        controlador = ui;
        idReliquia = id;
        veioDeSlotEquipamento = deSlotEquipamento;
        indiceSlotEquipamentoOrigem = indiceSlotEquipamento;
        indiceSlotDisponivelOrigem = indiceSlotDisponivel;
    }

    private void ResolverControladorSeNecessario()
    {
        if (controlador != null)
            return;
        var encontrados = FindObjectsByType<SalaoReliquiaGoblinUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (encontrados != null && encontrados.Length > 0)
            controlador = encontrados[0];
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ResolverControladorSeNecessario();
        dragValido = ReliquiasManager.PodeEditarEquipamento() && controlador != null && !string.IsNullOrEmpty(idReliquia);
        if (!dragValido)
            return;

        parentOriginal = transform.parent;
        canvas = GetComponentInParent<Canvas>();
        rectTransform = transform as RectTransform;
        if (rectTransform != null)
        {
            salvoAnchorMin = rectTransform.anchorMin;
            salvoAnchorMax = rectTransform.anchorMax;
            salvoAnchoredPosition = rectTransform.anchoredPosition;
            salvoOffsetMin = rectTransform.offsetMin;
            salvoOffsetMax = rectTransform.offsetMax;
            salvoLocalScale = rectTransform.localScale;
            salvoLocalRotation = rectTransform.localRotation;
            salvoPivot = rectTransform.pivot;
            salvoSizeDelta = rectTransform.sizeDelta;
        }

        if (canvas != null)
        {
            transform.SetParent(canvas.transform, true);
            var canvasRt = canvas.transform as RectTransform;
            Camera cam = ObterCameraParaCanvas(canvas, eventData);
            if (canvasRt != null && rectTransform != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, eventData.position, cam,
                    out Vector2 pLocal))
                offsetArrasteCanvas = rectTransform.anchoredPosition - pLocal;
        }

        canvasGroup.blocksRaycasts = false;
    }

    private static Camera ObterCameraParaCanvas(Canvas c, PointerEventData eventData)
    {
        if (c == null)
            return null;
        if (c.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;
        if (c.worldCamera != null)
            return c.worldCamera;
        return eventData != null ? eventData.pressEventCamera : null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragValido)
            return;
        if (rectTransform == null || canvas == null)
            return;

        var canvasRt = canvas.transform as RectTransform;
        if (canvasRt == null)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            return;
        }

        Camera cam = ObterCameraParaCanvas(canvas, eventData);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, eventData.position, cam, out Vector2 pLocal))
            rectTransform.anchoredPosition = pLocal + offsetArrasteCanvas;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        ResolverControladorSeNecessario();

        Vector2 posTela = eventData.position;
        if (posTela == Vector2.zero)
            posTela = Input.mousePosition;

        if (!dragValido || controlador == null)
        {
            ResetarPosicaoParent();
            return;
        }

        if (EventSystem.current == null)
        {
            ResetarPosicaoParent();
            return;
        }

        var resultados = ColetarTodosRaycasts(eventData, posTela, gameObject);

        ReliquiaSlotDropSalao slotEquip = BuscarPrimeiroEquipNosResultados(resultados);

        if (slotEquip != null)
        {
            if (!veioDeSlotEquipamento)
            {
                controlador.TentarEquiparEmSlot(slotEquip.IndiceSlot, idReliquia);
            }
            else if (indiceSlotEquipamentoOrigem >= 0 && slotEquip.IndiceSlot != indiceSlotEquipamentoOrigem)
            {
                controlador.TentarMoverEntreSlots(indiceSlotEquipamentoOrigem, slotEquip.IndiceSlot, idReliquia);
            }
            else
            {
                ResetarPosicaoParent();
                return;
            }

            controlador.AtualizarTudo();
            ResetarPosicaoParent();
            return;
        }

        ReliquiaSlotDisponivelSalao slotDisp = BuscarPrimeiroDisponivelNosResultados(resultados);

        if (slotDisp != null)
        {
            if (veioDeSlotEquipamento && indiceSlotEquipamentoOrigem >= 0)
            {
                controlador.TentarDesequiparParaSlotDisponivel(indiceSlotEquipamentoOrigem, slotDisp.IndiceSlot);
            }
            else if (!veioDeSlotEquipamento)
            {
                if (indiceSlotDisponivelOrigem >= 0 && indiceSlotDisponivelOrigem == slotDisp.IndiceSlot)
                {
                    ResetarPosicaoParent();
                    return;
                }

                controlador.TentarMoverPossuidaParaSlotVisual(indiceSlotDisponivelOrigem, slotDisp.IndiceSlot, idReliquia);
            }
            else
            {
                ResetarPosicaoParent();
                return;
            }

            controlador.AtualizarTudo();
            ResetarPosicaoParent();
            return;
        }

        ResetarPosicaoParent();
    }

    private static List<RaycastResult> ColetarTodosRaycasts(PointerEventData eventData, Vector2 posTela,
        GameObject ignorarRaiz)
    {
        var unificados = new List<RaycastResult>();
        var vistos = new HashSet<int>();
        Transform ignorarT = ignorarRaiz != null ? ignorarRaiz.transform : null;

        void AdicionarLista(List<RaycastResult> lista)
        {
            for (int i = 0; i < lista.Count; i++)
            {
                var go = lista[i].gameObject;
                if (go == null)
                    continue;
                if (ignorarT != null && (go.transform == ignorarT || go.transform.IsChildOf(ignorarT)))
                    continue;
                int id = go.GetInstanceID();
                if (id != 0 && vistos.Add(id))
                    unificados.Add(lista[i]);
            }
        }

        var a = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, a);
        AdicionarLista(a);

        var ped = new PointerEventData(EventSystem.current)
        {
            position = posTela
        };
        var b = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, b);
        AdicionarLista(b);

        ped.position = Input.mousePosition;
        var c = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, c);
        AdicionarLista(c);

        return unificados;
    }

    private static ReliquiaSlotDropSalao BuscarPrimeiroEquipNosResultados(List<RaycastResult> resultados)
    {
        for (int i = 0; i < resultados.Count; i++)
        {
            var slot = EncontrarSlotDropNoObjeto(resultados[i].gameObject);
            if (slot != null)
                return slot;
        }
        return null;
    }

    private static ReliquiaSlotDisponivelSalao BuscarPrimeiroDisponivelNosResultados(List<RaycastResult> resultados)
    {
        for (int i = 0; i < resultados.Count; i++)
        {
            var slot = EncontrarSlotDisponivelNoObjeto(resultados[i].gameObject);
            if (slot != null)
                return slot;
        }
        return null;
    }

    private static ReliquiaSlotDropSalao EncontrarSlotDropNoObjeto(GameObject go)
    {
        if (go == null)
            return null;
        var s = go.GetComponent<ReliquiaSlotDropSalao>();
        if (s != null)
            return s;
        s = go.GetComponentInParent<ReliquiaSlotDropSalao>();
        if (s != null)
            return s;
        return go.GetComponentInChildren<ReliquiaSlotDropSalao>(true);
    }

    private static ReliquiaSlotDisponivelSalao EncontrarSlotDisponivelNoObjeto(GameObject go)
    {
        if (go == null)
            return null;
        var s = go.GetComponent<ReliquiaSlotDisponivelSalao>();
        if (s != null)
            return s;
        s = go.GetComponentInParent<ReliquiaSlotDisponivelSalao>();
        if (s != null)
            return s;
        return go.GetComponentInChildren<ReliquiaSlotDisponivelSalao>(true);
    }

    private void ResetarPosicaoParent()
    {
        if (parentOriginal == null)
            return;

        transform.SetParent(parentOriginal, false);
        transform.SetAsLastSibling();

        if (rectTransform != null)
        {
            rectTransform.anchorMin = salvoAnchorMin;
            rectTransform.anchorMax = salvoAnchorMax;
            rectTransform.pivot = salvoPivot;
            rectTransform.sizeDelta = salvoSizeDelta;
            rectTransform.anchoredPosition = salvoAnchoredPosition;
            rectTransform.offsetMin = salvoOffsetMin;
            rectTransform.offsetMax = salvoOffsetMax;
            rectTransform.localScale = salvoLocalScale;
            rectTransform.localRotation = salvoLocalRotation;
        }
    }
}
