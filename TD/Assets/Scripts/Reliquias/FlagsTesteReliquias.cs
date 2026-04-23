/// <summary>Interruptores temporários para facilitar testes. Desligue antes do release.</summary>
public static class FlagsTesteReliquias
{
    /// <summary>Se true, novos saves e normalização garantem Carvão-Enxofre-Salitre possuída e no slot 0 para qualquer raça.</summary>
    public const bool TodasRacasComecamComCarvaoEnxofreSalitre = false;

    /// <summary>Se true, novos saves e normalização garantem Bússola do Verme possuída e no slot 1 para qualquer raça.</summary>
    public const bool TodasRacasComecamComBussolaVerme = true;
}
