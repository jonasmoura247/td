using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image iconeItem;
    [Tooltip("Text que exibe a quantidade (ex: 2, 10). Aparece quando quantidade > 1.")]
    public Text textoQuantidade;

    private string idItem;
    private int quantidade;
    private Transform parentOriginal;
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (iconeItem == null)
        {
            Transform icone = transform.Find("IconeItem");
            if (icone != null)
                iconeItem = icone.GetComponent<Image>();

            if (iconeItem == null)
            {
                Image[] images = GetComponentsInChildren<Image>(true);
                foreach (Image img in images)
                {
                    if (img.gameObject != gameObject)
                    {
                        iconeItem = img;
                        break;
                    }
                }
            }
        }

        if (textoQuantidade == null)
        {
            Transform txt = transform.Find("TextoQuantidade");
            if (txt != null)
                textoQuantidade = txt.GetComponent<Text>();
            if (textoQuantidade == null)
                textoQuantidade = GetComponentInChildren<Text>(true);
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (iconeItem != null)
        {
            iconeItem.enabled = false;
            iconeItem.raycastTarget = false; // Clicks passam para o slot (root) para o drag funcionar
        }
    }

    public void Configurar(Sprite spriteItem, string id, int qtd)
    {
        idItem = id;
        quantidade = qtd;

        if (iconeItem != null)
        {
            if (spriteItem == null)
            {
                iconeItem.enabled = false;
            }
            else
            {
                iconeItem.enabled = true;
                iconeItem.sprite = spriteItem;
                iconeItem.color = Color.white;
                iconeItem.preserveAspect = true;
            }
        }

        if (textoQuantidade != null)
        {
            textoQuantidade.gameObject.SetActive(qtd > 1);
            textoQuantidade.text = qtd.ToString();
            textoQuantidade.raycastTarget = false; // Evita bloquear o drag no slot
        }
    }

    /// <summary>
    /// Compatibilidade com AdicionarSlot. O slot será exibido mas não terá funcionalidade de delete.
    /// </summary>
    public void Configurar(Sprite spriteItem)
    {
        Configurar(spriteItem, "", 1);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentOriginal = transform.parent;
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        transform.SetParent(canvas.transform);
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform != null && canvas != null)
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
        transform.SetParent(parentOriginal);
        transform.SetAsLastSibling();

        if (ZonaDelete.Instance != null && ZonaDelete.Instance.ContemPointer(eventData, eventData.position))
        {
            if (quantidade <= 1)
            {
                if (LootManager.Instance != null)
                    LootManager.Instance.RemoverItem(idItem, 1);
            }
            else
            {
                var popup = PopupQuantidadeDelete.Instance;
                if (popup == null)
                {
                    popup = FindFirstObjectByType<PopupQuantidadeDelete>(FindObjectsInactive.Include);
                    if (popup != null) popup.gameObject.SetActive(true); // Ativa para Awake rodar e setar Instance
                }
                popup?.Abrir(idItem, quantidade);
            }
        }
    }

    public string IdItem => idItem;
    public int Quantidade => quantidade;
}
