#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>Presets de marcos/pontos por nivel e validacao de racaId no BestiarioDatabase.</summary>
public static class BestiarioDatabaseEditorPresets
{
    private static bool TentarObterDatabaseSelecionado(out BestiarioDatabase db)
    {
        db = Selection.activeObject as BestiarioDatabase;
        if (db != null)
            return true;
        EditorUtility.DisplayDialog("Bestiario", "Selecione um asset BestiarioDatabase na Project window.", "OK");
        return false;
    }

    [MenuItem("TD/Bestiario/Preset Facil (50/150/300, 10 pts) — criaturas selecionadas no asset")]
    public static void AplicarPresetFacilNosElementos()
    {
        if (!TentarObterDatabaseSelecionado(out var db))
            return;
        Undo.RecordObject(db, "Preset Facil bestiario");
        foreach (var c in db.criaturas)
        {
            if (c == null)
                continue;
            c.nivelDificuldade = NivelDificuldadeBestiario.Facil;
            c.marcosMortes = new[] { 50, 150, 300 };
            c.pontosPorEstagio = new[] { 10, 10, 10 };
        }
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        Debug.Log("[Bestiario] Preset Facil aplicado a todas as criaturas do database selecionado.");
    }

    [MenuItem("TD/Bestiario/Preset Medio (100/250/500, 15 pts) — criaturas selecionadas no asset")]
    public static void AplicarPresetMedioNosElementos()
    {
        if (!TentarObterDatabaseSelecionado(out var db))
            return;
        Undo.RecordObject(db, "Preset Medio bestiario");
        foreach (var c in db.criaturas)
        {
            if (c == null)
                continue;
            c.nivelDificuldade = NivelDificuldadeBestiario.Medio;
            c.marcosMortes = new[] { 100, 250, 500 };
            c.pontosPorEstagio = new[] { 15, 15, 15 };
        }
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        Debug.Log("[Bestiario] Preset Medio aplicado a todas as criaturas do database selecionado.");
    }

    [MenuItem("TD/Bestiario/Preset Dificil (160/400/800, 20 pts) — criaturas selecionadas no asset")]
    public static void AplicarPresetDificilNosElementos()
    {
        if (!TentarObterDatabaseSelecionado(out var db))
            return;
        Undo.RecordObject(db, "Preset Dificil bestiario");
        foreach (var c in db.criaturas)
        {
            if (c == null)
                continue;
            c.nivelDificuldade = NivelDificuldadeBestiario.Dificil;
            c.marcosMortes = new[] { 160, 400, 800 };
            c.pontosPorEstagio = new[] { 20, 20, 20 };
        }
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        Debug.Log("[Bestiario] Preset Dificil aplicado a todas as criaturas do database selecionado.");
    }

    [MenuItem("TD/Bestiario/Preset Expert (240/600/1200, 25 pts) — criaturas selecionadas no asset")]
    public static void AplicarPresetExpertNosElementos()
    {
        if (!TentarObterDatabaseSelecionado(out var db))
            return;
        Undo.RecordObject(db, "Preset Expert bestiario");
        foreach (var c in db.criaturas)
        {
            if (c == null)
                continue;
            c.nivelDificuldade = NivelDificuldadeBestiario.Expert;
            c.marcosMortes = new[] { 240, 600, 1200 };
            c.pontosPorEstagio = new[] { 25, 25, 25 };
        }
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        Debug.Log("[Bestiario] Preset Expert aplicado a todas as criaturas do database selecionado.");
    }

    [MenuItem("TD/Bestiario/Validar racaId no database selecionado (Console)")]
    public static void ValidarRacaId()
    {
        if (!TentarObterDatabaseSelecionado(out var db))
            return;
        int avisos = 0;
        if (db.criaturas == null)
            return;
        for (int i = 0; i < db.criaturas.Length; i++)
        {
            var c = db.criaturas[i];
            if (c == null || string.IsNullOrEmpty(c.id))
                continue;
            if (string.IsNullOrEmpty(c.racaId))
            {
                Debug.LogWarning($"[Bestiario] Criatura '{c.id}' sem racaId — nao entra na grelha filtrada por raca do jogador.");
                avisos++;
            }
        }
        if (avisos == 0)
            Debug.Log("[Bestiario] Todas as criaturas com id tem racaId preenchido.");
    }
}
#endif
