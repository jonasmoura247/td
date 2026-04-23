using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Gerencia a interação UI para o modo de patrulha de unidades aliadas.
///
/// Fluxo definição de rota:
///   1. Jogador clica em unidade aliada com <see cref="PatrulhaUnidade"/> que NÃO está patrulhando
///      → botão "Patrulha" aparece.
///   2. Clica no botão → posição atual da unidade vira PontoA; jogo aguarda clique no mapa.
///   3. Clique no mapa → PontoB definido → patrulha inicia automaticamente.
///
/// Fluxo pausa:
///   1. Jogador clica em unidade que JÁ está patrulhando → botão "Pausar/Despausar" aparece.
///   2. Clica no botão → <see cref="PatrulhaUnidade.AlternarPausaPatrulha"/> é chamado.
///
/// Visualização de alcance (ranged):
///   Ao selecionar qualquer unidade ranged (<see cref="SistemaIAUnidade.ehRanged"/>),
///   o <see cref="VisualizadorAlcanceUnidade"/> é ativado e segue a unidade.
///   Escondido ao deselecionar.
///
///   4. Clique direito ou Escape cancela o modo de espera.
/// </summary>
public class ServicoPatrulhaUI : MonoBehaviour
{
    public static ServicoPatrulhaUI Instancia { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject painelPatrulha;  // painel pai — controla visibilidade
    [SerializeField] private Button botaoPatrulha;
    [SerializeField] private Button botaoPausarPatrulha;  // pausar/despausar rota já ativa

    [Header("Detecção de clique em unidades")]
    [SerializeField] private LayerMask layerUnidadesJogador;

    private PatrulhaUnidade _unidadeSelecionada;
    private VisualizadorAlcanceUnidade _visualizadorAtual;
    private bool _aguardandoPontoB;

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;

        if (botaoPatrulha != null)
            botaoPatrulha.onClick.AddListener(AtivarModoPatrulha);

        if (botaoPausarPatrulha != null)
            botaoPausarPatrulha.onClick.AddListener(AlternarPausaPatrulha);

        // Garante que o painel começa escondido
        if (painelPatrulha != null)
            painelPatrulha.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    private void Update()
    {
        if (_aguardandoPontoB)
        {
            HandleModoEsperandoPontoB();
            return;
        }

        if (CliqueEsquerdoNesteFrame() && !PointerSobreUI())
            DetectarCliqueEmUnidade();
    }

    // ─── Modo aguardando ponto B ──────────────────────────────────────────

    private void HandleModoEsperandoPontoB()
    {
        bool cancelar = Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape);
        if (cancelar) { CancelarModo(); return; }

        if (!CliqueEsquerdoNesteFrame() || PointerSobreUI()) return;

        Vector3 pontoMundo = ObterPontoMundo();
        Debug.Log($"[PatrulhaUI] PontoB definido em {pontoMundo}. Patrulha iniciando...");
        _unidadeSelecionada.DefinirPontoB(pontoMundo);
        _aguardandoPontoB = false;
        _unidadeSelecionada = null;
    }

    // ─── Botão Patrulha ───────────────────────────────────────────────────

    private void AtivarModoPatrulha()
    {
        if (_unidadeSelecionada == null) return;
        Debug.Log($"[PatrulhaUI] Botao clicado. PontoA = posicao atual da unidade: {_unidadeSelecionada.transform.position}. Aguardando clique do PontoB...");
        _unidadeSelecionada.DefinirPontoA(_unidadeSelecionada.transform.position);
        _aguardandoPontoB = true;
        MostrarBotao(false);
    }

    // ─── Botão Pausar/Despausar ───────────────────────────────────────────

    private void AlternarPausaPatrulha()
    {
        if (_unidadeSelecionada == null) return;
        _unidadeSelecionada.AlternarPausaPatrulha();
        AtualizarBotoesPausa();
    }

    // ─── Detecção de unidade clicada ─────────────────────────────────────

    private void DetectarCliqueEmUnidade()
    {
        if (ServicoColocacaoUnidade.Instancia != null && ServicoColocacaoUnidade.Instancia.ModoColocacaoAtivo)
            return;

        Vector3 pontoMundo = ObterPontoMundo();

        LayerMask mascara = layerUnidadesJogador.value != 0
            ? layerUnidadesJogador
            : Physics2D.AllLayers;

        Debug.Log($"[PatrulhaUI] Clique em {pontoMundo} | layer mask={mascara.value}");

        Collider2D col = Physics2D.OverlapPoint(pontoMundo, mascara);

        if (col == null)
        {
            Debug.Log("[PatrulhaUI] Nenhum collider encontrado no ponto clicado.");
            DeselecionarUnidade();
            return;
        }

        Debug.Log($"[PatrulhaUI] Collider encontrado: {col.gameObject.name} (layer={LayerMask.LayerToName(col.gameObject.layer)})");

        var patrulha = col.GetComponentInParent<PatrulhaUnidade>();
        if (patrulha == null)
        {
            Debug.Log($"[PatrulhaUI] {col.gameObject.name} nao tem PatrulhaUnidade.");
            DeselecionarUnidade();
            return;
        }

        var uc = col.GetComponentInParent<UnidadeController>();
        if (uc == null || !uc.EstaVivo || uc.faccao != Faccao.Jogador)
        {
            Debug.Log($"[PatrulhaUI] Unidade invalida: uc={uc}, vivo={uc?.EstaVivo}, faccao={uc?.faccao}");
            DeselecionarUnidade();
            return;
        }

        Debug.Log($"[PatrulhaUI] Unidade valida selecionada: {uc.name}. Mostrando painel.");
        _unidadeSelecionada = patrulha;

        // Mostra o círculo de alcance para unidades ranged
        AtualizarVisualizadorAlcance(col);

        MostrarBotao(true);
        AtualizarBotoesPausa();
    }

    private void DeselecionarUnidade()
    {
        EsconderVisualizadorAlcance();
        _unidadeSelecionada = null;
        MostrarBotao(false);
    }

    private void CancelarModo()
    {
        EsconderVisualizadorAlcance();
        _aguardandoPontoB = false;
        _unidadeSelecionada = null;
        MostrarBotao(false);
    }

    /// <summary>
    /// Atualiza a visibilidade dos botões dentro do painel com base no estado atual de patrulha.
    /// - Se estiver patrulhando: mostra botão Pausar/Despausar, esconde botão de definição de rota.
    /// - Se não estiver: mostra botão de definição de rota, esconde Pausar.
    /// </summary>
    private void AtualizarBotoesPausa()
    {
        bool estaPatrulhando = _unidadeSelecionada != null && _unidadeSelecionada.EstaPatrulhando;
        bool estaPausada     = _unidadeSelecionada != null && _unidadeSelecionada.IsPausada;

        if (botaoPatrulha != null)
            botaoPatrulha.gameObject.SetActive(!estaPatrulhando);

        if (botaoPausarPatrulha != null)
        {
            botaoPausarPatrulha.gameObject.SetActive(estaPatrulhando);

            // Atualiza o texto do botão para refletir o estado atual
            var txt = botaoPausarPatrulha.GetComponentInChildren<UnityEngine.UI.Text>();
            if (txt != null) txt.text = estaPausada ? "Despausar" : "Pausar";

            var tmp = botaoPausarPatrulha.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmp != null) tmp.text = estaPausada ? "Despausar" : "Pausar";
        }
    }

    private void MostrarBotao(bool mostrar)
    {
        // Ativa/desativa o PAINEL pai — ativar só o botão não basta se o pai estiver inativo
        if (painelPatrulha != null)
            painelPatrulha.SetActive(mostrar);
        else if (botaoPatrulha != null)
            botaoPatrulha.gameObject.SetActive(mostrar); // fallback se painel não foi atribuído
    }

    // ─── Visualizador de alcance (Ranged) ────────────────────────────────

    private void AtualizarVisualizadorAlcance(Collider2D col)
    {
        EsconderVisualizadorAlcance();

        var ia = col.GetComponentInParent<SistemaIAUnidade>();
        if (ia == null || !ia.ehRanged) return;

        _visualizadorAtual = col.GetComponentInParent<VisualizadorAlcanceUnidade>();
        _visualizadorAtual?.MostrarAlcance();
    }

    private void EsconderVisualizadorAlcance()
    {
        _visualizadorAtual?.EsconderAlcance();
        _visualizadorAtual = null;
    }

    // ─── Utilitários de input ─────────────────────────────────────────────

    private static bool CliqueEsquerdoNesteFrame() => Input.GetMouseButtonDown(0);

    private static Vector3 ObterPontoMundo()
    {
        Camera cam = Camera.main;
        if (cam == null) return Vector3.zero;
        Vector3 screen = Input.mousePosition;
        screen.z = Mathf.Abs(cam.transform.position.z);
        Vector3 w = cam.ScreenToWorldPoint(screen);
        w.z = 0f;
        return w;
    }

    private static bool PointerSobreUI()
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject();
    }
}
