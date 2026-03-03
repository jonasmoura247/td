using UnityEngine;

public class ElfoMago : UnidadeController
{
    [Header("Configuracao de Ataque")]
    public float distanciaAtaque = 4f;
    public float intervaloAtaque = 2f;
    public GameObject magiaPrefab;
    public Transform pontoDisparo;

    private float proximoAtaque;
    private Transform alvo;

    protected override void Start()
    {
        vidaMaxima = 3;
        danoBase = 2;
        faccao = Faccao.Inimigo;

        base.Start();

        CasteloController castelo = FindObjectOfType<CasteloController>();

        if (castelo != null && castelo.faccao != this.faccao)
        {
            alvo = castelo.transform;
        }
    }

    private void Update()
    {
        if (!estaVivo || alvo == null)
            return;

        float distancia = Vector2.Distance(transform.position, alvo.position);

        if (distancia <= distanciaAtaque)
        {
            if (movimento != null)
                movimento.PararMovimento();

            Atacar();
        }
        else
        {
            if (movimento != null)
                movimento.ContinuarMovimento();
        }
    }

    private void Atacar()
    {
        if (Time.time < proximoAtaque)
            return;

        proximoAtaque = Time.time + intervaloAtaque;

        if (magiaPrefab == null || pontoDisparo == null)
            return;

        GameObject magia = Instantiate(
            magiaPrefab,
            pontoDisparo.position,
            Quaternion.identity
        );

        LancaProjetil proj = magia.GetComponent<LancaProjetil>();

        if (proj != null)
        {
            proj.Inicializar(alvo, danoBase);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}