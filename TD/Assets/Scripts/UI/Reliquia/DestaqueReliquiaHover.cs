using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Adiciona contorno sutil em volta do item (cor conforme raça) e intensifica ao passar o mouse.
/// Usa um filho Image atrás do item — apenas contorno, sem replicar a imagem.
/// </summary>
[RequireComponent(typeof(Image))]
public class DestaqueReliquiaHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Raça (define cor do contorno)")]
    [SerializeField] private TipoRaca raca = TipoRaca.Goblin;

    [Header("Contorno sutil")]
    [SerializeField] [Range(1.02f, 1.15f)] private float escalaSutil = 1.05f;
    [SerializeField] [Range(0.2f, 0.7f)] private float alphaSutil = 0.5f;

    [Header("Contorno no hover")]
    [SerializeField] [Range(1.05f, 1.25f)] private float escalaHover = 1.1f;
    [SerializeField] [Range(0.5f, 1f)] private float alphaHover = 0.9f;

    [Header("Transição")]
    [SerializeField] private float duracaoTransicao = 0.12f;

    private static readonly Color GoblinVerde = new Color(0.4f, 0.9f, 0.35f, 1f);
    private static readonly Color ElfoAzul = new Color(0.4f, 0.7f, 1f, 1f);
    private static readonly Color AnaoMarrom = new Color(0.8f, 0.5f, 0.3f, 1f);
    private static readonly Color HumanoDourado = new Color(0.95f, 0.85f, 0.5f, 1f);

    private Image imagemContorno;
    private float escalaAlvo;
    private float alphaAlvo;
    private float t;

    private void Awake()
    {
        var outline = GetComponent<Outline>();
        if (outline != null)
            Destroy(outline);

        CriarOuObterContorno();
    }

    private void Start()
    {
        AplicarEstadoSutil();
    }

    private void Update()
    {
        if (imagemContorno == null) return;
        if (t < 1f && duracaoTransicao > 0)
        {
            t += Time.deltaTime / duracaoTransicao;
            t = Mathf.Clamp01(t);
            float scale = Mathf.Lerp(imagemContorno.transform.localScale.x, escalaAlvo, t);
            imagemContorno.transform.localScale = Vector3.one * scale;
            Color c = imagemContorno.color;
            c.a = Mathf.Lerp(c.a, alphaAlvo, t);
            imagemContorno.color = c;
        }
    }

    private void CriarOuObterContorno()
    {
        Transform filho = transform.Find("ContornoReliquia");
        if (filho != null)
        {
            imagemContorno = filho.GetComponent<Image>();
            if (imagemContorno != null) return;
        }

        Image imagemPrincipal = GetComponent<Image>();
        if (imagemPrincipal == null || imagemPrincipal.sprite == null)
        {
            Debug.LogWarning("[DestaqueReliquiaHover] Image ou Sprite nao encontrado em " + gameObject.name);
            return;
        }

        GameObject goContorno = new GameObject("ContornoReliquia");
        goContorno.transform.SetParent(transform, false);

        imagemContorno = goContorno.AddComponent<Image>();
        imagemContorno.sprite = imagemPrincipal.sprite;
        imagemContorno.color = ObterCorDaRaca();
        imagemContorno.raycastTarget = false;
        imagemContorno.preserveAspect = imagemPrincipal.preserveAspect;

        RectTransform rt = goContorno.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one * escalaSutil;

        goContorno.transform.SetAsFirstSibling();
    }

    private Color ObterCorDaRaca()
    {
        return raca switch
        {
            TipoRaca.Goblin => GoblinVerde,
            TipoRaca.Elfo => ElfoAzul,
            TipoRaca.Anao => AnaoMarrom,
            TipoRaca.Humano => HumanoDourado,
            _ => GoblinVerde
        };
    }

    private void AplicarEstadoSutil()
    {
        escalaAlvo = escalaSutil;
        alphaAlvo = alphaSutil;
        t = 1f;
        if (imagemContorno != null)
        {
            Color c = ObterCorDaRaca();
            c.a = alphaSutil;
            imagemContorno.color = c;
            imagemContorno.transform.localScale = Vector3.one * escalaSutil;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        escalaAlvo = escalaHover;
        alphaAlvo = alphaHover;
        t = 0f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        escalaAlvo = escalaSutil;
        alphaAlvo = alphaSutil;
        t = 0f;
    }
}
