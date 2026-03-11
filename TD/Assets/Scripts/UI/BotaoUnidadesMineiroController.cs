using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla o botão de spawn do mineiro humano. Ao clicar, spawna um mineiro no centro do castelo se houver ouro suficiente.
/// </summary>
public class BotaoUnidadesMineiroController : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField] private int custoOuro = 70;
    [SerializeField] private GameObject mineiroPrefab;

    [Header("Referências")]
    [SerializeField] private TextMeshProUGUI textoCusto;
    [SerializeField] private Button botao;

    private void OnEnable()
    {
        if (botao != null)
            botao.onClick.AddListener(TentarSpawnarMineiro);
        AtualizarTexto();
    }

    private void OnDisable()
    {
        if (botao != null)
            botao.onClick.RemoveListener(TentarSpawnarMineiro);
    }

    private void Start()
    {
        AtualizarTexto();
    }

    private void AtualizarTexto()
    {
        if (textoCusto != null)
            textoCusto.text = custoOuro.ToString();
    }

    private void TentarSpawnarMineiro()
    {
        if (OuroManager.Instance == null) return;
        if (mineiroPrefab == null) return;

        if (!OuroManager.Instance.GastarOuro(custoOuro))
            return;

        Vector2 posicaoSpawn = ObterCentroCastelo();
        GameObject mineiro = Object.Instantiate(mineiroPrefab, posicaoSpawn, Quaternion.identity);
        mineiro.name = "HumanoMineiro";
    }

    private Vector2 ObterCentroCastelo()
    {
        GameObject castelo = GameObject.FindWithTag("Castelo");
        if (castelo != null)
            return castelo.transform.position;
        return Vector2.zero;
    }
}
