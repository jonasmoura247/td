using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour
{
    [Header("===== TIMER =====")]
    [SerializeField] private float tempoEntreWaves = 60f;
    private float tempoAtual;

    [SerializeField] private TextMeshProUGUI textoTimer;
    [SerializeField] private TextMeshProUGUI textoProximaWave;

    [SerializeField] private Button botaoPlay;
    [SerializeField] private Button botaoReiniciar;

    [Header("===== SPAWN =====")]
    [SerializeField] private float intervaloEntreSpawns = 0.3f;
    [SerializeField] private float offsetSpawn = 0.2f;

    [Header("===== ROTAS =====")]
    [SerializeField] private Transform rotasPai;

    [Header("===== PREFABS =====")]
    [SerializeField] private GameObject goblinGuerreiro;
    [SerializeField] private GameObject goblinLanceiro;
    [SerializeField] private GameObject elfoMago;

    [Header("===== CONFIGURAÇÃO QUANTIDADE GOBLIN =====")]
    [SerializeField] private int goblinBase = 3;
    [SerializeField] private int goblinPorWave = 2;

    [Header("===== CONFIGURAÇÃO QUANTIDADE ELFO =====")]
    [SerializeField] private int elfoBase = 1;
    [SerializeField] private int elfoPorWave = 1;

    [Header("===== MODO TESTE =====")]
    [SerializeField] private bool usarQuantidadeFixa = false;
    [SerializeField] private int quantidadeFixaTeste = 10;

    private List<GameObject> inimigosVivos = new List<GameObject>();

    private bool waveAtiva = false;
    private int numeroWave = 0;
    private Coroutine rotinaSpawn;

    private TipoRaca proximaRaca;

    private void Start()
    {
        tempoAtual = tempoEntreWaves;

        SortearProximaWave();

        botaoPlay.onClick.AddListener(ForcarInicioWave);
        botaoPlay.interactable = true;

        if (botaoReiniciar != null)
            botaoReiniciar.onClick.AddListener(ReiniciarPartida);
    }

    private void Update()
    {
        if (waveAtiva) return;

        tempoAtual -= Time.deltaTime;

        if (textoTimer != null)
            textoTimer.text = Mathf.CeilToInt(tempoAtual).ToString();

        if (tempoAtual <= 0)
        {
            IniciarWave();
        }
    }

    private void SortearProximaWave()
    {
        TipoRaca[] racas =
        {
            TipoRaca.Goblin,
            TipoRaca.Elfo
        };

        int index = Random.Range(0, racas.Length);
        proximaRaca = racas[index];

        AtualizarTextoProximaWave();
    }

    private void AtualizarTextoProximaWave()
    {
        if (textoProximaWave != null)
            textoProximaWave.text = "Próxima Wave: " + proximaRaca.ToString();
    }

    public void ForcarInicioWave()
    {
        if (waveAtiva) return;

        tempoAtual = 0;
        IniciarWave();
    }

    private void IniciarWave()
    {
        if (waveAtiva) return;

        waveAtiva = true;
        numeroWave++;

        botaoPlay.interactable = false;

        rotinaSpawn = StartCoroutine(GerarWaveComDelay());
    }

    private IEnumerator GerarWaveComDelay()
    {
        int quantidade;

        if (proximaRaca == TipoRaca.Goblin)
        {
            quantidade = usarQuantidadeFixa
                ? quantidadeFixaTeste
                : goblinBase + (numeroWave * goblinPorWave);

            for (int i = 0; i < quantidade; i++)
            {
                GameObject prefab = Random.value > 0.5f ? goblinGuerreiro : goblinLanceiro;

                Spawnar(prefab);
                yield return new WaitForSeconds(intervaloEntreSpawns);
            }
        }
        else if (proximaRaca == TipoRaca.Elfo)
        {
            quantidade = usarQuantidadeFixa
                ? quantidadeFixaTeste
                : elfoBase + (numeroWave * elfoPorWave);

            for (int i = 0; i < quantidade; i++)
            {
                Spawnar(elfoMago);
                yield return new WaitForSeconds(intervaloEntreSpawns);
            }
        }
    }

    private void Spawnar(GameObject prefab)
    {
        if (rotasPai == null || rotasPai.childCount == 0)
        {
            Debug.LogWarning("RotasPai não configurado!");
            return;
        }

        int rotaIndex = Random.Range(0, rotasPai.childCount);
        Transform rota = rotasPai.GetChild(rotaIndex);

        if (rota.childCount == 0)
        {
            Debug.LogWarning("Rota sem pontos!");
            return;
        }

        Transform pontoInicial = rota.GetChild(0);

        Vector2 offset = Random.insideUnitCircle * offsetSpawn;

        GameObject inimigo = Instantiate(
            prefab,
            pontoInicial.position + (Vector3)offset,
            Quaternion.identity
        );

        MovimentoUnidade mov = inimigo.GetComponent<MovimentoUnidade>();
        if (mov != null)
            mov.DefinirRota(rota);

        inimigosVivos.Add(inimigo);
        StartCoroutine(MonitorarMorte(inimigo));
    }

    private IEnumerator MonitorarMorte(GameObject inimigo)
    {
        while (inimigo != null)
            yield return null;

        inimigosVivos.Remove(inimigo);

        if (inimigosVivos.Count == 0 && waveAtiva)
            FinalizarWave();
    }

    private void FinalizarWave()
    {
        waveAtiva = false;
        tempoAtual = tempoEntreWaves;
        botaoPlay.interactable = true;

        SortearProximaWave();
    }

    public void ReiniciarPartida()
    {
        Time.timeScale = 1f;

        if (rotinaSpawn != null)
            StopCoroutine(rotinaSpawn);

        StopAllCoroutines();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}