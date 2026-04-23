public class MobOrc : MobController
{
    protected override void Start()
    {
        vidaMaxima = 200;
        danoBase   = 18;
        movimento.velocidade = 5f;
        base.Start();
    }
}
