using UnityEngine;

public class MobOgre : MobController
{
    protected override void Start()
    {
        vidaMaxima = 350;
        danoBase   = 22;
        movimento.velocidade = 4f;

        var res = gameObject.AddComponent<ResistenciaDano>();
        res.resistenciaFisica = 0.20f;

        base.Start();
    }
}
