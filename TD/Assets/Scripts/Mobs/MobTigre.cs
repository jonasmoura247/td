public class MobTigre : MobController
{
    protected override void Start()
    {
        vidaMaxima = 80;
        danoBase   = 14;
        movimento.velocidade = 11f;
        base.Start();
    }
}
