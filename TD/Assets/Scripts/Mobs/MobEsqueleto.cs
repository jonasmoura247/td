using UnityEngine;

public class MobEsqueleto : MobController
{
    protected override void Start()
    {
        vidaMaxima = 80;
        danoBase   = 10;
        movimento.velocidade = 6f;

        // Imune a veneno
        var res = gameObject.AddComponent<ResistenciaDano>();
        res.resistenciaVeneno = 1f;

        base.Start();
    }
}
