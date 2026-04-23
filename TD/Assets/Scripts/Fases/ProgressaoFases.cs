using System;
using System.Collections.Generic;

[Serializable]
public class ProgressaoFases
{
    public List<string> fasesVencidas = new List<string>();

    public bool FaseVencida(string faseId)
    {
        if (string.IsNullOrEmpty(faseId)) return false;
        return fasesVencidas != null && fasesVencidas.Contains(faseId);
    }

    /// <summary>
    /// Retorna true se a fase está desbloqueada, ou seja, o pré-requisito foi vencido
    /// (ou não existe pré-requisito).
    /// </summary>
    public bool FaseDesbloqueada(string fasePreRequisito)
    {
        if (string.IsNullOrEmpty(fasePreRequisito)) return true;
        return FaseVencida(fasePreRequisito);
    }

    public void RegistrarVitoria(string faseId)
    {
        if (string.IsNullOrEmpty(faseId)) return;
        if (fasesVencidas == null) fasesVencidas = new List<string>();
        if (!fasesVencidas.Contains(faseId))
            fasesVencidas.Add(faseId);
    }

    public void Normalizar()
    {
        if (fasesVencidas == null)
            fasesVencidas = new List<string>();
    }
}
