using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Spawn com deslocamento em linha (arqueiro / feiticeiro). Conta unidades vivas pelo tipo do primeiro MonoBehaviour no prefab de referencia.
/// </summary>
public class BotaoUnidadesSpawnLinhaController : MonoBehaviour
{
    [Header("Configuracao")]
    [SerializeField] private int custoOuro = 100;
    [SerializeField] private GameObject unidadePrefab;
    [SerializeField] private GameObject unidadePrefabFeminina;
    [SerializeField] private string nomeInstancia = "Unidade";
    [SerializeField] private float offsetPorUnidade = 1.5f;
    [SerializeField] private Vector2 direcaoDeslocamento = Vector2.left;

    [Tooltip("Prefab do projeto (raiz) usado so para descobrir o tipo MonoBehaviour a contar na cena.")]
    [SerializeField] private GameObject prefabReferenciaContagem;

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

        int quantidadeAtual = ContarVivos();
        Vector2 posicaoSpawn = ObterCentroCastelo() + direcaoDeslocamento * (quantidadeAtual * offsetPorUnidade);
        GameObject unidade = GameObject.Instantiate(prefabSelecionado, posicaoSpawn, Quaternion.identity);
        if (!string.IsNullOrEmpty(nomeInstancia))
            unidade.name = SeletorPrefabPorSexo.MontarNomeInstancia(nomeInstancia, feminino);

        UnidadeController uc = unidade.GetComponent<UnidadeController>();
        if (uc != null)
            uc.DefinirFaccaoParaSpawn(Faccao.Jogador);
    }

    private int ContarVivos()
    {
        if (prefabReferenciaContagem == null)
            return 0;

        var comp = prefabReferenciaContagem.GetComponent<MonoBehaviour>();
        if (comp == null)
            return 0;

        Type tipo = comp.GetType();
        var objs = UnityEngine.Object.FindObjectsByType(tipo, FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int n = 0;
        foreach (var o in objs)
        {
            if (o is UnidadeController u && u.EstaVivo)
                n++;
        }

        return n;
    }

    private static Vector2 ObterCentroCastelo()
    {
        GameObject castelo = GameObject.FindWithTag("Castelo");
        if (castelo != null)
            return castelo.transform.position;
        return Vector2.zero;
    }
}
