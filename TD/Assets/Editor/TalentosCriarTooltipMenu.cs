#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class TalentosCriarTooltipMenu
{
    [MenuItem("TD/Talentos/Criar TooltipTalentos no Canvas (para editar estilo)")]
    public static void CriarTooltipNaCena()
    {
        Canvas canvas = ResolverCanvas();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog(
                "Tooltip talentos",
                "Nenhum Canvas encontrado.\n\nSelecione um GameObject com componente Canvas na Hierarchy e tente de novo, ou adicione um Canvas na cena.",
                "OK");
            return;
        }

        var existente = canvas.GetComponentInChildren<TalentoTooltipUI>(true);
        if (existente != null)
        {
            Selection.activeGameObject = existente.gameObject;
            EditorGUIUtility.PingObject(existente);
            EditorUtility.DisplayDialog(
                "Tooltip talentos",
                "Ja existe um TooltipTalentos neste Canvas. Ele foi selecionado na Hierarchy — e so ajustar fonte e fundo no Inspector.",
                "OK");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, "Criar TooltipTalentos");
        var tt = TalentoTooltipUI.CriarTooltipFilhoDoCanvas(canvas);
        if (tt != null)
        {
            EditorUtility.SetDirty(canvas.gameObject);
            Selection.activeGameObject = tt.gameObject;
            EditorGUIUtility.PingObject(tt);
        }
    }

    private static Canvas ResolverCanvas()
    {
        GameObject sel = Selection.activeGameObject;
        if (sel != null)
        {
            var c = sel.GetComponent<Canvas>();
            if (c != null)
                return c;
            c = sel.GetComponentInParent<Canvas>();
            if (c != null)
                return c;
        }

        return Object.FindFirstObjectByType<Canvas>();
    }
}
#endif
