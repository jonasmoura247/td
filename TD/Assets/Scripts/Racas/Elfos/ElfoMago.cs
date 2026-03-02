using UnityEngine;

public class ElfoMago : UnidadeController
{
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
        if (castelo != null)
            alvo = castelo.transform;
    }

    private void Update()
    {
        if (!estaVivo || alvo == null) return;

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

    //private void AjustarPosicao()
    //{
    //    Vector2 lateral = Random.insideUnitCircle.normalized * 0.7f;
    //    transform.position += (Vector3)lateral * Time.deltaTime;
    //}

    private void Atacar()
    {
        if (Time.time < proximoAtaque) return;

        proximoAtaque = Time.time + intervaloAtaque;

        GameObject magia = Instantiate(magiaPrefab, pontoDisparo.position, Quaternion.identity);

        LancaProjetil proj = magia.GetComponent<LancaProjetil>();
        if (proj != null)
            proj.Inicializar(alvo, danoBase);
    }
}