using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Transforma qualquer painel de UI em um HUD deslizante lateral com seta.
///
/// ── Como usar ──────────────────────────────────────────────────────────
///  1. Adicione este componente a um GameObject vazio na cena (ou ao Canvas).
///  2. Arraste o painel desejado (ex: PainelReliquiasHUD, Torres) no campo "Painel Alvo".
///  3. Escolha o lado de onde o painel vai entrar (Esquerda, Direita, Cima, Baixo).
///  4. Rode a cena. O painel começa fechado; clique na seta para abrir/fechar.
///
/// ── Como funciona ──────────────────────────────────────────────────────
///  • O script cria um botão-seta DENTRO do painel, posicionado na borda oposta ao lado.
///  • Ao clicar, o painel desliza para dentro/fora da tela.
///  • O botão é sempre visível pois está na borda do painel (sai da tela junto com ele,
///    mas configurando a posição correta fica sempre visível).
/// </summary>
public class HUDDeslizanteBootstrap : MonoBehaviour
{
    [Header("Painel alvo")]
    [Tooltip("Arraste aqui o painel existente que vai ganhar o efeito de slide (ex: PainelReliquiasHUD).")]
    [SerializeField] private RectTransform painelAlvo;

    [Tooltip("Se vazio, tenta encontrar o painel pelo nome acima automaticamente.")]
    [SerializeField] private string nomePainelAutoDetectar = "";

    [Header("Lado de entrada")]
    [Tooltip("De qual lado da tela o painel entra. Use o mesmo lado onde o painel está atualmente.")]
    [SerializeField] private LadoDeslize lado = LadoDeslize.Esquerda;

    [Header("Seta")]
    [SerializeField] private float larguraSeta = 36f;
    [SerializeField] private float alturaSeta = 72f;
    [SerializeField] private Color corFundoSeta = new Color(0.12f, 0.13f, 0.18f, 0.92f);
    [SerializeField] private string textoFechado = "▶";
    [SerializeField] private string textoAberto  = "◀";

    [Header("Animação")]
    [SerializeField] private float duracao = 0.28f;
    [SerializeField] private AnimationCurve curva = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public enum LadoDeslize { Esquerda, Direita, Cima, Baixo }

    // ── estado ──────────────────────────────────────────────────────────
    private bool     _aberto;
    private Vector2  _posAberta;
    private Vector2  _posFechada;
    private Text     _textoSeta;
    private Coroutine _anim;

    // ────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        ResolverPainelAlvo();
        if (painelAlvo == null)
        {
            Debug.LogError("[HUDDeslizanteBootstrap] Painel alvo não encontrado. " +
                           "Defina 'Painel Alvo' no Inspector ou preencha 'Nome Painel Auto Detectar'.", this);
            enabled = false;
            return;
        }

        // Força recalcular layout para ter medidas corretas
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(painelAlvo);

        CalcularPosicoes();
        CriarBotaoSeta();
        painelAlvo.anchoredPosition = _posFechada;
        _aberto = false;
    }

    // ── resolução do painel ─────────────────────────────────────────────

    private void ResolverPainelAlvo()
    {
        if (painelAlvo != null) return;
        if (string.IsNullOrEmpty(nomePainelAutoDetectar)) return;

        Canvas cv = GetComponentInParent<Canvas>()
                    ?? FindFirstObjectByType<Canvas>();
        if (cv == null) return;

        Transform t = cv.transform.Find(nomePainelAutoDetectar);
        if (t != null)
            painelAlvo = t.GetComponent<RectTransform>();
    }

    // ── cálculo de posições ─────────────────────────────────────────────

    private void CalcularPosicoes()
    {
        _posAberta = painelAlvo.anchoredPosition;

        float w = painelAlvo.rect.width;
        float h = painelAlvo.rect.height;

        _posFechada = lado switch
        {
            LadoDeslize.Esquerda => _posAberta + new Vector2(-w, 0f),
            LadoDeslize.Direita  => _posAberta + new Vector2( w, 0f),
            LadoDeslize.Cima     => _posAberta + new Vector2(0f,  h),
            LadoDeslize.Baixo    => _posAberta + new Vector2(0f, -h),
            _                    => _posAberta,
        };
    }

    // ── criação do botão seta ───────────────────────────────────────────

    private void CriarBotaoSeta()
    {
        // O botão é filho do painel: desliza junto com ele,
        // mas fica posicionado NA BORDA OPOSTA ao lado de entrada,
        // portanto sempre visível na tela mesmo quando o painel está fechado.

        GameObject go = new GameObject("BotaoSeta_HUDSlide", typeof(RectTransform));
        go.transform.SetParent(painelAlvo, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(larguraSeta, alturaSeta);
        PositionarSeta(rt);

        // Fundo
        Image img = go.AddComponent<Image>();
        img.color = corFundoSeta;
        img.raycastTarget = true;

        // Botão
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(AlternarEstado);

        // Texto
        GameObject goTxt = new GameObject("Texto", typeof(RectTransform));
        goTxt.transform.SetParent(go.transform, false);
        RectTransform rtTxt = goTxt.GetComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.offsetMin = rtTxt.offsetMax = Vector2.zero;

        _textoSeta = goTxt.AddComponent<Text>();
        _textoSeta.text         = textoFechado;
        _textoSeta.alignment    = TextAnchor.MiddleCenter;
        _textoSeta.fontSize     = 20;
        _textoSeta.color        = Color.white;
        _textoSeta.font         = BuscarFonte();
        _textoSeta.raycastTarget = false;
    }

    private void PositionarSeta(RectTransform rt)
    {
        // Ancoragem e pivot para cada lado
        switch (lado)
        {
            case LadoDeslize.Esquerda:
                // Painel entra pela esquerda → borda visível é a DIREITA do painel
                rt.anchorMin = new Vector2(1f, 0.5f);
                rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot     = new Vector2(0f, 0.5f);    // pivo na esquerda do botão
                rt.anchoredPosition = Vector2.zero;      // colado à borda direita
                break;

            case LadoDeslize.Direita:
                // Painel entra pela direita → borda visível é a ESQUERDA do painel
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot     = new Vector2(1f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                break;

            case LadoDeslize.Cima:
                // Painel entra por cima → borda visível é a PARTE INFERIOR do painel
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot     = new Vector2(0.5f, 1f);
                rt.anchoredPosition = Vector2.zero;
                break;

            case LadoDeslize.Baixo:
                // Painel entra por baixo → borda visível é o TOPO do painel
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot     = new Vector2(0.5f, 0f);
                rt.anchoredPosition = Vector2.zero;
                break;
        }
    }

    // ── lógica de deslize ───────────────────────────────────────────────

    public void AlternarEstado()
    {
        if (_aberto) Fechar(); else Abrir();
    }

    public void Abrir()
    {
        if (_aberto) return;
        _aberto = true;
        if (_textoSeta != null) _textoSeta.text = textoAberto;
        Animar(_posAberta);
    }

    public void Fechar()
    {
        if (!_aberto) return;
        _aberto = false;
        if (_textoSeta != null) _textoSeta.text = textoFechado;
        Animar(_posFechada);
    }

    private void Animar(Vector2 destino)
    {
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(CoroutineAnimar(destino));
    }

    private IEnumerator CoroutineAnimar(Vector2 destino)
    {
        Vector2 origem = painelAlvo.anchoredPosition;
        float t = 0f;
        while (t < duracao)
        {
            t += Time.unscaledDeltaTime;
            float p = curva.Evaluate(Mathf.Clamp01(t / duracao));
            painelAlvo.anchoredPosition = Vector2.LerpUnclamped(origem, destino, p);
            yield return null;
        }
        painelAlvo.anchoredPosition = destino;
        _anim = null;
    }

    // ── utilitários ─────────────────────────────────────────────────────

    private static Font BuscarFonte()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f != null) return f;
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    // Gizmo visual para ajudar a ver o lado escolhido no editor
    private void OnDrawGizmosSelected()
    {
        if (painelAlvo == null) return;
        Gizmos.color = Color.cyan;
        Vector3 pos = painelAlvo.position;
        Vector3 dir = lado switch
        {
            LadoDeslize.Esquerda => Vector3.left,
            LadoDeslize.Direita  => Vector3.right,
            LadoDeslize.Cima     => Vector3.up,
            LadoDeslize.Baixo    => Vector3.down,
            _                    => Vector3.left,
        };
        Gizmos.DrawRay(pos, dir * 50f);
    }
}
