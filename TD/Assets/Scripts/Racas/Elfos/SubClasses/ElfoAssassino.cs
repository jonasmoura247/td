using UnityEngine;

public class ElfoAssassino : ElfoController
{
    [SerializeField] private float multiplicadorDanoAssassino = 1f;
    [Header("Configuracao Ranged")]
    [SerializeField] private float alcanceAtaque = 25f;
    [SerializeField] private float intervaloAtaque = 2f;
    [SerializeField] private int danoFlecha = 2;
    [SerializeField] private GameObject prefabFlecha;
    [SerializeField] private Transform pontoDisparo;

    private float proximoAtaque;
    private SistemaIAUnidade _ia;

    protected override void Start()
    {
        alcanceAtaque = 25f;
        intervaloAtaque = 2f;
        danoFlecha = 2;
        base.Start();
        _ia = GetComponent<SistemaIAUnidade>();
        CarregarPrefabFlechaSeNecessario();
    }

    private void Update()
    {
        if (_ia != null && _ia.enabled) return;
        if (!estaVivo) return;

        UnidadeController inimigo = EncontrarInimigoMaisProximo(alcanceAtaque);
        if (!EhAlvoInimigoValido(inimigo))
        {
            movimento?.ContinuarMovimento();
            return;
        }

        float dist = Vector2.Distance(transform.position, inimigo.transform.position);
        if (dist <= alcanceAtaque)
        {
            movimento?.PararMovimento();
            AtacarLegado(inimigo.transform);
            return;
        }

        movimento?.ContinuarMovimento();
    }

    // ── Chamado pelo SistemaIAUnidade ────────────────────────────────────

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

    // ── Legado ──────────────────────────────────────────────────────────

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
        prefabFlecha = Resources.Load<GameObject>("Projetis/Flecha");
        if (prefabFlecha == null)
            Debug.LogError($"[ElfoAssassino] prefabFlecha não encontrado em Resources/Projetis/Flecha. Atribua o prefab da flecha no componente '{name}'.", this);
    }

    public override int GetDanoMagicoFinal()
        => Mathf.RoundToInt(base.GetDanoMagicoFinal() * multiplicadorDanoAssassino);

    public override int GetDanoAtaque() => AplicarModificadoresTalentosDano(GetDanoMagicoFinal());
}
