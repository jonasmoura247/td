using UnityEngine;

public class MobZumbie : MobController
{
    protected override void Start()
    {
        vidaMaxima = 150;
        danoBase   = 12;
        movimento.velocidade = 4f;

        // Imune a veneno, regenera vida
        var res = gameObject.AddComponent<ResistenciaDano>();
        res.resistenciaVeneno = 1f;

        var regen = gameObject.AddComponent<RegeneracaoVida>();
        regen.vidaPorSegundo = 5f;

        base.Start();
    }
}
