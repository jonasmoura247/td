using UnityEngine;

/// <summary>
/// Lógica pura de regeneração de vida. Sem dependência de Unity (testável em EditMode).
/// </summary>
public static class RegeneracaoVidaLogica
{
    /// <summary>
    /// Calcula quanto de vida deve ser regenerado dado o tempo passado.
    /// </summary>
    public static int CalcularCura(float vidaPorSegundo, float deltaTime)
    {
        if (vidaPorSegundo <= 0f || deltaTime <= 0f) return 0;
        return Mathf.Max(1, Mathf.RoundToInt(vidaPorSegundo * deltaTime));
    }

    /// <summary>
    /// Calcula nova vida após cura, sem ultrapassar o máximo.
    /// </summary>
    public static int AplicarCura(int vidaAtual, int cura, int vidaMaxima)
    {
        if (cura <= 0) return vidaAtual;
        return Mathf.Min(vidaAtual + cura, vidaMaxima);
    }
}
