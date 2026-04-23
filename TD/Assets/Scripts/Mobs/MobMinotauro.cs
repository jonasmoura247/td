public class MobMinotauro : MobController
{
    protected override void Start()
    {
        vidaMaxima = 280;
        danoBase   = 25;
        movimento.velocidade = 5f;
        base.Start();
    }
}
