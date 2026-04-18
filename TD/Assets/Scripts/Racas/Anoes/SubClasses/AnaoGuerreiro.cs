using UnityEngine;

public class AnaoGuerreiro : AnoesController
{
    [SerializeField] private float multiplicadorDanoGuerreiro = 1.15f;
    [Header("Configuracao Melee")]
    [SerializeField] private float alcanceAtaque = 2f;


    private float _proximoAtaque;
    private SistemaIAUnidade _ia;

    protected override void Start()
    {
        base.Start();
        _ia = GetComponent<SistemaIAUnidade>();
    }

    private void Update()
    {
        if (_ia != null && _ia.enabled) return;
        if (!estaVivo) return;

        UnidadeController alvo = EncontrarInimigoMaisProximo(alcanceAtaque);
        if (!EhAlvoInimigoValido(alvo))
        {
            movimento?.ContinuarMovimento();
            return;
        }

        movimento?.PararMovimento();
        Atacar(alvo);
    }

    private void Atacar(UnidadeController alvo)
    {
        if (Time.time < _proximoAtaque) return;
        _proximoAtaque = Time.time + GetIntervaloAtaque();
        alvo.ReceberDano(GetDanoAtaque());
    }

    public override void ExecutarAtaqueIA(UnidadeController alvo)
    {
        if (alvo == null || !alvo.EstaVivo) return;
        int dano = GetDanoAtaque();
        ServicoAplicacaoDano.Aplicar(
            alvo,
            alvo.transform.position,
            dano,
            PedidoDanoFlutuante.Criar(dano, TipoDanoCombate.Fisico),
            dispararProcsRunaBestiario: faccao == Faccao.Inimigo);
    }

    public override int GetDanoFinal()
    {
        return Mathf.RoundToInt(danoBase * multiplicadorDanoGuerreiro);
    }
}
