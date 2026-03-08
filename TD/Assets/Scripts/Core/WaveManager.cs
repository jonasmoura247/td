using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class WaveRacaConfig
    {
        public TipoRaca raca;
        public GameObject[] prefabs;
        public int baseQtd = 1;
        public int incrementoPorWave = 1;
        public bool temBoss = false;
        public GameObject bossPrefab;
    }

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

    [Header("===== CONFIGURACOES POR RACA =====")]
    [SerializeField] private WaveRacaConfig[] configuracoesRacas;

    [Header("===== MODO TESTE =====")]
    [SerializeField] private bool usarQuantidadeFixa = false;
    [SerializeField] private int quantidadeFixaTeste = 10;

    private readonly List<GameObject> inimigosVivos = new List<GameObject>();

    private bool waveAtiva = false;
    private int numeroWave = 0;
    private Coroutine rotinaSpawn;

    private TipoRaca proximaRaca;
    private WaveRacaConfig configAtual;

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
        if (configuracoesRacas == null || configuracoesRacas.Length == 0)
        {
            Debug.LogError("Nenhuma configuracao de raca foi atribuida ao WaveManager.");
            proximaRaca = default;
            AtualizarTextoProximaWave();
            return;
        }

        int index = Random.Range(0, configuracoesRacas.Length);
        configAtual = configuracoesRacas[index];
        proximaRaca = configAtual != null ? configAtual.raca : default;

        AtualizarTextoProximaWave();
    }

    private void AtualizarTextoProximaWave()
    {
        if (textoProximaWave != null)
            textoProximaWave.text = "Proxima Wave: " + proximaRaca.ToString();
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
        if (configAtual == null)
        {
            Debug.LogError("Configuracao atual de wave esta nula. Verifique o WaveManager.");
            yield break;
        }

        if (configAtual.prefabs == null || configAtual.prefabs.Length == 0)
        {
            Debug.LogError($"Configuracao da raca {configAtual.raca} nao possui prefabs atribuidos.");
            yield break;
        }

        int quantidade = usarQuantidadeFixa
            ? quantidadeFixaTeste
            : configAtual.baseQtd + (numeroWave * configAtual.incrementoPorWave);

        if (configAtual.temBoss && configAtual.bossPrefab != null)
        {
            Spawnar(configAtual.bossPrefab);
            yield return new WaitForSeconds(intervaloEntreSpawns);
        }

        for (int i = 0; i < quantidade; i++)
        {
            GameObject prefab = configAtual.prefabs[Random.Range(0, configAtual.prefabs.Length)];
            Spawnar(prefab);
            yield return new WaitForSeconds(intervaloEntreSpawns);
        }
    }

    private void Spawnar(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Tentativa de spawnar um prefab nulo.");
            return;
        }

        if (rotasPai == null || rotasPai.childCount == 0)
        {
            Debug.LogWarning("RotasPai nao configurado!");
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