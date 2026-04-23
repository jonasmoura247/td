public class MobLobo : MobController
{
    protected override void Start()
    {
        vidaMaxima = 60;
        danoBase   = 10;
        movimento.velocidade = 12f;
        base.Start();
    }
}
