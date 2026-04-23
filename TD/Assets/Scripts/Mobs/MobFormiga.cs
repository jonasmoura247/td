public class MobFormiga : MobController
{
    protected override void Start()
    {
        vidaMaxima = 25;
        danoBase   = 4;
        movimento.velocidade = 8f;
        base.Start();
    }
}
