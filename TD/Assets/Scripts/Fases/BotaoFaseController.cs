using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Botão de fase no mapa. Configure via Inspector e posicione livremente na cena.
/// faseId deve ser único por fase (ex: "natural_1", "natural_2", ...).
/// fasePreRequisito deve ser o faseId da fase anterior (vazio = sempre desbloqueada).
/// </summary>
public class BotaoFaseController : MonoBehaviour
{
    [Header("Configuração da Fase")]
    [SerializeField] private string faseId;
    [SerializeField] private string nomeCena = "Mapa_1";
    [SerializeField] private string fasePreRequisito;
    [SerializeField] private int mundoId = 1;
    [SerializeField] private int numeroDaFase = 1;

    [Header("Visual")]
    [SerializeField] private Color corBloqueado = new Color(0.35f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color corDisponivel = Color.white;
    [SerializeField] private Color corVencido = new Color(1f, 0.85f, 0.2f, 1f);

    private Button botao;
    private Image imagem;

    public int MundoId => mundoId;
    public int NumeroDaFase => numeroDaFase;
    public string NomeCena => nomeCena;
    public string FaseId => faseId;

    private void Start()
    {
        botao = GetComponent<Button>();
        imagem = GetComponent<Image>();
        AtualizarEstado();
    }

    private void AtualizarEstado()
    {
        var progressao = ObterProgressao();
        bool desbloqueada = progressao?.FaseDesbloqueada(fasePreRequisito) ?? string.IsNullOrEmpty(fasePreRequisito);
        bool vencida = progressao?.FaseVencida(faseId) ?? false;

        if (botao != null)
            botao.interactable = desbloqueada;

        if (imagem != null)
        {
            if (!desbloqueada)
                imagem.color = corBloqueado;
            else if (vencida)
                imagem.color = corVencido;
            else
                imagem.color = corDisponivel;
        }
    }

    /// <summary>Chamado pelo OnClick do botão.</summary>
    public void TentarEntrar()
    {
        var progressao = ObterProgressao();
        bool desbloqueada = progressao?.FaseDesbloqueada(fasePreRequisito) ?? string.IsNullOrEmpty(fasePreRequisito);

        if (!desbloqueada)
        {
            Debug.Log($"[BotaoFaseController] Fase {faseId} bloqueada. Vença '{fasePreRequisito}' primeiro.");
            return;
        }

        FaseIdentificacao.FaseAtualId = faseId;
        Time.timeScale = 1f;
        SceneManager.LoadScene(nomeCena);
    }

    private ProgressaoFases ObterProgressao()
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || PerfilJogadorAtivo.Dados == null)
            return null;
        return PerfilJogadorAtivo.Dados.progressaoFases;
    }
}
