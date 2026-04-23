using System;

/// <summary>
/// Representa o contrato de um herói contratado: duração em fases, custo de renovação.
/// Persiste no save — não pode ser alterado via restart (anti-exploit).
/// </summary>
[Serializable]
public class ContratoHeroi
{
    public string idHeroi;
    public int fasesRestantes;
    public int fasesTotais;
    public int custoRenovacaoBase;
    public int vezesRenovado;
    public bool avisoPenultimaFaseEnviado;

    /// <summary>True quando está na última fase antes de expirar.</summary>
    public bool NaPenultimaFase => fasesRestantes == 1;

    /// <summary>True quando o contrato terminou.</summary>
    public bool Expirado => fasesRestantes <= 0;

    /// <summary>
    /// Custo de renovação cresce a cada vez que o jogador renova.
    /// Fórmula: base * (1 + vezeRenovado).
    /// </summary>
    public int CalcularCustoRenovacao() => custoRenovacaoBase * (1 + vezesRenovado);
}
