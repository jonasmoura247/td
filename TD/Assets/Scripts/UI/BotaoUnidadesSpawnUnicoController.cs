using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Spawn de uma unidade no centro do castelo (ex.: mineiro). Configuravel por inspector para qualquer raca.
/// </summary>
public class BotaoUnidadesSpawnUnicoController : MonoBehaviour
{
    [Header("Configuracao")]
    [SerializeField] private int custoOuro = 70;
    [SerializeField] private GameObject unidadePrefab;
    [SerializeField] private GameObject unidadePrefabFeminina;
    [SerializeField] private string nomeInstancia = "Unidade";

    [Header("Referencias")]
    [SerializeField] private TextMeshProUGUI textoCusto;
    [SerializeField] private Button botao;

    private void OnEnable()
    {
        if (botao != null)
            botao.onClick.AddListener(TentarSpawnar);
        AtualizarTexto();
    }

    private void OnDisable()
    {
        if (botao != null)
            botao.onClick.RemoveListener(TentarSpawnar);
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

    private void TentarSpawnar()
    {
        if (OuroManager.Instance == null)
            return;

        GameObject prefabSelecionado = SeletorPrefabPorSexo.Selecionar(unidadePrefab, unidadePrefabFeminina, out bool feminino);
        if (prefabSelecionado == null)
            return;

        if (!OuroManager.Instance.GastarOuro(custoOuro))
            return;

        Vector2 posicaoSpawn = ObterCentroCastelo();
        GameObject unidade = GameObject.Instantiate(prefabSelecionado, posicaoSpawn, Quaternion.identity);
        if (!string.IsNullOrEmpty(nomeInstancia))
            unidade.name = SeletorPrefabPorSexo.MontarNomeInstancia(nomeInstancia, feminino);
    }

    private static Vector2 ObterCentroCastelo()
    {
        GameObject castelo = GameObject.FindWithTag("Castelo");
        if (castelo != null)
            return castelo.transform.position;
        return Vector2.zero;
    }
}
