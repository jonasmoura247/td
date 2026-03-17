using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Painel que aparece ao clicar em um slot vazio.
/// Permite escolher raça (4 opções) e gênero do chefe (feminino/masculino).
/// </summary>
public class UICriacaoPerfil : MonoBehaviour
{
    [Header("Raças - 4 botões")]
    [SerializeField] private Button botaoGoblin;
    [SerializeField] private Button botaoElfo;
    [SerializeField] private Button botaoAnao;
    [SerializeField] private Button botaoHumano;

    [Header("Gênero do chefe")]
    [SerializeField] private Toggle toggleFeminino;
    [SerializeField] private Toggle toggleMasculino;
    [SerializeField] private ToggleGroup toggleGroupGenero;

    [Header("Ações")]
    [SerializeField] private Button botaoConfirmar;
    [SerializeField] private Button botaoVoltar;

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI textoInstrucao;

    private TipoRaca racaSelecionada = TipoRaca.Goblin;
    private GeneroChefe generoSelecionado = GeneroChefe.Masculino;
    private int indiceSlot;

    private void Awake()
    {
        if (botaoGoblin != null) botaoGoblin.onClick.AddListener(() => SelecionarRaca(TipoRaca.Goblin));
        if (botaoElfo != null) botaoElfo.onClick.AddListener(() => SelecionarRaca(TipoRaca.Elfo));
        if (botaoAnao != null) botaoAnao.onClick.AddListener(() => SelecionarRaca(TipoRaca.Anao));
        if (botaoHumano != null) botaoHumano.onClick.AddListener(() => SelecionarRaca(TipoRaca.Humano));

        if (toggleFeminino != null) toggleFeminino.onValueChanged.AddListener(OnToggleFeminino);
        if (toggleMasculino != null) toggleMasculino.onValueChanged.AddListener(OnToggleMasculino);

        if (botaoConfirmar != null) botaoConfirmar.onClick.AddListener(Confirmar);
        if (botaoVoltar != null) botaoVoltar.onClick.AddListener(Voltar);
    }

    private void OnEnable()
    {
        racaSelecionada = TipoRaca.Goblin;
        generoSelecionado = GeneroChefe.Masculino;

        if (toggleMasculino != null) toggleMasculino.isOn = true;
        if (toggleFeminino != null) toggleFeminino.isOn = false;

        AtualizarInstrucao();
    }

    /// <summary>Abre o painel para criar perfil no slot indicado.</summary>
    public void Abrir(int slot)
    {
        indiceSlot = slot;
        gameObject.SetActive(true);
    }

    public void Fechar()
    {
        gameObject.SetActive(false);
    }

    private void SelecionarRaca(TipoRaca raca)
    {
        racaSelecionada = raca;
        AtualizarInstrucao();
    }

    private void OnToggleFeminino(bool ativo)
    {
        if (ativo) generoSelecionado = GeneroChefe.Feminino;
    }

    private void OnToggleMasculino(bool ativo)
    {
        if (ativo) generoSelecionado = GeneroChefe.Masculino;
    }

    private void AtualizarInstrucao()
    {
        if (textoInstrucao == null) return;

        string racaStr = racaSelecionada switch
        {
            TipoRaca.Goblin => "Goblin",
            TipoRaca.Elfo => "Elfo",
            TipoRaca.Anao => "Anão",
            TipoRaca.Humano => "Humano",
            _ => "?"
        };

        string generoStr = generoSelecionado == GeneroChefe.Feminino ? "feminino" : "masculino";
        textoInstrucao.text = $"Raça: {racaStr} | Chefe: {generoStr}";
    }

    private void Confirmar()
    {
        if (SaveSlotManager.Instance == null)
        {
            Debug.LogError("[UICriacaoPerfil] SaveSlotManager nao encontrado!");
            return;
        }

        SaveSlotManager.Instance.CriarPerfilEIniciar(indiceSlot, racaSelecionada, generoSelecionado);
    }

    private void Voltar()
    {
        Fechar();
    }
}
