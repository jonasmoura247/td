using UnityEngine;

public class MobGolemPedra : MobController
{
    protected override void Start()
    {
        vidaMaxima = 500;
        danoBase   = 30;
        movimento.velocidade = 3f;

        var res = gameObject.AddComponent<ResistenciaDano>();
        res.resistenciaFisica = 0.40f;

        base.Start();
    }
}
