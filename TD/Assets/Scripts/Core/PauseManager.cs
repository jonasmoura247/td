using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gerencia o estado de pausa global do jogo.
/// Singleton desacoplado: define Time.timeScale e dispara evento OnPauseAlterado.
/// Coloque um GameObject com este componente em cada cena jogável.
/// </summary>
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    /// <summary>Disparado com <c>true</c> ao pausar e <c>false</c> ao retomar.</summary>
    public static event Action<bool> OnPauseAlterado;

    public bool EstaPausado { get; private set; }

    [Header("Configuração")]
    [Tooltip("Se verdadeiro, Escape alterna o pause automaticamente.")]
    [SerializeField] private bool usarTeclaEscape = true;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            OnPauseAlterado = null;
        }
    }

    private void Update()
    {
        if (usarTeclaEscape && Input.GetKeyDown(KeyCode.Escape))
            AlternarPause();
    }

    public void AlternarPause()
    {
        if (EstaPausado)
            Retomar();
        else
            Pausar();
    }

    public void Pausar()
    {
        if (EstaPausado) return;
        EstaPausado = true;
        Time.timeScale = 0f;
        OnPauseAlterado?.Invoke(true);
    }

    public void Retomar()
    {
        if (!EstaPausado) return;
        EstaPausado = false;
        Time.timeScale = 1f;
        OnPauseAlterado?.Invoke(false);
    }

    /// <summary>Reinicia a cena atual (usado pelo botão Reiniciar do menu de pause).</summary>
    public void Reiniciar()
    {
        EstaPausado = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Volta para o lobby da raça ativa (somente fora do lobby).</summary>
    public void VoltarParaLobby()
    {
        EstaPausado = false;
        Time.timeScale = 1f;

        SaveSlotManager.GarantirExistencia();
        TipoRaca raca = PerfilJogadorAtivo.TemPerfilAtivo ? PerfilJogadorAtivo.Raca : TipoRaca.Goblin;

        if (SaveSlotManager.Instance != null)
        {
            string cena = SaveSlotManager.Instance.ObterCenaLobby(raca);
            if (!string.IsNullOrEmpty(cena))
            {
                SceneManager.LoadScene(cena);
                return;
            }
        }

        Debug.LogError("[PauseManager] Não foi possível determinar o lobby da raça ativa.");
    }

    /// <summary>Vai para a tela de seleção de slots (sair da partida sem fechar o app).</summary>
    public void SairDaPartida()
    {
        EstaPausado = false;
        Time.timeScale = 1f;
        PerfilJogadorAtivo.LimparPerfil();
        SceneManager.LoadScene("SelecaoSlots");
    }

    /// <summary>Encerra o aplicativo (ou para o Play Mode no editor).</summary>
    public void SairDoJogo()
    {
        EstaPausado = false;
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
