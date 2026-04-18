using UnityEngine;

public class UnidadeController : MonoBehaviour, IDanificavel
{
    [Header("Status Base")]
    public int vidaMaxima = 100;
    protected int vidaAtual;
    public int danoBase = 1;
    protected bool estaVivo = true;

    [Header("Faccao")]
    public Faccao faccao;

    // Permite que o WaveManager force a facção antes de Start() rodar.
    // Unidades colocadas manualmente no editor não chamam DefinirFaccaoParaSpawn
    // e usam a facção padrão definida no prefab/controller.
    private bool _faccaoForcada = false;
    private Faccao _faccaoForcadaValor;

    /// <summary>
    /// Nome do prefab original (sem "(Clone)"). Definido em Awake antes de qualquer rename.
    /// </summary>
    public string NomePrefab { get; private set; }

    /// <summary>
    /// Chamar após Instantiate (antes de Start) para forçar a facção desta unidade.
    /// Sobrepõe qualquer valor hardcoded no controller (ex: GoblinController seta Inimigo por padrão).
    /// Atualiza também o layer do GameObject para que OverlapCircle funcione corretamente.
    /// </summary>
    public void DefinirFaccaoParaSpawn(Faccao faccaoForcada)
    {
        _faccaoForcada = true;
        _faccaoForcadaValor = faccaoForcada;
    }

    [Header("Combate")]
    [Tooltip("Intervalo em segundos entre ataques (vale para unidades e para o castelo). Menor = mais rápido.")]
    [SerializeField] private float intervaloAtaque = 1f;

    [Header("Crítico")]
    [Tooltip("Chance de causar um golpe crítico (0 = 0%, 1 = 100%). Padrão: 10%.")]
    [Range(0f, 1f)] public float chanceCritico = 0.10f;
    [Tooltip("Multiplicador de dano em golpes críticos.")]
    public float multiplicadorCritico = 1.5f;

    protected Rigidbody2D rb;
    protected Collider2D col;
    protected MovimentoUnidade movimento;

    public bool EstaVivo => estaVivo;

    protected virtual void Awake()
    {
        NomePrefab = gameObject.name.Replace("(Clone)", "").Trim();

        rb        = GetComponent<Rigidbody2D>();
        col       = GetComponent<Collider2D>();
        movimento = GetComponent<MovimentoUnidade>();
    }

    protected virtual void Start()
    {
        // Se o WaveManager (ou qualquer sistema de spawn externo) chamou DefinirFaccaoParaSpawn()
        // antes deste Start() rodar, sobrepõe a facção padrão do prefab/subclass.
        if (_faccaoForcada)
        {
            faccao = _faccaoForcadaValor;

            // Garante que o layer bata com a facção para OverlapCircle
            string layerNome = faccao == Faccao.Jogador ? "Jogador" : "Inimigo";
            int layer = LayerMask.NameToLayer(layerNome);
            if (layer >= 0)
                gameObject.layer = layer;
        }

        vidaAtual = vidaMaxima;

        if (faccao == Faccao.Jogador)
        {
            UnidadesManager mgr = UnidadesManager.Instancia;
            if (mgr == null)
                mgr = FindFirstObjectByType<UnidadesManager>(FindObjectsInactive.Exclude);
            mgr?.Registar(this);
        }
    }

    protected virtual void OnDestroy()
    {
        if (faccao == Faccao.Jogador && UnidadesManager.Instancia != null)
            UnidadesManager.Instancia.Desregistar(this);
    }

    /// <summary>
    /// Chamar após talentos/runas no spawn. Evita colisão física antes de <see cref="Start"/> com <c>vidaAtual</c> ainda 0.
    /// </summary>
    public void SincronizarVidaAposSpawn()
    {
        if (!estaVivo)
            return;
        vidaAtual = vidaMaxima;
    }

    public virtual void ReceberDano(int quantidade)
    {
        if (!estaVivo || quantidade <= 0)
            return;

        vidaAtual -= quantidade;

        if (faccao == Faccao.Inimigo)
            BestiarioDanoEcoRuna.TentarAposDanoDoJogador(this, quantidade);

        if (vidaAtual <= 0)
            Morrer();
    }

    /// <summary>Dano extra de runas (sem disparar procs encadeados).</summary>
    public void ReceberDanoSemProcs(int quantidade)
    {
        if (!estaVivo || quantidade <= 0)
            return;

        vidaAtual -= quantidade;

        if (vidaAtual <= 0)
            Morrer();
    }

    /// <summary>
    /// Chamado pelo castelo ao detectar colisão com esta unidade.
    /// Para o movimento para que a unidade fique parada ao redor do castelo
    /// e o componente de ataque ao castelo assuma o controle.
    /// (Recuo removido conforme novo comportamento TD.)
    /// </summary>
    public void ImpactarCastelo(Vector2 origemImpacto)
    {
        if (!estaVivo) return;
        movimento?.PararMovimento();
    }

    protected virtual void Morrer()
    {
        if (!estaVivo)
            return;

        estaVivo = false;

        NotificarMorte();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (movimento != null)
            movimento.PararMovimento();

        if (col != null)
            col.enabled = false;

        Destroy(gameObject, 0.1f);
    }

    private void NotificarMorte()
    {
        var componentes = GetComponents<MonoBehaviour>();
        foreach (var componente in componentes)
        {
            if (componente is IMorteListener listener)
                listener.OnMorte(this);
        }
    }

    public int GetVidaAtual() => vidaAtual;

    public virtual int GetDanoAtaque() => AplicarModificadoresTalentosDano(danoBase);

    public float GetIntervaloAtaque() => Mathf.Max(0.1f, intervaloAtaque);

    protected int AplicarModificadoresTalentosDano(int valor)
    {
        var m = GetComponent<UnidadeModificadores>();
        if (m == null)
            return valor;
        return m.ModificarDanoFinal(valor);
    }

    protected bool EhAlvoInimigoValido(UnidadeController alvo)
    {
        return alvo != null &&
               alvo != this &&
               alvo.EstaVivo &&
               alvo.faccao != faccao;
    }

    // Buffer reutilizável por instância para EncontrarInimigoMaisProximo
    private readonly Collider2D[] _bufferDeteccao = new Collider2D[32];

    /// <summary>
    /// Busca o inimigo mais próximo dentro de <paramref name="alcanceMaximo"/>.
    /// Usa OverlapCircle com ContactFilter2D (mais eficiente que FindObjectsByType).
    /// Continua funcionando sem layer configurada (fallback por GetComponent).
    /// </summary>
    protected UnidadeController EncontrarInimigoMaisProximo(float alcanceMaximo, LayerMask? layerOverride = null)
    {
        LayerMask mask = layerOverride ?? LayerMask.GetMask(
            faccao == Faccao.Jogador ? "Inimigo" : "Jogador");

        // Se nenhuma layer válida, faz fallback com Physics2D.defaultContactOffset
        var filtro = new ContactFilter2D();
        filtro.SetLayerMask(mask.value != 0 ? mask : (LayerMask)~0);
        int count = Physics2D.OverlapCircle(transform.position, alcanceMaximo, filtro, _bufferDeteccao);

        UnidadeController maisProximo = null;
        float menorDistancia = alcanceMaximo + 1f;

        for (int i = 0; i < count; i++)
        {
            if (_bufferDeteccao[i] == null) continue;
            var u = _bufferDeteccao[i].GetComponentInParent<UnidadeController>();
            if (!EhAlvoInimigoValido(u)) continue;

            float dist = Vector2.Distance(transform.position, u.transform.position);
            if (dist < menorDistancia)
            {
                menorDistancia = dist;
                maisProximo = u;
            }
        }

        return maisProximo;
    }

    /// <summary>
    /// Ponto de extensão para o SistemaIAUnidade executar o ataque desta unidade.
    /// Implementação padrão: dano melee direto.
    /// Unidades ranged (arqueiro, lanceiro, mago) devem sobrescrever para disparar projétil.
    /// O SistemaIAUnidade gerencia o cooldown externamente — este método executa o ataque imediatamente.
    /// </summary>
    public virtual void ExecutarAtaqueIA(UnidadeController alvo)
    {
        if (alvo == null || !alvo.EstaVivo) return;

        int dano = GetDanoAtaque();
        ServicoAplicacaoDano.Aplicar(
            alvo,
            alvo.transform.position,
            dano,
            PedidoDanoFlutuante.Criar(dano, TipoDanoCombate.Fisico),
            dispararProcsRunaBestiario: faccao == Faccao.Inimigo);
    }
}
