using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Spawn do mineiro no centro do castelo (mesma lógica para todas as raças: prefab com <see cref="MineiroAliadoController"/>).
/// </summary>
public class BotaoUnidadesMineiroHumanoController : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField] private int custoOuro = 70;
    [SerializeField] private GameObject mineiroPrefab;
    [SerializeField] private string nomeInstancia = "HumanoMineiro";

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
        GameObject mineiro = GameObject.Instantiate(mineiroPrefab, posicaoSpawn, Quaternion.identity);
        if (!string.IsNullOrEmpty(nomeInstancia))
            mineiro.name = nomeInstancia;
    }

    private Vector2 ObterCentroCastelo()
    {
        GameObject castelo = GameObject.FindWithTag("Castelo");
        if (castelo != null)
            return castelo.transform.position;
        return Vector2.zero;
    }
}
