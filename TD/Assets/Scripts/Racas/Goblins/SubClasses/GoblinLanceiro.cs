using UnityEngine;

public class GoblinLanceiro : GoblinController
{
    [Header("Configuracao de Ataque")]
    public float distanciaDeAtaque = 25f;
    public float intervaloAtaque = 2f;
    public int danoFlecha = 2;
    public GameObject projetilPrefab;
    public Transform pontoDisparo;

    private float proximoAtaque;

    protected override void Start()
    {
        faccao = Faccao.Inimigo;
        distanciaDeAtaque = 25f;
        intervaloAtaque = 2f;
        danoFlecha = 2;
        base.Start();
        CarregarFlechaSeNecessario();
    }

    private void Update()
    {
        if (!estaVivo)
            return;

        UnidadeController inimigoProximo = EncontrarInimigoMaisProximo(distanciaDeAtaque);
        if (!EhAlvoInimigoValido(inimigoProximo))
        {
            movimento?.ContinuarMovimento();
            return;
        }

        float distancia = Vector2.Distance(transform.position, inimigoProximo.transform.position);
        if (distancia <= distanciaDeAtaque)
        {
            movimento?.PararMovimento();
            Atacar(inimigoProximo.transform);
            return;
        }

        movimento?.ContinuarMovimento();
    }

    private void Atacar(Transform alvo)
    {
        if (Time.time < proximoAtaque)
            return;

        if (alvo == null)
            return;

        proximoAtaque = Time.time + intervaloAtaque;

        CarregarFlechaSeNecessario();
        if (projetilPrefab == null)
            return;

        Vector3 origem = pontoDisparo != null ? pontoDisparo.position : transform.position;

        GameObject proj = Instantiate(
            projetilPrefab,
            origem,
            Quaternion.identity
        );

        LancaProjetil p = proj.GetComponent<LancaProjetil>();

        if (p != null)
            p.Inicializar(alvo, danoFlecha, gameObject.name);
    }

    private void CarregarFlechaSeNecessario()
    {
        GameObject flecha = Resources.Load<GameObject>("Projetis/Flecha");
        if (flecha != null)
            projetilPrefab = flecha;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaDeAtaque);
    }
}
