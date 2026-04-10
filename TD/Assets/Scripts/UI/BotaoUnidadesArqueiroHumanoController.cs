using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla o botao de spawn do arqueiro humano.
/// </summary>
public class BotaoUnidadesArqueiroHumanoController : MonoBehaviour
{
    [Header("Configuracao")]
    [SerializeField] private int custoOuro = 100;
    [SerializeField] private GameObject arqueiroPrefab;
    [SerializeField] private GameObject arqueiroPrefabFeminino;
    [SerializeField] private float offsetPorArqueiro = 1.5f;

    [Header("Referencias")]
    [SerializeField] private TextMeshProUGUI textoCusto;
    [SerializeField] private Button botao;

    private void OnEnable()
    {
        if (botao != null)
            botao.onClick.AddListener(TentarSpawnarArqueiro);
        AtualizarTexto();
    }

    private void OnDisable()
    {
        if (botao != null)
            botao.onClick.RemoveListener(TentarSpawnarArqueiro);
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

    private void TentarSpawnarArqueiro()
    {
        if (OuroManager.Instance == null)
            return;

        GameObject prefabSelecionado = SeletorPrefabPorSexo.Selecionar(arqueiroPrefab, arqueiroPrefabFeminino, out bool feminino);
        if (prefabSelecionado == null)
            return;

        if (!OuroManager.Instance.GastarOuro(custoOuro))
            return;

        int quantidadeAtual = ContarArqueirosVivos();
        Vector2 posicaoSpawn = ObterCentroCastelo() + Vector2.left * quantidadeAtual * offsetPorArqueiro;
        GameObject arqueiro = Object.Instantiate(prefabSelecionado, posicaoSpawn, Quaternion.identity);
        arqueiro.name = SeletorPrefabPorSexo.MontarNomeInstancia("HumanoArqueiro", feminino);
    }

    private Vector2 ObterCentroCastelo()
    {
        GameObject castelo = GameObject.FindWithTag("Castelo");
        if (castelo != null)
            return castelo.transform.position;
        return Vector2.zero;
    }

    private int ContarArqueirosVivos()
    {
        var controllers = FindObjectsByType<HumanoArqueiroController>(FindObjectsSortMode.None);
        int vivas = 0;
        foreach (var c in controllers)
        {
            if (c != null && c.EstaVivo)
                vivas++;
        }

        return vivas;
    }
}
