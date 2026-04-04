using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour
{
    public const int TotalOndas = 8;

    private static readonly int[] InimigosComunsPorOndaDesign = { 5, 7, 10, 15, 15, 20, 25, 30 };

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
    [SerializeField] private TextMeshProUGUI textoIndicadorOnda;

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
    [Tooltip("Uma entrada por TipoRaca (Goblin, Humano, Elfo, Anao). Usada no modo Aleatoria e para comuns/boss quando a wave nao define prefabs proprios. Com perfil ativo, a raca do jogador e excluida do sorteio.")]
    [SerializeField] private WaveRacaConfig[] configuracoesRacas;

    [Header("===== TALENTOS =====")]
    [SerializeField] private TalentTreeDatabase talentTreeDatabase;

    [Header("===== BESTIARIO =====")]
    [SerializeField] private BestiarioDatabase bestiarioDatabase;

    private readonly List<GameObject> inimigosVivos = new List<GameObject>();

    private bool waveAtiva = false;
    private bool spawnDestaWaveConcluido = false;
    private int numeroWave = 0;
    private Coroutine rotinaSpawn;

    private WaveConfig configWaveAtual;
    private WaveRacaConfig configRacaAtual;

    private bool partidaFinalizada;

    private void Awake()
    {
        if (FindFirstObjectByType<RunExpPartida>(FindObjectsInactive.Include) == null)
        {
            var go = new GameObject("RunExpPartida");
            go.AddComponent<RunExpPartida>();
        }
    }

    private void Start()
    {
        tempoAtual = tempoEntreWaves;

        if (!PerfilJogadorAtivo.TemPerfilAtivo)
            Debug.LogWarning("[WaveManager] Perfil ativo nao encontrado: o filtro de raca inimiga esta desativado (todas as racas em configuracoesRacas podem ser sorteadas). Entre no mapa pelo fluxo de slots (criar ou carregar perfil).");

        DefinirProximaWave();

        botaoPlay.onClick.AddListener(ForcarInicioWave);
        botaoPlay.interactable = true;

        if (botaoReiniciar != null)
            botaoReiniciar.onClick.AddListener(ReiniciarPartida);

        if (PerfilJogadorAtivo.TemPerfilAtivo && PerfilJogadorAtivo.Dados != null)
        {
            var db = ModificadoresPartidaTalentos.CarregarDatabaseOuNull(talentTreeDatabase);
            db?.RecalcularPontosDisponiveis(PerfilJogadorAtivo.Dados);
        }
    }

    private void Update()
    {
        if (partidaFinalizada)
            return;
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
        configRacaAtual = SortearConfigRacaInimiga();
        configWaveAtual = ObterConfigWave(numeroWave + 1);

        AtualizarHudOndas();
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
        WaveRacaConfig raca = configRacaAtual;
        if (raca == null)
            return new WaveConfig();

        int comuns;
        if (waveNumero >= 1 && waveNumero <= TotalOndas)
            comuns = InimigosComunsPorOndaDesign[waveNumero - 1];
        else
            comuns = Mathf.Max(0, raca.baseQtd + (waveNumero * raca.incrementoPorWave));

        bool ondaBoss = waveNumero == 3 || waveNumero == 6 || waveNumero == TotalOndas;
        int bosses = ondaBoss && raca.bossPrefab != null ? 1 : 0;

        return new WaveConfig
        {
            modoWave = ModoWave.Aleatoria,
            ladoSpawn = (LadoSpawn)Random.Range(0, 4),
            quantidadeBoss = bosses,
            bossPrefab = raca.bossPrefab,
            quantidadeComuns = comuns,
            prefabsComuns = null
        };
    }

    private WaveRacaConfig SortearConfigRacaInimiga()
    {
        if (configuracoesRacas == null || configuracoesRacas.Length == 0)
            return null;

        var candidatos = new List<WaveRacaConfig>();

        if (!PerfilJogadorAtivo.TemPerfilAtivo)
        {
            foreach (var c in configuracoesRacas)
            {
                if (c != null)
                    candidatos.Add(c);
            }
        }
        else
        {
            TipoRaca racaJogador = PerfilJogadorAtivo.Raca;
            foreach (var c in configuracoesRacas)
            {
                if (c != null && c.raca != racaJogador)
                    candidatos.Add(c);
            }
        }

        if (candidatos.Count > 0)
            return candidatos[Random.Range(0, candidatos.Count)];

        if (PerfilJogadorAtivo.TemPerfilAtivo)
            Debug.LogError("[WaveManager] Nenhuma raca inimiga apos excluir a raca do jogador (" + PerfilJogadorAtivo.Raca + "). Preencha configuracoesRacas com pelo menos uma raca diferente da do perfil.");
        else
            Debug.LogWarning("[WaveManager] Nenhuma entrada valida em configuracoesRacas.");

        return null;
    }

    /// <summary>
    /// Indica se a wave depende de configRacaAtual para prefabs de comuns ou boss.
    /// </summary>
    private static bool WavePrecisaConfigRacaParaSpawn(WaveConfig cfg)
    {
        if (cfg == null)
            return false;

        bool comunsVemDaRaca = cfg.modoWave == ModoWave.Aleatoria ||
            cfg.prefabsComuns == null || cfg.prefabsComuns.Length == 0;

        if (comunsVemDaRaca)
            return true;

        return cfg.quantidadeBoss > 0 && cfg.bossPrefab == null;
    }

    private void AtualizarHudOndas()
    {
        if (textoIndicadorOnda != null)
        {
            if (waveAtiva)
                textoIndicadorOnda.text = $"Wave {numeroWave}/{TotalOndas}";
            else if (numeroWave >= TotalOndas)
                textoIndicadorOnda.text = $"Wave {TotalOndas}/{TotalOndas}";
            else
                textoIndicadorOnda.text = $"Wave {numeroWave + 1}/{TotalOndas}";
        }

        AtualizarTextoProximaWave();
    }

    private void AtualizarTextoProximaWave()
    {
        if (textoProximaWave == null)
            return;
        if (configRacaAtual != null)
            textoProximaWave.text = "Proxima Wave: " + ObterNomeRacaAmigavel(configRacaAtual.raca);
        else
            textoProximaWave.text = "Proxima Wave: —";
    }

    private static string ObterNomeRacaAmigavel(TipoRaca raca)
    {
        return raca switch
        {
            TipoRaca.Goblin => "Goblin",
            TipoRaca.Humano => "Humano",
            TipoRaca.Elfo => "Elfo",
            TipoRaca.Anao => "Anão",
            _ => raca.ToString()
        };
    }

    public void ForcarInicioWave()
    {
        if (partidaFinalizada)
            return;
        if (waveAtiva) return;

        tempoAtual = 0;
        IniciarWave();
    }

    private void IniciarWave()
    {
        if (partidaFinalizada)
            return;
        if (waveAtiva) return;

        int proximaWave = numeroWave + 1;
        WaveConfig cfgPreview = ObterConfigWave(proximaWave);
        if (WavePrecisaConfigRacaParaSpawn(cfgPreview) && configRacaAtual == null)
        {
            Debug.LogError("[WaveManager] Wave " + proximaWave + " precisa de configuracoesRacas validas (raca inimiga), mas nenhuma esta disponivel. Corrija o Inspector ou entre pelo fluxo de slots.");
            tempoAtual = tempoEntreWaves;
            if (botaoPlay != null)
                botaoPlay.interactable = true;
            return;
        }

        waveAtiva = true;
        spawnDestaWaveConcluido = false;
        numeroWave++;

        configWaveAtual = ObterConfigWave(numeroWave);

        botaoPlay.interactable = false;

        AtualizarHudOndas();

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
            GameObject prefabBoss = ObterPrefabBossParaSpawn();
            if (prefabBoss != null)
            {
                Spawnar(prefabBoss, rota);
                yield return new WaitForSeconds(intervaloEntreSpawns);
            }
            else
            {
                Debug.LogWarning($"Wave {numeroWave}: quantidadeBoss={qtdBoss} mas nenhum bossPrefab (wave nem raca).");
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

    private GameObject ObterPrefabBossParaSpawn()
    {
        if (configWaveAtual != null && configWaveAtual.bossPrefab != null)
            return configWaveAtual.bossPrefab;
        if (configRacaAtual != null && configRacaAtual.bossPrefab != null)
            return configRacaAtual.bossPrefab;
        return null;
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

        Debug.Log($"[WaveManager] Onda {numeroWave}: spawn '{prefab.name}' -> instancia '{inimigo.name}'");

        MovimentoUnidade mov = inimigo.GetComponent<MovimentoUnidade>();
        if (mov != null)
            mov.DefinirRota(rota);

        UnidadeController uc = inimigo.GetComponent<UnidadeController>();
        if (uc != null)
            ModificadoresPartidaTalentos.Aplicar(uc, talentTreeDatabase);
        BestiarioRunasSpawnAplicador.Aplicar(inimigo, uc, bestiarioDatabase);

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

        if (numeroWave >= TotalOndas)
        {
            partidaFinalizada = true;
            tempoAtual = tempoEntreWaves;
            botaoPlay.interactable = false;
            if (RunExpPartida.Instance != null)
                RunExpPartida.Instance.ConcluirComVitoria();
            if (textoIndicadorOnda != null)
                textoIndicadorOnda.text = "Vitoria";
            return;
        }

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

#if UNITY_EDITOR
    [ContextMenu("Preencher ondas 1-8 (design doc)")]
    private void PreencherOndasDesignDoc()
    {
        configuracoesWaves = new WaveConfig[TotalOndas];
        for (int i = 0; i < TotalOndas; i++)
        {
            int n = i + 1;
            configuracoesWaves[i] = new WaveConfig
            {
                modoWave = ModoWave.Aleatoria,
                ladoSpawn = LadoSpawn.Esquerda,
                quantidadeComuns = InimigosComunsPorOndaDesign[i],
                quantidadeBoss = (n == 3 || n == 6 || n == TotalOndas) ? 1 : 0,
                bossPrefab = null,
                prefabsComuns = null
            };
        }

        UnityEditor.EditorUtility.SetDirty(this);
        if (gameObject != null)
            UnityEditor.EditorUtility.SetDirty(gameObject);
    }
#endif
}
