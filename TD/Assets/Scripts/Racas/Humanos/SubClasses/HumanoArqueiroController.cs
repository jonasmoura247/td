using UnityEngine;

/// <summary>
/// Controller do arqueiro humano. Unidade de combate à distância.
/// </summary>
public class HumanoArqueiroController : HumanoController
{
    protected override void Start()
    {
        vidaMaxima = 100;
        danoBase = 1;
        faccao = Faccao.Jogador;
        base.Start();
    }
}
