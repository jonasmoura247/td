using UnityEngine;
using UnityEngine.UI;

public class PopupQuantidadeDelete : MonoBehaviour
{
    public static PopupQuantidadeDelete Instance;

    [Header("UI")]
    public GameObject painelPopup;
    public Slider sliderQuantidade;
    public Text textoQuantidade;
    public Button botaoConfirmar;
    public Button botaoCancelar;

    private string idItemAtual;

    void Awake()
    {
        Instance = this;
        // Esconde o root inteiro (inclui FundoEscuro + PainelPopup) para evitar fundo preto "preso"
        if (gameObject.activeSelf)
            gameObject.SetActive(false);

        if (sliderQuantidade != null)
            sliderQuantidade.onValueChanged.AddListener(OnSliderChanged);

        if (botaoConfirmar != null)
            botaoConfirmar.onClick.AddListener(OnConfirmar);

        if (botaoCancelar != null)
            botaoCancelar.onClick.AddListener(OnCancelar);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Abrir(string idItem, int quantidadeMax)
    {
        idItemAtual = idItem;

        if (sliderQuantidade != null)
        {
            sliderQuantidade.minValue = 1;
            sliderQuantidade.maxValue = quantidadeMax;
            sliderQuantidade.wholeNumbers = true;
            sliderQuantidade.value = 1;
        }

        AtualizarTextoQuantidade(1);

        // Mostra o root inteiro (FundoEscuro + PainelPopup)
        gameObject.SetActive(true);
    }

    void OnSliderChanged(float valor)
    {
        int qtd = Mathf.RoundToInt(valor);
        AtualizarTextoQuantidade(qtd);
    }

    void AtualizarTextoQuantidade(int qtd)
    {
        if (textoQuantidade != null)
            textoQuantidade.text = qtd.ToString();
    }

    public void OnConfirmar()
    {
        int qtd = 1;
        if (sliderQuantidade != null)
            qtd = Mathf.RoundToInt(sliderQuantidade.value);

        if (LootManager.Instance != null && !string.IsNullOrEmpty(idItemAtual))
            LootManager.Instance.RemoverItem(idItemAtual, qtd);

        Fechar();
    }

    public void OnCancelar()
    {
        Fechar();
    }

    public void Fechar()
    {
        // Esconde o root inteiro (inclui FundoEscuro + PainelPopup)
        gameObject.SetActive(false);
        idItemAtual = null;
    }
}
