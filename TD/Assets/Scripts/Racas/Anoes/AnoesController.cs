using UnityEngine;

public class AnoesController : UnidadeController
{
    [SerializeField] protected float multiplicadorDanoRacial = 1f;
    [SerializeField] protected float multiplicadorVelocidade = 0.88f;

    protected override void Start()
    {
        faccao = Faccao.Inimigo;
        base.Start();

        if (movimento != null)
            movimento.velocidade *= multiplicadorVelocidade;
    }

    public virtual int GetDanoFinal()
    {
        return Mathf.RoundToInt(danoBase * multiplicadorDanoRacial);
    }

    public override int GetDanoAtaque() => AplicarModificadoresTalentosDano(GetDanoFinal());
}
