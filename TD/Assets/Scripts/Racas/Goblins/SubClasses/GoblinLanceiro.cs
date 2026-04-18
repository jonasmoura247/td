using UnityEngine;

public class GoblinLanceiro : GoblinController
{
    [Header("Configuracao de Ataque")]
    public float distanciaDeAtaque = 25f;
    public int danoFlecha = 2;
    public GameObject projetilPrefab;
    public Transform pontoDisparo;

    private float proximoAtaque;
    private SistemaIAUnidade _ia;
    private EstruturasController _casteloCache;

    protected override void Start()
    {
        faccao = Faccao.Inimigo;
        distanciaDeAtaque = 25f;
        danoFlecha = 2;
        base.Start();
        _ia = GetComponent<SistemaIAUnidade>();
        CarregarFlechaSeNecessario();
        BuscarCasteloCached();
    }

    private void Update()
    {
        // SistemaIAUnidade presente: ele gerencia unidades aliadas como alvo;
        // apenas o ataque ao castelo (fallback) permanece aqui.
        if (_ia != null && _ia.enabled)
        {
            // Quando IA está ociosa (sem unidade-alvo), verifica castelo em alcance
            if (_ia.Estado == EstadoIA.Ocioso && CasteloEmAlcance())
            {
                movimento?.PararMovimento();
                AtacarCasteloLanceiro();
            }
            return;
        }

        // ── Comportamento legado (sem SistemaIAUnidade) ──────────────────

        if (!estaVivo) return;

        // 1. Prioridade: unidades aliadas em alcance
        UnidadeController inimigoProximo = EncontrarInimigoMaisProximo(distanciaDeAtaque);
        if (EhAlvoInimigoValido(inimigoProximo))
        {
            float dist = Vector2.Distance(transform.position, inimigoProximo.transform.position);
            if (dist <= distanciaDeAtaque)
            {
                movimento?.PararMovimento();
                AtacarLegado(inimigoProximo.transform);
                return;
            }
        }

        // 2. Fallback: atacar castelo se estiver ao alcance
        if (CasteloEmAlcance())
        {
            movimento?.PararMovimento();
            AtacarCasteloLanceiro();
            return;
        }

        movimento?.ContinuarMovimento();
    }

    // ── Chamado pelo SistemaIAUnidade ────────────────────────────────────

    public override void ExecutarAtaqueIA(UnidadeController alvo)
    {
        if (alvo == null) return;
        CarregarFlechaSeNecessario();
        if (projetilPrefab == null) return;

        Vector3 origem = pontoDisparo != null ? pontoDisparo.position : transform.position;
        GameObject proj = Instantiate(projetilPrefab, origem, Quaternion.identity);
        LancaProjetil p = proj.GetComponent<LancaProjetil>();
        if (p != null) p.Inicializar(alvo.transform, danoFlecha, gameObject.name);
    }

    // ── Ataque ao castelo (ranged fallback) ─────────────────────────────

    private bool CasteloEmAlcance()
    {
        if (_casteloCache == null || !_casteloCache.EstaVivo) return false;
        return Vector2.Distance(transform.position, _casteloCache.transform.position) <= distanciaDeAtaque;
    }

    private void AtacarCasteloLanceiro()
    {
        if (Time.time < proximoAtaque) return;
        if (_casteloCache == null || !_casteloCache.EstaVivo) return;

        proximoAtaque = Time.time + GetIntervaloAtaque();
        CarregarFlechaSeNecessario();
        if (projetilPrefab == null) return;

        Vector3 origem = pontoDisparo != null ? pontoDisparo.position : transform.position;
        GameObject proj = Instantiate(projetilPrefab, origem, Quaternion.identity);
        LancaProjetil p = proj.GetComponent<LancaProjetil>();
        if (p != null) p.Inicializar(_casteloCache.transform, danoFlecha, gameObject.name);
    }

    // ── Legado ───────────────────────────────────────────────────────────

    private void AtacarLegado(Transform alvo)
    {
        if (Time.time < proximoAtaque || alvo == null) return;
        proximoAtaque = Time.time + GetIntervaloAtaque();
        CarregarFlechaSeNecessario();
        if (projetilPrefab == null) return;

        Vector3 origem = pontoDisparo != null ? pontoDisparo.position : transform.position;
        GameObject proj = Instantiate(projetilPrefab, origem, Quaternion.identity);
        LancaProjetil p = proj.GetComponent<LancaProjetil>();
        if (p != null) p.Inicializar(alvo, danoFlecha, gameObject.name);
    }

    // ── Utilitários ─────────────────────────────────────────────────────

    private void CarregarFlechaSeNecessario()
    {
        if (projetilPrefab != null) return;
        Debug.LogError($"[GoblinLanceiro] projetilPrefab não está configurado no Inspector do prefab '{name}'. Atribua o prefab da flecha no componente.", this);
    }

    private void BuscarCasteloCached()
    {
        GameObject go = GameObject.FindGameObjectWithTag("Castelo");
        if (go != null) _casteloCache = go.GetComponent<EstruturasController>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaDeAtaque);
    }
}
