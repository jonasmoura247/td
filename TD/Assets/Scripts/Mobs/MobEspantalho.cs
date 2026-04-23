using UnityEngine;

public class MobEspantalho : MobController
{
    protected override void Start()
    {
        vidaMaxima = 120;
        danoBase   = 9;
        movimento.velocidade = 5f;

        // Resistência parcial a veneno (80%)
        var res = gameObject.AddComponent<ResistenciaDano>();
        res.resistenciaVeneno = 0.80f;

        base.Start();
    }
}
