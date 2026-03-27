using UnityEngine;
using UnityEngine.UI;

public class EfeitoOlhoQueNadaVe : MonoBehaviour, IReliquiaEfeito
{
    [SerializeField] private float alphaCamadaEscura = 0.55f;
    [SerializeField] private float alphaCamadaNevoa = 0.22f;
    [SerializeField] private float multiplicadorOrtho = 0.88f;
    [SerializeField] private Color corNevoa = new Color(0.55f, 0.58f, 0.62f, 1f);

    private GameObject raizUi;
    private Camera cam;
    private float orthoOriginal;
    private bool alterouOrtho;

    public void Iniciar(ReliquiaContext contexto)
    {
        cam = contexto.CameraPrincipal;
        if (cam != null && cam.orthographic)
        {
            alterouOrtho = true;
            orthoOriginal = cam.orthographicSize;
            cam.orthographicSize = orthoOriginal * multiplicadorOrtho;
        }

        Canvas canvas = contexto.CanvasOverlay;
        if (canvas == null)
            return;

        raizUi = new GameObject("FogGuerra_OlhoQueNadaVe");
        raizUi.transform.SetParent(canvas.transform, false);
        RectTransform rtRaiz = raizUi.AddComponent<RectTransform>();
        rtRaiz.anchorMin = Vector2.zero;
        rtRaiz.anchorMax = Vector2.one;
        rtRaiz.offsetMin = Vector2.zero;
        rtRaiz.offsetMax = Vector2.zero;
        rtRaiz.SetAsLastSibling();

        GameObject goEscuro = new GameObject("CamadaEscura");
        goEscuro.transform.SetParent(raizUi.transform, false);
        RectTransform re = goEscuro.AddComponent<RectTransform>();
        re.anchorMin = Vector2.zero;
        re.anchorMax = Vector2.one;
        re.offsetMin = Vector2.zero;
        re.offsetMax = Vector2.zero;
        Image imgEscuro = goEscuro.AddComponent<Image>();
        imgEscuro.color = new Color(0f, 0f, 0f, alphaCamadaEscura);
        imgEscuro.raycastTarget = false;

        GameObject goNevoa = new GameObject("CamadaNevoa");
        goNevoa.transform.SetParent(raizUi.transform, false);
        RectTransform rn = goNevoa.AddComponent<RectTransform>();
        rn.anchorMin = Vector2.zero;
        rn.anchorMax = Vector2.one;
        rn.offsetMin = Vector2.zero;
        rn.offsetMax = Vector2.zero;
        Image imgNevoa = goNevoa.AddComponent<Image>();
        imgNevoa.color = new Color(corNevoa.r, corNevoa.g, corNevoa.b, alphaCamadaNevoa);
        imgNevoa.raycastTarget = false;
    }

    public void Encerrar()
    {
        if (cam != null && alterouOrtho)
        {
            cam.orthographicSize = orthoOriginal;
        }

        cam = null;
        alterouOrtho = false;

        if (raizUi != null)
        {
            Destroy(raizUi);
            raizUi = null;
        }
    }
}
