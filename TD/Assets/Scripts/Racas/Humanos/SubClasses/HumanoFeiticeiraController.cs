using UnityEngine;

/// <summary>
/// Controller da feiticeira humana. Unidade de combate mago.
/// Se SistemaIAUnidade estiver presente, este Update() é ignorado.
/// </summary>
public class HumanoFeiticeiraController : HumanoController
{
    [Header("Configuração de Ataque")]
    [SerializeField] private float rangeDeAtaque = 30f;

    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private AudioClip somFireball;

    private float proximoAtaque;
    private SistemaIAUnidade _ia;

    protected override void Start()
    {
        vidaMaxima = 100;
        danoBase = 1;
        faccao = Faccao.Jogador;
        base.Start();
        _ia = GetComponent<SistemaIAUnidade>();
        CarregarRecursos();
    }

    private void Update()
    {
        if (_ia != null && _ia.enabled) return;
        if (!estaVivo) return;

        UnidadeController inimigo = EncontrarInimigoMaisProximo(rangeDeAtaque);
        if (inimigo == null)
        {
            movimento?.PararMovimento();
            return;
        }

        float dist = Vector2.Distance(transform.position, inimigo.transform.position);
        if (dist <= rangeDeAtaque)
        {
            movimento?.PararMovimento();
            AtacarLegado(inimigo.transform);
        }
        else
        {
            movimento?.ContinuarMovimento();
        }
    }

    // ── Chamado pelo SistemaIAUnidade ────────────────────────────────────

    public override void ExecutarAtaqueIA(UnidadeController alvo)
    {
        if (alvo == null) return;
        CarregarRecursos();
        if (fireballPrefab == null) return;

        GameObject fb = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
        FireballProjetil proj = fb.GetComponent<FireballProjetil>();
        if (proj != null) proj.Inicializar(alvo.transform, GetDanoAtaque(), gameObject.name);

        if (somFireball != null)
            AudioSource.PlayClipAtPoint(somFireball, transform.position);
    }

    // ── Legado ──────────────────────────────────────────────────────────

    private void AtacarLegado(Transform alvo)
    {
        if (Time.time < proximoAtaque || alvo == null) return;
        CarregarRecursos();
        if (fireballPrefab == null) return;

        proximoAtaque = Time.time + GetIntervaloAtaque();

        GameObject fb = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
        FireballProjetil proj = fb.GetComponent<FireballProjetil>();
        if (proj != null) proj.Inicializar(alvo, GetDanoAtaque(), gameObject.name);

        if (somFireball != null)
            AudioSource.PlayClipAtPoint(somFireball, transform.position);
    }

    private void CarregarRecursos()
    {
        if (fireballPrefab == null)
            fireballPrefab = Resources.Load<GameObject>("Fireball");
        if (somFireball == null)
            somFireball = Resources.Load<AudioClip>("Sons/Humanos/Feiticeiro/fireball");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, rangeDeAtaque);
    }
}
