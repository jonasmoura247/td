public class MobRato : MobController
{
    protected override void Start()
    {
        vidaMaxima = 35;
        danoBase   = 6;
        movimento.velocidade = 14f;
        base.Start();
    }
}
