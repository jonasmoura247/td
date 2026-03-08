using UnityEngine;

public class GoblinController : UnidadeController
{
    [Header("Configurações Base do Goblin")]
    [SerializeField] protected float multiplicadorDanoRacial = 1f;

    protected int bonusDanoFixo = 0;

    protected override void Start()
    {
        faccao = Faccao.Inimigo;
        base.Start();
    }

    public void AplicarBonusDano(int valor)
    {
        bonusDanoFixo += valor;
    }

    public void RemoverBonusDano(int valor)
    {
        bonusDanoFixo -= valor;

        if (bonusDanoFixo < 0)
            bonusDanoFixo = 0;
    }

    public virtual int GetDanoFinal()
    {
        return Mathf.RoundToInt(
            danoBase * multiplicadorDanoRacial
        ) + bonusDanoFixo;
    }

    public override int GetDanoAtaque() => GetDanoFinal();

    /// <summary>Usado para logs de debug da aura.</summary>
    public int GetBonusDanoAura() => bonusDanoFixo;
}