using UnityEngine;

public class AnaoMineiroController : MineiroAliadoController
{
    [Header("Atributos Raciais Anao")]
    [SerializeField] protected float multiplicadorDanoRacial = 1f;
    [SerializeField] protected float multiplicadorVelocidade = 0.88f;

    protected override void AplicarBonusRacialMineiro()
    {
        if (movimento != null)
            movimento.velocidade *= multiplicadorVelocidade;
    }
}
