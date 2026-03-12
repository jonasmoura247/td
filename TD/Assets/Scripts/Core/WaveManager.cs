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

    [System.Serializable]
    public class WaveConfig
    {
        [Tooltip("Se Aleatoria: escolhe criaturas disponíveis da raça. Se Fixa: usa prefabs específicos.")]
        public ModoWave modoWave = ModoWave.Fixa;

        [Tooltip("Lado do mapa de onde todos os inimigos da wave surgirão.")]
        public LadoSpawn ladoSpawn = LadoSpawn.Esquerda;

        [Tooltip("Quantidade fixa de bosses nesta wave.")]
        public int quantidadeBoss = 0;

        [Tooltip("Prefab do boss utilizado (obrigatório se quantidadeBoss > 0).")]
        public GameObject bossPrefab;

        [Tooltip("Quantidade fixa de inimigos comuns nesta wave.")]
        public int quantidadeComuns = 0;

        [Tooltip("Prefabs específicos para modo Fixa. Usados em sequência/ciclo. Ignorado no modo Aleatoria.")]
        public GameObject[] prefabsComuns;
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

    [Header("===== CONFIGURACOES DE WAVES =====")]
    [Tooltip("Configuração de cada wave. Índice 0 = Wave 1, índice 1 = Wave 2, etc. Se vazio, usa config padrão.")]
    [SerializeField] private WaveConfig[] configuracoesWaves;

    [Header("===== CONFIGURACOES POR RACA (para modo Aleatoria) =====")]
    [Tooltip("Usado quando modoWave é Aleatoria. Por enquanto: apenas Goblin.")]
    [SerializeField] private WaveRacaConfig[] configuracoesRacas;

    private readonly List<GameObject> inimigosVivos = new List<GameObject>();

    private bool waveAtiva = false;
    private bool spawnDestaWaveConcluido = false;
    private int numeroWave = 0;
    private Coroutine rotinaSpawn;

    private WaveConfig configWaveAtual;
    private WaveRacaConfig configRacaAtual;

    private void Start()
    {
        tempoAtual = tempoEntreWaves;

        DefinirProximaWave();

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

    private void DefinirProximaWave()
    {
        configWaveAtual = ObterConfigWave(numeroWave + 1);
        configRacaAtual = ObterConfigRacaGoblin();

        AtualizarTextoProximaWave();
    }

    private WaveConfig ObterConfigWave(int waveNumero)
    {
        if (configuracoesWaves != null && waveNumero > 0 && waveNumero <= configuracoesWaves.Length)
        {
            WaveConfig config = configuracoesWaves[waveNumero - 1];
            if (config != null)
                return config;
        }

        return CriarConfigPadrao(waveNumero);
    }

    private WaveConfig CriarConfigPadrao(int waveNumero)
    {
        WaveRacaConfig raca = ObterConfigRacaGoblin();
        if (raca == null)
            return new WaveConfig();

        return new WaveConfig
        {
            modoWave = ModoWave.Aleatoria,
            ladoSpawn = (LadoSpawn)Random.Range(0, 4),
            quantidadeBoss = raca.temBoss ? 1 : 0,
            bossPrefab = raca.bossPrefab,
            quantidadeComuns = Mathf.Max(0, raca.baseQtd + (waveNumero * raca.incrementoPorWave)),
            prefabsComuns = null
        };
    }

    private WaveRacaConfig ObterConfigRacaGoblin()
    {
        if (configuracoesRacas == null || configuracoesRacas.Length == 0)
            return null;

        foreach (var c in configuracoesRacas)
        {
            if (c != null && c.raca == TipoRaca.Goblin)
                return c;
        }

        return configuracoesRacas[0];
    }

    private void AtualizarTextoProximaWave()
    {
        if (textoProximaWave != null)
            textoProximaWave.text = "Proxima Wave: Goblin";
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
        spawnDestaWaveConcluido = false;
        numeroWave++;

        configWaveAtual = ObterConfigWave(numeroWave);
        configRacaAtual = ObterConfigRacaGoblin();

        botaoPlay.interactable = false;

        rotinaSpawn = StartCoroutine(GerarWaveComDelay());
    }

    private IEnumerator GerarWaveComDelay()
    {
        if (configWaveAtual == null)
        {
            Debug.LogError("Configuracao da wave esta nula. Verifique o WaveManager.");
            yield break;
        }

        LadoSpawn ladoEscolhido = configWaveAtual.modoWave == ModoWave.Aleatoria
            ? (LadoSpawn)Random.Range(0, 4)
            : configWaveAtual.ladoSpawn;

        Transform rota = ObterRotaPorLado(ladoEscolhido);
        if (rota == null)
        {
            Debug.LogError("Nao foi possivel obter a rota para o lado: " + ladoEscolhido);
            yield break;
        }

        int qtdBoss = Mathf.Max(0, configWaveAtual.quantidadeBoss);
        int qtdComuns = Mathf.Max(0, configWaveAtual.quantidadeComuns);

        for (int i = 0; i < qtdBoss; i++)
        {
            if (configWaveAtual.bossPrefab != null)
            {
                Spawnar(configWaveAtual.bossPrefab, rota);
                yield return new WaitForSeconds(intervaloEntreSpawns);
            }
            else
            {
                Debug.LogWarning($"Wave {numeroWave}: quantidadeBoss={qtdBoss} mas bossPrefab nao atribuido.");
            }
        }

        GameObject[] prefabsComuns = ObterPrefabsComuns();
        if (prefabsComuns == null || prefabsComuns.Length == 0)
        {
            if (qtdComuns > 0)
                Debug.LogWarning($"Wave {numeroWave}: Nenhum prefab de inimigo comum disponivel.");
        }
        else
        {
            for (int i = 0; i < qtdComuns; i++)
            {
                GameObject prefab = configWaveAtual.modoWave == ModoWave.Fixa
                    ? prefabsComuns[i % prefabsComuns.Length]
                    : prefabsComuns[Random.Range(0, prefabsComuns.Length)];
                if (prefab != null)
                {
                    Spawnar(prefab, rota);
                    yield return new WaitForSeconds(intervaloEntreSpawns);
                }
            }
        }

        spawnDestaWaveConcluido = true;
        rotinaSpawn = null;

        if (inimigosVivos.Count == 0 && waveAtiva)
            FinalizarWave();
    }

    private GameObject[] ObterPrefabsComuns()
    {
        if (configWaveAtual.modoWave == ModoWave.Fixa &&
            configWaveAtual.prefabsComuns != null &&
            configWaveAtual.prefabsComuns.Length > 0)
        {
            return configWaveAtual.prefabsComuns;
        }

        if (configRacaAtual != null && configRacaAtual.prefabs != null && configRacaAtual.prefabs.Length > 0)
        {
            return configRacaAtual.prefabs;
        }

        return null;
    }

    private Transform ObterRotaPorLado(LadoSpawn lado)
    {
        if (rotasPai == null || rotasPai.childCount == 0)
            return null;

        int index = (int)lado;
        if (index < 0 || index >= rotasPai.childCount)
            index = 0;

        Transform rota = rotasPai.GetChild(index);
        if (rota.childCount == 0)
            return null;

        return rota;
    }

    private void Spawnar(GameObject prefab, Transform rota)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Tentativa de spawnar um prefab nulo.");
            return;
        }

        if (rota == null || rota.childCount == 0)
        {
            Debug.LogWarning("Rota invalida para spawn!");
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

        if (inimigosVivos.Count == 0 && waveAtiva && spawnDestaWaveConcluido)
            FinalizarWave();
    }

    private void FinalizarWave()
    {
        waveAtiva = false;
        tempoAtual = tempoEntreWaves;
        botaoPlay.interactable = true;

        DefinirProximaWave();
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
