using UnityEngine;

public static class BestiarioCatalogoUtil
{
    private const string CaminhoResources = "Bestiario/BestiarioDatabase";

    public static BestiarioDatabase CarregarOuNull(BestiarioDatabase referencia)
    {
        if (referencia != null)
            return referencia;
        return Resources.Load<BestiarioDatabase>(CaminhoResources);
    }
}
