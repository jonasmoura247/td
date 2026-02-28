using UnityEngine;

public class HumanoMineiroController : HumanoController
{
    private float tempoProcurar = 2f;
    private float proximaBusca = 0f;
    private bool minerando = false;

    protected override void Start()
    {
        base.Start();

        ProcurarMinerio();
    }

    void ProcurarMinerio()
    {
        GameObject minerio = GameObject.FindWithTag("MinerioDeOuro");

        if (minerio == null || movimento == null)
            return;

        CriarRotaAteMinerio(minerio.transform);
    }

    void Update()
    {
        if (minerando)
            return;

        if (Time.time >= proximaBusca)
        {
            proximaBusca = Time.time + tempoProcurar;
            ProcurarMinerio();
        }
    }

    void CriarRotaAteMinerio(Transform destino)
    {
        GameObject rotaTemp = new GameObject("RotaTemporaria");

        GameObject pontoInicial = new GameObject("PontoInicial");
        pontoInicial.transform.position = transform.position;
        pontoInicial.transform.parent = rotaTemp.transform;

        GameObject pontoDestino = new GameObject("PontoDestino");
        pontoDestino.transform.position = destino.position;
        pontoDestino.transform.parent = rotaTemp.transform;

        movimento.DefinirRota(rotaTemp.transform);
    }
}