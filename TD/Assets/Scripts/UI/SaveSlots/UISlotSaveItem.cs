using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Componente de um slot individual na tela de seleção.
/// Exibe estado vazio ou preenchido e notifica o pai ao clicar.
/// </summary>
public class UISlotSaveItem : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Button botao;
    [SerializeField] private GameObject painelVazio;
    [SerializeField] private GameObject painelPreenchido;
    [SerializeField] private TextMeshProUGUI textoDescricao;
    [SerializeField] private TextMeshProUGUI textoVazio;

    [Header("Texto slot vazio")]
    [SerializeField] private string textoSlotVazio = "Novo Jogo";

    private int indiceSlot;
    private UISlotsPrincipal painelPai;

    private void Awake()
    {
        if (botao == null)
            botao = GetComponent<Button>();

        botao.onClick.AddListener(AoClicar);
    }

    private void Start()
    {
        GarantirReferencias();
    }

    /// <summary>Preenche referências nulas buscando nos filhos (útil quando slots foram duplicados).</summary>
    private void GarantirReferencias()
    {
        if (painelVazio == null)
        {
            var t = transform.Find("PainelVazio");
            if (t != null) painelVazio = t.gameObject;
        }
        if (painelPreenchido == null)
        {
            var t = transform.Find("PainelPreenchido");
            if (t != null) painelPreenchido = t.gameObject;
        }
        if (textoDescricao == null && painelPreenchido != null)
        {
            textoDescricao = painelPreenchido.GetComponentInChildren<TextMeshProUGUI>(true);
        }
        if (textoVazio == null && painelVazio != null)
        {
            textoVazio = painelVazio.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    /// <summary>Configura o slot. Chamado pelo UISlotsPrincipal.</summary>
    public void Configurar(int indice, UISlotsPrincipal pai)
    {
        indiceSlot = indice;
        painelPai = pai;
    }

    /// <summary>Atualiza a exibição conforme os dados do slot.</summary>
    public void AtualizarVisual(DadosSlotSave dados)
    {
        GarantirReferencias();

        if (dados == null || !dados.ocupado)
        {
            if (painelVazio != null) painelVazio.SetActive(true);
            if (painelPreenchido != null) painelPreenchido.SetActive(false);
            if (textoVazio != null) textoVazio.text = textoSlotVazio;
            return;
        }

        if (painelVazio != null) painelVazio.SetActive(false);
        if (painelPreenchido != null) painelPreenchido.SetActive(true);
        if (textoDescricao != null)
            textoDescricao.text = dados.ObterDescricaoResumida();
    }

    private void AoClicar()
    {
        painelPai?.AoSolicitarSlot(indiceSlot);
    }
}
