using UnityEditor;
using UnityEngine;

/// <summary>
/// Menu: Tower Defense > Criar Bandeiras Mapa Natural
/// Cria 10 objetos BandeiraFase no world space, prontos para posicionar.
/// Sprites das bandeiras devem ser arrastados no Inspector de cada um.
/// </summary>
public static class BandeiraFaseSetup
{
    private static readonly (string faseId, string preRequisito)[] Fases =
    {
        ("natural_1",  ""),
        ("natural_2",  "natural_1"),
        ("natural_3",  "natural_2"),
        ("natural_4",  "natural_3"),
        ("natural_5",  "natural_4"),
        ("natural_6",  "natural_5"),
        ("natural_7",  "natural_6"),
        ("natural_8",  "natural_7"),
        ("natural_9",  "natural_8"),
        ("natural_10", "natural_9"),
    };

    [MenuItem("Tower Defense/Criar Bandeiras Mapa Natural")]
    public static void CriarBandeiras()
    {
        // Container pai para organizar a hierarquia
        var container = new GameObject("Bandeiras_Natural");
        Undo.RegisterCreatedObjectUndo(container, "Criar Bandeiras Natural");

        for (int i = 0; i < Fases.Length; i++)
        {
            var (faseId, preReq) = Fases[i];

            var go = new GameObject($"Bandeira_{i + 1}");
            go.transform.SetParent(container.transform);

            // Posiciona em linha para facilitar reposicionamento manual
            go.transform.localPosition = new Vector3(i * 3f, 0f, 0f);

            go.AddComponent<SpriteRenderer>();

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 2f);

            var ctrl = go.AddComponent<BandeiraFaseController>();

            // Preenche os campos via SerializedObject para respeitar o Undo
            var so = new SerializedObject(ctrl);
            so.FindProperty("faseId").stringValue = faseId;
            so.FindProperty("nomeCena").stringValue = "Mapa_1";
            so.FindProperty("fasePreRequisito").stringValue = preReq;
            so.ApplyModifiedPropertiesWithoutUndo();

            Undo.RegisterCreatedObjectUndo(go, "Criar Bandeiras Natural");
        }

        Selection.activeGameObject = container;
        Debug.Log("[BandeiraFaseSetup] 10 bandeiras criadas em 'Bandeiras_Natural'. " +
                  "Arraste os sprites no Inspector de cada uma e reposicione no mapa.");
    }
}
