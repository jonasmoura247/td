using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Spawn do mineiro no centro do castelo (mesma logica para todas as racas: prefab com <see cref="MineiroAliadoController"/>).
/// </summary>
public class BotaoUnidadesMineiroHumanoController : MonoBehaviour
{
    [Header("Configuracao")]
    [SerializeField] private int custoOuro = 70;
    [SerializeField] private GameObject mineiroPrefab;
    [SerializeField] private GameObject mineiroPrefabFeminino;
    [SerializeField] private string nomeInstancia = "HumanoMineiro";

    [Header("Referencias")]
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
        if (OuroManager.Instance == null)
            return;

        GameObject prefabSelecionado = SeletorPrefabPorSexo.Selecionar(mineiroPrefab, mineiroPrefabFeminino, out bool feminino);
        if (prefabSelecionado == null)
            return;

        if (!OuroManager.Instance.GastarOuro(custoOuro))
            return;

        Vector2 posicaoSpawn = ObterCentroCastelo();
        GameObject mineiro = GameObject.Instantiate(prefabSelecionado, posicaoSpawn, Quaternion.identity);
        if (!string.IsNullOrEmpty(nomeInstancia))
            mineiro.name = SeletorPrefabPorSexo.MontarNomeInstancia(nomeInstancia, feminino);
    }

    private Vector2 ObterCentroCastelo()
    {
        GameObject castelo = GameObject.FindWithTag("Castelo");
        if (castelo != null)
            return castelo.transform.position;
        return Vector2.zero;
    }
}
