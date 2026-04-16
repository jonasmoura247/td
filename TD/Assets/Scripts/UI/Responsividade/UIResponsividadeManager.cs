using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton central de responsividade de UI.
/// Detecta resolucao, aspect ratio e notifica os demais sistemas quando ha mudanca.
/// Tambem padroniza o CanvasScaler do Canvas principal da cena.
/// </summary>
[DefaultExecutionOrder(-100)]
public class UIResponsividadeManager : MonoBehaviour
{
    // ─── Singleton ───────────────────────────────────────────────────────────
    public static UIResponsividadeManager Instance { get; private set; }

    // ─── Eventos ─────────────────────────────────────────────────────────────
    /// <summary>Disparado sempre que a resolucao ou o aspect ratio muda.</summary>
    public static event Action OnLayoutAtualizado;

    // ─── Configuracao ────────────────────────────────────────────────────────
    [Header("Resolucao de referencia")]
    [Tooltip("Resolucao base para todos os Canvases da cena (recomendado: 1920x1080).")]
    [SerializeField] private Vector2 resolucaoReferencia = new Vector2(1920f, 1080f);

    [Tooltip("Balanco entre escala por largura (0) e altura (1). 0.5 distribui o erro uniformemente.")]
    [Range(0f, 1f)]
    [SerializeField] private float matchWidthOrHeight = 0.5f;

    [Tooltip("Se verdadeiro, aplica automaticamente as configuracoes de CanvasScaler em todos os Canvases da cena no Start.")]
    [SerializeField] private bool aplicarEmTodosCanvases = true;

    // ─── Estado interno ──────────────────────────────────────────────────────
    private int _larguraAnterior;
    private int _alturaAnterior;

    // ─── Propriedades publicas ───────────────────────────────────────────────
    public Vector2 ResolucaoReferencia => resolucaoReferencia;

    /// <summary>Aspect ratio atual da tela (largura / altura).</summary>
    public float AspectRatioAtual => (float)Screen.width / Mathf.Max(Screen.height, 1);

    /// <summary>Perfil de aspect ratio calculado para o tamanho atual de tela.</summary>
    public PerfilAspectRatio PerfilAtual => CalcularPerfil(AspectRatioAtual);

    /// <summary>Fator de escala do Canvas em relacao a resolucao de referencia.</summary>
    public float FatorEscalaAtual
    {
        get
        {
            float escalaW = Screen.width  / resolucaoReferencia.x;
            float escalaH = Screen.height / resolucaoReferencia.y;
            return Mathf.Lerp(escalaW, escalaH, matchWidthOrHeight);
        }
    }

    // ─── Ciclo de vida ───────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _larguraAnterior = Screen.width;
        _alturaAnterior  = Screen.height;

        if (aplicarEmTodosCanvases)
            AplicarEmTodosCanvasesDaCena();

        OnLayoutAtualizado?.Invoke();
    }

    private void Update()
    {
        if (Screen.width != _larguraAnterior || Screen.height != _alturaAnterior)
        {
            _larguraAnterior = Screen.width;
            _alturaAnterior  = Screen.height;
            OnLayoutAtualizado?.Invoke();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ─── API publica ─────────────────────────────────────────────────────────

    /// <summary>
    /// Aplica as configuracoes padrao de CanvasScaler (Scale With Screen Size, 1920x1080, Match 0.5)
    /// em todos os Canvases raiz da cena atual.
    /// </summary>
    public void AplicarEmTodosCanvasesDaCena()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            // Apenas Canvases raiz (sem Canvas pai)
            if (canvas.transform.parent != null && canvas.transform.parent.GetComponentInParent<Canvas>() != null)
                continue;

            AplicarEmCanvas(canvas);
        }
    }

    /// <summary>
    /// Aplica as configuracoes de CanvasScaler em um Canvas especifico.
    /// Nao sobrescreve se ja estiver configurado corretamente.
    /// </summary>
    public void AplicarEmCanvas(Canvas canvas)
    {
        if (canvas == null) return;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode          = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution  = resolucaoReferencia;
        scaler.screenMatchMode      = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight   = matchWidthOrHeight;

        // Pixel perfect causa artefatos em resolucoes nao-multiplas da referencia
        canvas.pixelPerfect = false;
    }

    /// <summary>
    /// Converte uma posicao em pixels da resolucao de referencia para a resolucao atual.
    /// Util para posicionar elementos dinamicamente por script.
    /// </summary>
    public Vector2 EscalarPosicao(Vector2 posicaoNaReferencia)
    {
        float fatorX = Screen.width  / resolucaoReferencia.x;
        float fatorY = Screen.height / resolucaoReferencia.y;
        return new Vector2(posicaoNaReferencia.x * fatorX, posicaoNaReferencia.y * fatorY);
    }

    /// <summary>
    /// Converte um tamanho em pixels da resolucao de referencia para a resolucao atual,
    /// usando o fator de escala uniforme.
    /// </summary>
    public float EscalarTamanho(float tamanhoNaReferencia)
    {
        return tamanhoNaReferencia * FatorEscalaAtual;
    }

    // ─── Perfil de Aspect Ratio ──────────────────────────────────────────────
    public static PerfilAspectRatio CalcularPerfil(float aspect)
    {
        if (aspect >= 2.0f)   return PerfilAspectRatio.Ultrawide;   // 21:9 ou mais largo
        if (aspect >= 1.6f)   return PerfilAspectRatio.Widescreen;  // 16:9, 16:10
        if (aspect >= 1.2f)   return PerfilAspectRatio.Padrao;      // 4:3, 5:4
        if (aspect >= 0.75f)  return PerfilAspectRatio.TabletSquare; // ~1:1, 4:3 retrato
        return PerfilAspectRatio.Portrait;                           // 9:16 e mais alto
    }
}

/// <summary>Perfis de aspect ratio suportados pelo sistema de responsividade.</summary>
public enum PerfilAspectRatio
{
    Portrait,      // 9:16 — mobile retrato
    TabletSquare,  // 3:4  — tablet quadrado/retrato
    Padrao,        // 4:3  — monitores classicos
    Widescreen,    // 16:9 / 16:10 — padrao moderno
    Ultrawide      // 21:9 ou mais — monitores ultrawide
}
