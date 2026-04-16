using UnityEngine;

/// <summary>
/// Controller do arqueiro humano. Unidade de combate à distância.
/// Se SistemaIAUnidade estiver presente no prefab, este Update() é ignorado
/// e o ataque é executado via ExecutarAtaqueIA().
/// </summary>
public class HumanoArqueiroController : HumanoController
{
    [Header("Configuracao Arqueiro")]
    [SerializeField] private float alcanceAtaque = 25f;
    [SerializeField] private float intervaloAtaque = 2f;
    [SerializeField] private int danoFlecha = 2;
    [SerializeField] private GameObject prefabFlecha;
    [SerializeField] private Transform pontoDisparo;

    private float proximoAtaque;
    private SistemaIAUnidade _ia;

    protected override void Start()
    {
        vidaMaxima = 100;
        danoBase = 1;
        faccao = Faccao.Jogador;
        alcanceAtaque = 25f;
        intervaloAtaque = 2f;
        danoFlecha = 2;
        base.Start();
        _ia = GetComponent<SistemaIAUnidade>();
        CarregarPrefabFlechaSeNecessario();
    }

    private void Update()
    {
        // SistemaIAUnidade presente: delega toda a lógica de AI a ele
        if (_ia != null && _ia.enabled) return;

        if (!estaVivo) return;

        UnidadeController inimigoProximo = EncontrarInimigoMaisProximo(alcanceAtaque);
        if (!EhAlvoInimigoValido(inimigoProximo))
        {
            movimento?.ContinuarMovimento();
            return;
        }

        float distancia = Vector2.Distance(transform.position, inimigoProximo.transform.position);
        if (distancia <= alcanceAtaque)
        {
            movimento?.PararMovimento();
            AtacarLegado(inimigoProximo.transform);
            return;
        }

        movimento?.ContinuarMovimento();
    }

    // ── Chamado pelo SistemaIAUnidade (sem cooldown próprio — IA gerencia) ──

    public override void ExecutarAtaqueIA(UnidadeController alvo)
    {
        if (alvo == null) return;
        CarregarPrefabFlechaSeNecessario();
        if (prefabFlecha == null) return;

        Vector3 origem = pontoDisparo != null ? pontoDisparo.position : transform.position;
        GameObject flecha = Instantiate(prefabFlecha, origem, Quaternion.identity);
        LancaProjetil p = flecha.GetComponent<LancaProjetil>();
        if (p != null) p.Inicializar(alvo.transform, danoFlecha, gameObject.name);
    }

    // ── Usado pelo Update() legado (com cooldown próprio) ──────────────────

    private void AtacarLegado(Transform alvo)
    {
        if (Time.time < proximoAtaque || alvo == null) return;
        CarregarPrefabFlechaSeNecessario();
        if (prefabFlecha == null) return;

        proximoAtaque = Time.time + intervaloAtaque;

        Vector3 origem = pontoDisparo != null ? pontoDisparo.position : transform.position;
        GameObject flecha = Instantiate(prefabFlecha, origem, Quaternion.identity);
        LancaProjetil p = flecha.GetComponent<LancaProjetil>();
        if (p != null) p.Inicializar(alvo, danoFlecha, gameObject.name);
    }

    private void CarregarPrefabFlechaSeNecessario()
    {
        if (prefabFlecha != null) return;
        Debug.LogError($"[HumanoArqueiroController] prefabFlecha não está configurado no Inspector do prefab '{name}'. Atribua o prefab da flecha no componente.", this);
    }
}
