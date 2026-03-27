using UnityEngine;

public class HumanoGuerreiro : HumanoController
{
    [SerializeField] private float multiplicadorDanoGuerreiro = 1f;

    protected override void Start()
    {
        faccao = Faccao.Inimigo;
        base.Start();
    }

    public virtual int GetDanoFinal()
    {
        return Mathf.RoundToInt(danoBase * multiplicadorDanoRacial * multiplicadorDanoGuerreiro);
    }

    public override int GetDanoAtaque() => GetDanoFinal();
}
