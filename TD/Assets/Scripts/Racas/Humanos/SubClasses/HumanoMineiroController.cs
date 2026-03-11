using UnityEngine;

public class HumanoMineiroController : HumanoController
{
    private enum EstadoMineiro { IndoMinerio, Minerando, IndoBase }

    [Header("Mineracao")]
    [SerializeField] private float distanciaMinera = 3f;
    [SerializeField] private float distanciaBase = 3f;
    [SerializeField] private int quantidadePorColeta = 5;
    [SerializeField] private float tempoMineracao = 5f;
    [SerializeField] private float tempoProcurar = 2f;

    private EstadoMineiro estado = EstadoMineiro.IndoMinerio;
    private float proximaBusca = 0f;
    private float tempoFimMineracao = 0f;
    private int ouroAcumulado = 0;
    private MinerioDeOuro minerioAtual;
    private Transform castelo;

    protected override void Start()
    {
        base.Start();
        castelo = GameObject.FindWithTag("Castelo")?.transform;
        ProcurarMinerio();
    }

    void ProcurarMinerio()
    {
        minerioAtual = null;
        GameObject minerioGO = GameObject.FindWithTag("MinerioDeOuro");

        if (minerioGO == null || movimento == null)
        {
            estado = EstadoMineiro.IndoMinerio;
            proximaBusca = Time.time + tempoProcurar;
            return;
        }

        minerioAtual = minerioGO.GetComponent<MinerioDeOuro>();
        if (minerioAtual == null)
        {
            estado = EstadoMineiro.IndoMinerio;
            proximaBusca = Time.time + tempoProcurar;
            return;
        }

        estado = EstadoMineiro.IndoMinerio;
        CriarRotaAte(minerioGO.transform);
    }

    void IrParaBase()
    {
        if (castelo == null || movimento == null) return;

        estado = EstadoMineiro.IndoBase;
        CriarRotaAte(castelo);
    }

    void Update()
    {
        if (estado == EstadoMineiro.IndoMinerio)
        {
            if (minerioAtual == null)
            {
                if (Time.time >= proximaBusca)
                {
                    proximaBusca = Time.time + tempoProcurar;
                    ProcurarMinerio();
                }
                return;
            }

            float distMinerio = Vector2.Distance(transform.position, minerioAtual.transform.position);

            if (distMinerio <= distanciaMinera)
            {
                estado = EstadoMineiro.Minerando;
                movimento?.PararMovimento();
                tempoFimMineracao = Time.time + tempoMineracao;
            }
        }
        else if (estado == EstadoMineiro.Minerando)
        {
            if (minerioAtual == null)
            {
                IrParaBase();
                return;
            }

            if (Time.time >= tempoFimMineracao)
            {
                int coletado = minerioAtual.Coletar(quantidadePorColeta);
                ouroAcumulado += coletado;
                IrParaBase();
            }
        }
        else if (estado == EstadoMineiro.IndoBase)
        {
            if (castelo == null)
            {
                estado = EstadoMineiro.IndoMinerio;
                ProcurarMinerio();
                return;
            }

            float distBase = Vector2.Distance(transform.position, castelo.position);

            if (distBase <= distanciaBase)
            {
                movimento?.PararMovimento();

                if (OuroManager.Instance != null && ouroAcumulado > 0)
                {
                    OuroManager.Instance.AdicionarOuro(ouroAcumulado);
                    ouroAcumulado = 0;
                }

                ProcurarMinerio();
            }
        }
    }

    void CriarRotaAte(Transform destino)
    {
        if (destino == null || movimento == null) return;

        GameObject rotaTemp = new GameObject("RotaTemporaria");

        GameObject pontoInicial = new GameObject("PontoInicial");
        pontoInicial.transform.position = transform.position;
        pontoInicial.transform.parent = rotaTemp.transform;

        GameObject pontoDestino = new GameObject("PontoDestino");
        pontoDestino.transform.position = destino.position;
        pontoDestino.transform.parent = rotaTemp.transform;

        movimento.DefinirRota(rotaTemp.transform);
        movimento.ContinuarMovimento();
    }
}