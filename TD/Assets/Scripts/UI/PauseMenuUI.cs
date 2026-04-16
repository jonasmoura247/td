using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla a UI do menu de pause.
/// Assine PauseManager.OnPauseAlterado para mostrar/ocultar o painel.
///
/// Configuração na cena:
///   1. Crie um Canvas (Screen Space - Overlay, ordem alta).
///   2. Dentro, crie um painel centralizado e atribua a painelPause.
///   3. Adicione este componente a qualquer GameObject na cena.
///   4. Atribua os botões pelos campos SerializeField.
///   5. Marque estaNoLobby = true nas cenas de lobby (oculta "Voltar ao Lobby").
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("Painel raiz")]
    [SerializeField] private GameObject painelPause;

    [Header("Botões")]
    [SerializeField] private Button botaoRetomar;
    [SerializeField] private Button botaoReiniciar;
    [SerializeField] private Button botaoVoltarLobby;
    [SerializeField] private Button botaoSairPartida;
    [SerializeField] private Button botaoSairJogo;

    [Header("Configuração da cena")]
    [Tooltip("Marque true nas cenas de lobby. Oculta o botão 'Voltar ao Lobby'.")]
    [SerializeField] private bool estaNoLobby = false;

    private void Awake()
    {
        // Garante que o painel começa oculto
        if (painelPause != null)
            painelPause.SetActive(false);
    }

    private void Start()
    {
        // Botão Voltar ao Lobby visível apenas fora do lobby
        if (botaoVoltarLobby != null)
            botaoVoltarLobby.gameObject.SetActive(!estaNoLobby);

        // Wire-up dos botões
        botaoRetomar?.onClick.AddListener(OnRetomar);
        botaoReiniciar?.onClick.AddListener(OnReiniciar);
        botaoVoltarLobby?.onClick.AddListener(OnVoltarLobby);
        botaoSairPartida?.onClick.AddListener(OnSairPartida);
        botaoSairJogo?.onClick.AddListener(OnSairJogo);

        PauseManager.OnPauseAlterado += AtualizarVisibilidade;
    }

    private void OnDestroy()
    {
        PauseManager.OnPauseAlterado -= AtualizarVisibilidade;
    }

    private void AtualizarVisibilidade(bool pausado)
    {
        if (painelPause != null)
            painelPause.SetActive(pausado);
    }

    // ─── Handlers ──────────────────────────────────────────────────────────

    private void OnRetomar()
    {
        PauseManager.Instance?.Retomar();
    }

    private void OnReiniciar()
    {
        PauseManager.Instance?.Reiniciar();
    }

    private void OnVoltarLobby()
    {
        PauseManager.Instance?.VoltarParaLobby();
    }

    private void OnSairPartida()
    {
        PauseManager.Instance?.SairDaPartida();
    }

    private void OnSairJogo()
    {
        PauseManager.Instance?.SairDoJogo();
    }

    // ─── Botão de pause na HUD (pode ser chamado diretamente por um Button) ─

    /// <summary>Chame este método no OnClick do botão de pause da HUD.</summary>
    public void BotaoPauseClicado()
    {
        PauseManager.Instance?.AlternarPause();
    }
}
