using UnityEngine;

public class GoblinLanceiro : GoblinController
{
    [Header("Configuração de Ataque")]
    public float distanciaDeAtaque = 3f;
    public float intervaloAtaque = 1.5f;
    public GameObject projetilPrefab;
    public Transform pontoDisparo;

    private float proximoAtaque;
    private Transform alvo;
    private bool estaParadoParaAtacar = false;

    protected override void Start()
    {
        vidaMaxima = 2;
        danoBase = 1;
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
        if (!estaVivo || alvo == null) return;

        float distancia = Vector2.Distance(transform.position, alvo.position);

        if (distancia <= distanciaDeAtaque)
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

    //private void AjustarPosicaoLateral()
    //{
    //    Vector2 lateral = Random.insideUnitCircle.normalized * 0.5f;
    //    transform.position += (Vector3)lateral * Time.deltaTime;
    //}

    private void Atacar()
    {
        if (Time.time < proximoAtaque) return;

        proximoAtaque = Time.time + intervaloAtaque;

        if (projetilPrefab == null || pontoDisparo == null) return;

        GameObject proj = Instantiate(
            projetilPrefab,
            pontoDisparo.position,
            Quaternion.identity
        );

        LancaProjetil p = proj.GetComponent<LancaProjetil>();

        if (p != null)
        {
            p.Inicializar(alvo, danoBase);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaDeAtaque);
    }
}