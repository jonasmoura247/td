using UnityEngine;

public class ElfoMineiroController : MineiroAliadoController
{
    [Header("Atributos Raciais Elfo")]
    [SerializeField] protected float multiplicadorVelocidade = 1.1f;
    [SerializeField] protected float multiplicadorDanoMagico = 1f;

    protected override void AplicarBonusRacialMineiro()
    {
        if (movimento != null)
            movimento.velocidade *= multiplicadorVelocidade;
    }
}
