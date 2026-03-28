#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PainelArvoreTalentos))]
public class PainelArvoreTalentosEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space(6f);
        EditorGUILayout.HelpBox(
            "Content (Scroll View): com Content Altura Automatica desligado, defina largura e altura no Rect Transform do objeto Area Lista Nos. Com a opcao ligada, a altura e calculada pelo conteudo e o campo Height e sobrescrito.",
            MessageType.Info);
        EditorGUILayout.Space(8f);
        var painel = (PainelArvoreTalentos)target;
        using (new EditorGUI.DisabledScope(Application.isPlaying))
        {
            if (GUILayout.Button("Gerar preview no Editor (sem Play)", GUILayout.Height(28f)))
                painel.MontarPreviewNoEditor();
            if (GUILayout.Button("Limpar area da lista"))
                painel.LimparAreaListaNoEditor();
        }
        if (Application.isPlaying)
            EditorGUILayout.HelpBox("Preview no Editor fica desativado durante o Play. Use o jogo normalmente.", MessageType.Info);
    }
}
#endif
