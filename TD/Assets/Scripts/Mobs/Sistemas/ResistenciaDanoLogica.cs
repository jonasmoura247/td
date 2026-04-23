using UnityEngine;

/// <summary>
/// Lógica pura de cálculo de resistência a dano. Sem dependência de Unity (testável em EditMode).
/// </summary>
public static class ResistenciaDanoLogica
{
    /// <summary>
    /// Aplica percentual de resistência ao dano original.
    /// Garante dano mínimo de 1 quando há resistência parcial (nunca zera o dano sem imunidade).
    /// Para imunidade total (resistencia >= 1f), retorna 0.
    /// </summary>
    public static int AplicarResistencia(int danoOriginal, float percentualResistencia)
    {
        if (danoOriginal <= 0) return 0;

        float resistencia = Mathf.Clamp01(percentualResistencia);

        if (resistencia >= 1f) return 0;
        if (resistencia <= 0f) return danoOriginal;

        return Mathf.Max(1, Mathf.RoundToInt(danoOriginal * (1f - resistencia)));
    }
}
