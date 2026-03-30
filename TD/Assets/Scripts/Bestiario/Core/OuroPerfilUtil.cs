using UnityEngine;

/// <summary>Sincroniza o ouro persistido no slot com <see cref="OuroManager"/> quando o singleton existe (ex.: lobby).</summary>
public static class OuroPerfilUtil
{
    public static void AplicarOuroDoPerfilAoManager(DadosSlotSave dados)
    {
        if (dados == null || OuroManager.Instance == null)
            return;
        OuroManager.Instance.SetOuro(Mathf.Max(0, dados.ouro));
    }
}
