using UnityEngine;
using UnityEngine.SceneManagement;

public enum EstadoBandeira { Bloqueada, Disponivel, Vencida }

/// <summary>
/// Objeto de bandeira no world space. Posicione livremente no mapa.
/// Detecta clique via OnMouseDown (requer BoxCollider2D + câmera 2D padrão).
/// Troca o sprite de acordo com a raça ativa do jogador.
/// [ExecuteAlways] garante que o sprite aparece no Scene View sem precisar dar Play.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class BandeiraFaseController : MonoBehaviour
{
    [Header("Fase")]
    [SerializeField] private string faseId;
    [SerializeField] private string nomeCena = "Mapa_1";
    [SerializeField] private string fasePreRequisito;
    [SerializeField] private int mundoId = 1;
    [SerializeField] private int indiceFase = 1;

    [Header("Bandeiras por Raça")]
    [SerializeField] private Sprite bandeiraGoblin;
    [SerializeField] private Sprite bandeiraElfo;
    [SerializeField] private Sprite bandeiraAnao;
    [SerializeField] private Sprite bandeiraHumano;

    [Header("Estados Visuais")]
    [SerializeField] private Color corBloqueado = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color corVencido = new Color(1f, 0.85f, 0.2f, 1f);

    [Header("Referências (opcional)")]
    [Tooltip("Filho com sprite de cadeado. Ativado quando a fase está bloqueada.")]
    [SerializeField] private GameObject iconeLock;

    [Header("Preview no Editor")]
    [Tooltip("Raça exibida no Scene View para facilitar o posicionamento. Não afeta o jogo.")]
    [SerializeField] private TipoRaca racaPreviewEditor = TipoRaca.Goblin;

    // Injetável em testes para interceptar o carregamento de cena
    public System.Action<string> CarregarCena;

    public string FaseId => faseId;
    public string NomeCena => nomeCena;
    public int MundoId => mundoId;
    public int IndiceFase => indiceFase;

    private SpriteRenderer sr;
    private Collider2D col;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        CarregarCena = SceneManager.LoadScene;
    }

    private void Start()
    {
        AtualizarSpritePorRaca();
        AtualizarEstado();
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        Sprite preview = ObterBandeiraParaRaca(racaPreviewEditor);
        if (preview != null && sr != null)
            sr.sprite = preview;
    }

    private void OnMouseDown() => TentarEntrar();

    // ── API pública (também usada por testes) ─────────────────────────────

    public void TentarEntrar()
    {
        if (ObterEstado() == EstadoBandeira.Bloqueada)
        {
            Debug.Log($"[BandeiraFase] '{faseId}' bloqueada. Vença '{fasePreRequisito}' primeiro.");
            return;
        }

        FaseIdentificacao.FaseAtualId = faseId;
        Time.timeScale = 1f;
        CarregarCena?.Invoke(nomeCena);
    }

    public EstadoBandeira ObterEstado()
    {
        if (!EstaDesbloqueada()) return EstadoBandeira.Bloqueada;
        if (EstaVencida())       return EstadoBandeira.Vencida;
        return EstadoBandeira.Disponivel;
    }

    public void AtualizarEstado()
    {
        var estado = ObterEstado();

        if (col != null)
            col.enabled = estado != EstadoBandeira.Bloqueada;

        if (sr != null)
        {
            sr.color = estado switch
            {
                EstadoBandeira.Bloqueada  => corBloqueado,
                EstadoBandeira.Vencida    => corVencido,
                _                         => Color.white
            };
        }

        if (iconeLock != null)
            iconeLock.SetActive(estado == EstadoBandeira.Bloqueada);
    }

    public void AtualizarSpritePorRaca()
    {
        Sprite bandeira = ObterBandeiraParaRaca(PerfilJogadorAtivo.Raca);
        if (bandeira != null && sr != null)
            sr.sprite = bandeira;
    }

    public Sprite ObterBandeiraParaRaca(TipoRaca raca) => raca switch
    {
        TipoRaca.Goblin  => bandeiraGoblin,
        TipoRaca.Elfo    => bandeiraElfo,
        TipoRaca.Anao    => bandeiraAnao,
        TipoRaca.Humano  => bandeiraHumano,
        _                => null
    };

    // ── Lógica interna ────────────────────────────────────────────────────

    private bool EstaDesbloqueada()
    {
        var p = ObterProgressao();
        return p?.FaseDesbloqueada(fasePreRequisito) ?? string.IsNullOrEmpty(fasePreRequisito);
    }

    private bool EstaVencida() => ObterProgressao()?.FaseVencida(faseId) ?? false;

    private ProgressaoFases ObterProgressao()
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || PerfilJogadorAtivo.Dados == null)
            return null;
        return PerfilJogadorAtivo.Dados.progressaoFases;
    }
}
