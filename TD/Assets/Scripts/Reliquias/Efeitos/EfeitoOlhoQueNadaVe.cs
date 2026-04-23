using UnityEngine;
using UnityEngine.UI;

public class EfeitoOlhoQueNadaVe : MonoBehaviour, IReliquiaEfeito
{
    [SerializeField] private float alphaCamadaEscura   = 0.55f;
    [SerializeField] private float alphaCamadaNevoa    = 0.22f;
    [SerializeField] private float multiplicadorOrtho  = 0.88f;
    [SerializeField] private Color corNevoa            = new Color(0.55f, 0.58f, 0.62f, 1f);
    [SerializeField] private float raioReveal          = 0.12f;
    [SerializeField] private float suavidadeReveal     = 0.03f;
    // Atribuir no prefab. Se null, usa Shader.Find como fallback.
    [SerializeField] private Shader shaderFog;

    private static readonly int IdCenterX = Shader.PropertyToID("_CenterX");
    private static readonly int IdCenterY = Shader.PropertyToID("_CenterY");

    private GameObject  raizUi;
    private Camera      cam;
    private float       orthoOriginal;
    private bool        alterouOrtho;
    private Material    matEscuro;
    private Material    matNevoa;
    private Transform   castelo;

    public void Iniciar(ReliquiaContext contexto)
    {
        cam = contexto.CameraPrincipal;
        if (cam != null && cam.orthographic)
        {
            alterouOrtho  = true;
            orthoOriginal = cam.orthographicSize;
            cam.orthographicSize = orthoOriginal * multiplicadorOrtho;
        }

        castelo = EncontrarCastelo();

        Canvas canvas = contexto.CanvasOverlay;
        if (canvas == null)
            return;

        raizUi = new GameObject("FogGuerra_OlhoQueNadaVe");
        raizUi.transform.SetParent(canvas.transform, false);
        var rtRaiz = raizUi.AddComponent<RectTransform>();
        rtRaiz.anchorMin = Vector2.zero;
        rtRaiz.anchorMax = Vector2.one;
        rtRaiz.offsetMin = Vector2.zero;
        rtRaiz.offsetMax = Vector2.zero;
        rtRaiz.SetAsLastSibling();

        Shader fog = shaderFog != null ? shaderFog : Shader.Find("UI/FogOfWarCutout");
        if (fog == null)
        {
            Debug.LogWarning("[OlhoQueNadaVe] Shader 'UI/FogOfWarCutout' nao encontrado. Atribua o shader no prefab.");
            return;
        }
        matEscuro = CriarMaterial(fog, new Color(0f, 0f, 0f, alphaCamadaEscura));
        matNevoa  = CriarMaterial(fog, new Color(corNevoa.r, corNevoa.g, corNevoa.b, alphaCamadaNevoa));

        CriarCamada(raizUi.transform, "CamadaEscura", matEscuro);
        CriarCamada(raizUi.transform, "CamadaNevoa", matNevoa);

        AtualizarReveal();
    }

    private void Update()
    {
        if (matEscuro != null)
            AtualizarReveal();
    }

    public void Encerrar()
    {
        if (cam != null && alterouOrtho)
            cam.orthographicSize = orthoOriginal;

        cam          = null;
        alterouOrtho = false;
        castelo      = null;

        if (matEscuro != null) { Destroy(matEscuro); matEscuro = null; }
        if (matNevoa  != null) { Destroy(matNevoa);  matNevoa  = null; }

        if (raizUi != null) { DestroyImmediate(raizUi); raizUi = null; }
    }

    private void AtualizarReveal()
    {
        if (castelo == null || cam == null) return;

        Vector3 vp = cam.WorldToViewportPoint(castelo.position);
        matEscuro.SetFloat(IdCenterX, vp.x);
        matEscuro.SetFloat(IdCenterY, vp.y);
        matNevoa.SetFloat(IdCenterX,  vp.x);
        matNevoa.SetFloat(IdCenterY,  vp.y);
    }

    private Material CriarMaterial(Shader fog, Color cor)
    {
        var mat = new Material(fog);
        mat.SetColor("_Color",  cor);
        mat.SetFloat("_Radius", raioReveal);
        mat.SetFloat("_Soft",   suavidadeReveal);
        return mat;
    }

    private static void CriarCamada(Transform pai, string nome, Material mat)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.material      = mat;
        img.color         = Color.white;
        img.raycastTarget = false;
    }

    private static Transform EncontrarCastelo()
    {
        var cc = FindFirstObjectByType<CasteloController>();
        return cc != null ? cc.transform : null;
    }
}
