using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla o botão de spawn do feiticeiro humano. Placeholder - funcionalidade a ser implementada.
/// </summary>
public class BotaoUnidadesFeiticeiroHumanoController : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField] private int custoOuro = 110;
    [SerializeField] private GameObject feiticeiroPrefab;
    [SerializeField] private float offsetPorFeiticeira = 1.5f;

    [Header("Referências")]
    [SerializeField] private TextMeshProUGUI textoCusto;
    [SerializeField] private Button botao;

    private void OnEnable()
    {
        if (botao != null)
            botao.onClick.AddListener(TentarSpawnarFeiticeiro);
        AtualizarTexto();
    }

    private void OnDisable()
    {
        if (botao != null)
            botao.onClick.RemoveListener(TentarSpawnarFeiticeiro);
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

    private void TentarSpawnarFeiticeiro()
    {
        if (OuroManager.Instance == null) return;
        if (feiticeiroPrefab == null) return;

        if (!OuroManager.Instance.GastarOuro(custoOuro))
            return;

        int quantidadeAtual = ContarFeiticeirasVivas();
        Vector2 posicaoSpawn = ObterCentroCastelo() + Vector2.right * quantidadeAtual * offsetPorFeiticeira;
        GameObject feiticeiro = Object.Instantiate(feiticeiroPrefab, posicaoSpawn, Quaternion.identity);
        feiticeiro.name = "HumanoFeiticeira";
    }

    private Vector2 ObterCentroCastelo()
    {
        GameObject castelo = GameObject.FindWithTag("Castelo");
        if (castelo != null)
            return castelo.transform.position;
        return Vector2.zero;
    }

    private int ContarFeiticeirasVivas()
    {
        var controllers = FindObjectsByType<HumanoFeiticeiraController>(FindObjectsSortMode.None);
        int vivas = 0;
        foreach (var c in controllers)
        {
            if (c != null && c.EstaVivo)
                vivas++;
        }
        return vivas;
    }
}
