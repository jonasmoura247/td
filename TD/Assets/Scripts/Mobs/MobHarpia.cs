using UnityEngine;

public class MobHarpia : MobController
{
    protected override void Start()
    {
        vidaMaxima = 90;
        danoBase   = 12;
        movimento.velocidade = 10f;

        // Voadores ignoram waypoints e voam direto ao castelo
        gameObject.AddComponent<ComportamentoVoador>();

        base.Start();
    }
}
