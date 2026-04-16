using UnityEngine;

public class TorreController : MonoBehaviour, IDanificavel
{
    [Header("Dados")]
    [SerializeField] protected TorreDadosSO dados;

    [Header("Referências")]
    [SerializeField] protected Transform pontoDisparo;
    [SerializeField] protected Animator animatorOpcional;

    [Header("Prioridade de alvo")]
    [Tooltip("Usado quando o inimigo não tem MovimentoUnidade (fallback por distância ao objetivo).")]
    [SerializeField] protected Transform referenciaObjetivoTrajetoFallback;

    [SerializeField] protected bool procurarCasteloPorTag = true;

    private TorreBaseCore _vida;
    private TorreSistemaModificadoresCore _mods;
    private TorreAtributosCore _atributos;
    private TorreAlvejamentoCore _alvejamento;
    private TorreAnimacaoDisparoBridge _animBridge;
    private TorreAtaqueCore _ataque;

    public TorreDadosSO Dados => dados;
    public TorreSistemaModificadoresCore Modificadores => _mods;
    public Transform AlvoAtual => _alvejamento != null ? _alvejamento.AlvoAtual : null;
    public bool EstaViva => _vida != null && _vida.EstaViva;
    public bool EstaVivo => EstaViva;

    protected virtual void Awake()
    {
        if (pontoDisparo == null)
        {
            Transform encontrado = transform.Find("PontoDisparo");
            if (encontrado != null)
                pontoDisparo = encontrado;
        }

        _vida = new TorreBaseCore();
        _mods = new TorreSistemaModificadoresCore();
        _atributos = new TorreAtributosCore();
        _atributos.Configurar(dados, _mods);

        _alvejamento = new TorreAlvejamentoCore();
        _animBridge = new TorreAnimacaoDisparoBridge();
        _animBridge.Configurar(animatorOpcional, dados);

        _ataque = new TorreAtaqueCore();
        _ataque.Configurar(pontoDisparo, _atributos, _animBridge, gameObject.name);

        if (dados != null)
            _vida.Inicializar(_atributos.ObterVidaMaximaEfetiva());
        else
            _vida.Inicializar(1);

        _vida.AoMorrer += AoMorrerTorre;
    }

    protected virtual void Start()
    {
        if (referenciaObjetivoTrajetoFallback == null && procurarCasteloPorTag)
        {
            GameObject castelo = GameObject.FindGameObjectWithTag("Castelo");
            if (castelo != null)
                referenciaObjetivoTrajetoFallback = castelo.transform;
        }

        PrioridadeAlvoTorre prioridade = dados != null ? dados.prioridadeAlvo : PrioridadeAlvoTorre.Primeiro;
        _alvejamento.Configurar(transform, _atributos, referenciaObjetivoTrajetoFallback, prioridade);

        TorresManager mgr = TorresManager.Instancia;
        if (mgr == null)
            mgr = FindFirstObjectByType<TorresManager>(FindObjectsInactive.Exclude);

        mgr?.Registar(this);
    }

    protected virtual void OnDestroy()
    {
        if (_vida != null)
            _vida.AoMorrer -= AoMorrerTorre;

        if (TorresManager.Instancia != null)
            TorresManager.Instancia.Desregistar(this);
    }

    protected virtual void Update()
    {
        if (!EstaViva)
            return;

        _alvejamento.AtualizarAlvo();
        if (_alvejamento.AlvoAtual != null)
            _ataque.TentarAtacar(_alvejamento.AlvoAtual);
    }

    public virtual void ReceberDano(int quantidade)
    {
        if (!EstaViva || quantidade <= 0)
            return;

        ServicoDanoFlutuante.Mostrar(
            transform.position,
            PedidoDanoFlutuante.Criar(quantidade, TipoDanoCombate.Fisico));

        AplicarDanoSomenteVida(quantidade);
    }

    /// <summary>Aplica dano na vida da torre sem texto (feedback já exibido por <see cref="ServicoAplicacaoDano"/>).</summary>
    public void AplicarDanoSomenteVida(int quantidade)
    {
        _vida?.ReceberDano(quantidade);
    }

    protected virtual void AoMorrerTorre(TorreBaseCore _)
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Altera a prioridade de alvo em runtime (ex.: via HUD).
    /// Reflete imediatamente no próximo frame de atualização do alvejamento.
    /// </summary>
    public void DefinirPrioridadeAlvo(PrioridadeAlvoTorre prioridade)
    {
        _alvejamento?.DefinirPrioridade(prioridade);
    }

    /// <summary>Animation Event no clip de ataque: dispara o projétil no timing da animação.</summary>
    public void AnimacaoEventoDisparo()
    {
        _ataque?.ExecutarDisparoPendentePorAnimacao();
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (dados == null || !dados.mostrarGizmos)
            return;

        float r = dados.alcanceAtaque;
        Gizmos.color = new Color(0.25f, 0.85f, 0.35f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, r);

        if (pontoDisparo != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pontoDisparo.position, 0.12f);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif
        if (AlvoAtual != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, AlvoAtual.position);
        }
    }
}
