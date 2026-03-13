#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;

/// <summary>
/// Adiciona o sistema de inventário (Inventario, LootManager, UIInventario, BotaoInventario)
/// à cena ativa. Use quando a cena Lobby estiver aberta.
/// Menu: Ferramentas > Adicionar Inventário à Cena
/// </summary>
public static class ConfiguradorInventarioLobby
{
    const string CAMINHO_PREFAB_INVENTARIO = "Assets/Prefabs/UI/Inventario/Inventario.prefab";
    const string CAMINHO_PREFAB_INVENTARIO_FALLBACK = "Assets/Scripts/Loot/Inventario.prefab";
    const string CAMINHO_PREFAB_SLOT = "Assets/Prefabs/UI/Inventario/SlotItem.prefab";
    const string CAMINHO_PREFAB_SLOT_FALLBACK = "Assets/Scripts/Loot/SlotItem.prefab";
    const string CAMINHO_ITEM_DATABASE = "Assets/Scripts/Inventario/ItemDatabase.asset";

    [MenuItem("Ferramentas/Adicionar Inventário à Cena")]
    public static void AdicionarInventarioACena()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("[ConfiguradorInventario] Saia do modo Play para adicionar o inventario.");
            return;
        }
        var inventarioPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CAMINHO_PREFAB_INVENTARIO)
            ?? AssetDatabase.LoadAssetAtPath<GameObject>(CAMINHO_PREFAB_INVENTARIO_FALLBACK);
        var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CAMINHO_PREFAB_SLOT)
            ?? AssetDatabase.LoadAssetAtPath<GameObject>(CAMINHO_PREFAB_SLOT_FALLBACK);
        var itemDb = AssetDatabase.LoadAssetAtPath<ItemDatabase>(CAMINHO_ITEM_DATABASE);

        if (inventarioPrefab == null)
        {
            Debug.LogError($"[ConfiguradorInventario] Prefab Inventario nao encontrado. Procure em: {CAMINHO_PREFAB_INVENTARIO} ou {CAMINHO_PREFAB_INVENTARIO_FALLBACK}");
            return;
        }
        if (slotPrefab == null)
        {
            Debug.LogError($"[ConfiguradorInventario] Prefab SlotItem nao encontrado. Procure em: {CAMINHO_PREFAB_SLOT} ou {CAMINHO_PREFAB_SLOT_FALLBACK}");
            return;
        }
        if (itemDb == null)
        {
            Debug.LogError("[ConfiguradorInventario] ItemDatabase nao encontrado em: " + CAMINHO_ITEM_DATABASE);
            return;
        }

        string pathInv = AssetDatabase.GetAssetPath(inventarioPrefab);
        string pathSlot = AssetDatabase.GetAssetPath(slotPrefab);
        Debug.Log($"[ConfiguradorInventario] Usando Inventario: {pathInv} | SlotItem: {pathSlot}");

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[ConfiguradorInventario] Nenhum Canvas encontrado na cena!");
            return;
        }

        Undo.RegisterCompleteObjectUndo(canvas.gameObject, "Adicionar Inventario");

        if (Object.FindAnyObjectByType<LootManager>() != null)
        {
            Debug.LogWarning("[ConfiguradorInventario] LootManager ja existe na cena. Pulando criacao.");
        }
        else
        {
            Undo.RegisterCreatedObjectUndo(CriarLootManager(itemDb), "Criar LootManager");
        }

        if (Object.FindAnyObjectByType<UIInventario>() != null)
        {
            Debug.LogWarning("[ConfiguradorInventario] UIInventario ja existe na cena. Pulando criacao.");
        }
        else
        {
            GameObject inventarioInstance = (GameObject)PrefabUtility.InstantiatePrefab(inventarioPrefab, canvas.transform);
            Undo.RegisterCreatedObjectUndo(inventarioInstance, "Criar Inventario");
            inventarioInstance.name = "Inventario";
            RectTransform rtInventario = inventarioInstance.GetComponent<RectTransform>();
            if (rtInventario != null)
            {
                rtInventario.anchorMin = new Vector2(0.5f, 0.5f);
                rtInventario.anchorMax = new Vector2(0.5f, 0.5f);
                rtInventario.pivot = new Vector2(0.5f, 0.5f);
                rtInventario.anchoredPosition = Vector2.zero;
                rtInventario.sizeDelta = new Vector2(480, 360);
            }

            Transform painelInventario = inventarioInstance.transform.Find("PainelInventario");
            Transform gradeItens = painelInventario?.Find("GradeItens");

            if (painelInventario == null || gradeItens == null)
            {
                Debug.LogError("[ConfiguradorInventario] Estrutura do prefab Inventario invalida (PainelInventario/GradeItens nao encontrados).");
                return;
            }

            painelInventario.gameObject.SetActive(false);

            GameObject goUIInventario = new GameObject("UIInventario");
            Undo.RegisterCreatedObjectUndo(goUIInventario, "Criar UIInventario");
            UIInventario uiInv = goUIInventario.AddComponent<UIInventario>();
            uiInv.painelInventario = painelInventario.gameObject;
            uiInv.gradeItens = gradeItens;
            uiInv.prefabSlot = slotPrefab;

            CriarBotaoInventario(canvas.transform, uiInv);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[ConfiguradorInventario] Sistema de inventario adicionado com sucesso!");
    }

    static GameObject CriarLootManager(ItemDatabase itemDb)
    {
        GameObject go = new GameObject("LootManager");
        LootManager lm = go.AddComponent<LootManager>();
        SerializedObject so = new SerializedObject(lm);
        so.FindProperty("itemDatabase").objectReferenceValue = itemDb;
        so.ApplyModifiedPropertiesWithoutUndo();
        return go;
    }

    static void CriarBotaoInventario(Transform parentCanvas, UIInventario uiInventario)
    {
        GameObject botao = new GameObject("BotaoInventario");
        Undo.RegisterCreatedObjectUndo(botao, "Criar BotaoInventario");
        botao.transform.SetParent(parentCanvas, false);

        RectTransform rt = botao.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-80, -80);
        rt.sizeDelta = new Vector2(140, 44);

        Image img = botao.AddComponent<Image>();
        img.color = new Color(0.3f, 0.5f, 0.8f, 1f);
        img.raycastTarget = true;

        Button btn = botao.AddComponent<Button>();
        btn.targetGraphic = img;
        // AddPersistentListener serializa a chamada - AddListener com lambda nao funciona ao carregar a cena
        UnityEventTools.AddPersistentListener(btn.onClick, new UnityAction(uiInventario.AlternarInventario));

        GameObject texto = new GameObject("Text");
        texto.transform.SetParent(botao.transform, false);
        RectTransform rtTexto = texto.AddComponent<RectTransform>();
        rtTexto.anchorMin = Vector2.zero;
        rtTexto.anchorMax = Vector2.one;
        rtTexto.offsetMin = Vector2.zero;
        rtTexto.offsetMax = Vector2.zero;
        var tmp = texto.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = "Inventário";
        tmp.fontSize = 18;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }
}
#endif
