using UnityEngine;

public static class NivelRacaCalculadora
{
    public static int ExpTotalNecessariaParaAlcancarNivel(int nivel)
    {
        if (nivel <= 1)
            return 0;
        int l = nivel - 1;
        return 150 * l * l + 250 * l;
    }

    public static int CalcularNivelPorExpTotal(int expTotal)
    {
        if (expTotal < 0)
            expTotal = 0;
        int nivel = 1;
        while (nivel < 100000)
        {
            int limiarProximo = ExpTotalNecessariaParaAlcancarNivel(nivel + 1);
            if (expTotal < limiarProximo)
                return nivel;
            nivel++;
        }
        return nivel;
    }

    public static int ObterExpNoNivelAtual(int expTotal)
    {
        return ObterExpAcumuladaNoNivelAtual(expTotal);
    }

    public static int ObterExpAcumuladaNoNivelAtual(int expTotal)
    {
        int nivel = CalcularNivelPorExpTotal(expTotal);
        int limiarAtual = ExpTotalNecessariaParaAlcancarNivel(nivel);
        return Mathf.Max(0, expTotal - limiarAtual);
    }

    public static int ObterExpTotalParaCompletarSegmentoDoNivelAtual(int expTotal)
    {
        int nivel = CalcularNivelPorExpTotal(expTotal);
        int limiarAtual = ExpTotalNecessariaParaAlcancarNivel(nivel);
        int limiarProximo = ExpTotalNecessariaParaAlcancarNivel(nivel + 1);
        return Mathf.Max(0, limiarProximo - limiarAtual);
    }

    public static int ObterExpNecessariaParaProximoNivel(int expTotal)
    {
        return ObterExpRestanteParaProximoNivel(expTotal);
    }

    public static int ObterExpRestanteParaProximoNivel(int expTotal)
    {
        int nivel = CalcularNivelPorExpTotal(expTotal);
        int limiarProximo = ExpTotalNecessariaParaAlcancarNivel(nivel + 1);
        return Mathf.Max(0, limiarProximo - expTotal);
    }

    public static float CalcularProgressoDentroDoNivel01(int expTotal)
    {
        int no = ObterExpAcumuladaNoNivelAtual(expTotal);
        int span = ObterExpTotalParaCompletarSegmentoDoNivelAtual(expTotal);
        if (span <= 0)
            return 1f;
        return Mathf.Clamp01(no / (float)span);
    }
}
