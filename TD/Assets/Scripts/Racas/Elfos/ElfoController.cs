using UnityEngine;

public class ElfoController : UnidadeController
{
    [Header("Configurações Raciais Elfo")]
    [SerializeField] protected float multiplicadorVelocidade = 1.1f;
    [SerializeField] protected float multiplicadorDanoMagico = 1f;

    protected override void Start()
    {
        faccao = Faccao.Inimigo; // padrão dos elfos inimigos

        base.Start();

        AplicarBonusRacial();
    }

    protected virtual void AplicarBonusRacial()
    {
        if (movimento != null)
            movimento.velocidade *= multiplicadorVelocidade;
    }

    public virtual int GetDanoMagicoFinal()
    {
        return Mathf.RoundToInt(danoBase * multiplicadorDanoMagico);
    }
}