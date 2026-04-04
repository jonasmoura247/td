using UnityEngine;

/// <summary>
/// Torre goblin ranged com lança; reutiliza LancaProjetil e esquema PontoDisparo do GoblinLanceiro.
/// </summary>
public class TorreGoblinLanceiraController : TorreController
{
    protected override void Awake()
    {
        base.Awake();

        if (dados != null && dados.raca != TipoRaca.Goblin)
        {
            Debug.LogWarning(
                $"[TorreGoblinLanceira] O asset de dados indica raça {dados.raca}; esta torre espera {TipoRaca.Goblin}.",
                this);
        }
    }
}
