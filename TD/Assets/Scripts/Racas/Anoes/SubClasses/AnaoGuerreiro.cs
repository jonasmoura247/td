using UnityEngine;

public class AnaoGuerreiro : AnoesController
{
    [SerializeField] private float multiplicadorDanoGuerreiro = 1.15f;

    public override int GetDanoFinal()
    {
        return Mathf.RoundToInt(base.GetDanoFinal() * multiplicadorDanoGuerreiro);
    }
}
