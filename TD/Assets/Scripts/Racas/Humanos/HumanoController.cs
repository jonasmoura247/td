using UnityEngine;

public class HumanoController : UnidadeController
{
    [Header("Atributos Raciais Humano")]
    [SerializeField] protected float multiplicadorDanoRacial = 1f;

    protected override void Start()
    {
        base.Start();
    }
}
