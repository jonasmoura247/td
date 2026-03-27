using UnityEngine;

public class ElfoAssassino : ElfoController
{
    [SerializeField] private float multiplicadorDanoAssassino = 1f;

    public override int GetDanoMagicoFinal()
    {
        return Mathf.RoundToInt(base.GetDanoMagicoFinal() * multiplicadorDanoAssassino);
    }

    public override int GetDanoAtaque() => GetDanoMagicoFinal();
}
