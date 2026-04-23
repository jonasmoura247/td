using UnityEngine;

/// <summary>
/// Primeiro herói do jogo: o Pirata.
/// Reutiliza toda a infraestrutura existente (UnidadeController, SistemaIAUnidade,
/// MovimentoUnidade, AnimacaoSpritePorVelocidade2D).
/// Facção: Jogador. Raridade: Azul.
/// </summary>
[RequireComponent(typeof(AnimacaoSpritePorVelocidade2D))]
[RequireComponent(typeof(SistemaIAUnidade))]
[RequireComponent(typeof(MovimentoUnidade))]
public class HeroiPirataController : UnidadeController
{
    protected override void Awake()
    {
        base.Awake();
        faccao = Faccao.Jogador;
    }

    protected override void Start()
    {
        base.Start();
        Debug.Log("[HEROI PIRATA SPAWNADO]");
    }
}
