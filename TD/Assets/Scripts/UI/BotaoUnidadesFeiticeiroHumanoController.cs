using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla o botao de spawn do feiticeiro humano.
/// </summary>
public class BotaoUnidadesFeiticeiroHumanoController : MonoBehaviour
{
    [Header("Configuracao")]
    [SerializeField] private int custoOuro = 110;
    [SerializeField] private GameObject feiticeiroPrefab;
    [SerializeField] private GameObject feiticeiroPrefabFeminino;
    [SerializeField] private float offsetPorFeiticeira = 1.5f;

    [Header("Referencias")]
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
        if (OuroManager.Instance == null)
            return;

        GameObject prefabSelecionado = SeletorPrefabPorSexo.Selecionar(feiticeiroPrefab, feiticeiroPrefabFeminino, out bool feminino);
        if (prefabSelecionado == null)
            return;

        if (!OuroManager.Instance.GastarOuro(custoOuro))
            return;

        int quantidadeAtual = ContarFeiticeirasVivas();
        Vector2 posicaoSpawn = ObterCentroCastelo() + Vector2.right * quantidadeAtual * offsetPorFeiticeira;
        GameObject feiticeiro = Object.Instantiate(prefabSelecionado, posicaoSpawn, Quaternion.identity);
        feiticeiro.name = SeletorPrefabPorSexo.MontarNomeInstancia("HumanoFeiticeira", feminino);
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
