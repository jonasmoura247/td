public class MobAranha : MobController
{
    protected override void Start()
    {
        vidaMaxima = 45;
        danoBase   = 8;
        movimento.velocidade = 13f;
        base.Start();
    }
}
