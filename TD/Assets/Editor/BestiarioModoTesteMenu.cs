#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class BestiarioModoTesteMenu
{
    private const string MenuRaiz = "TD/Bestiario/Modo teste runas";

    [MenuItem(MenuRaiz + "/Alternar flag global (log)")]
    private static void AlternarFlag()
    {
        BestiarioModoTeste.Ativo = !BestiarioModoTeste.Ativo;
        Debug.Log($"[Bestiario] Modo teste runas = {BestiarioModoTeste.Ativo} (use também o checkbox no PainelBestiario em Play).");
    }
}
#endif
