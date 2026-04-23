using UnityEditor;
using UnityEngine;

/// <summary>
/// Cria bandeiras de fase + setas de navegação em cada mapa mundo.
/// Menu: Tower Defense > Setup Mapas > ...
/// Posicione os objetos criados livremente no mapa após rodar o menu.
/// Substitua o sprite das setas pelo seu asset de seta depois.
/// </summary>
public static class MapaNavegacaoSetup
{
    // ── Definição dos mapas ───────────────────────────────────────────────

    private struct ConfigMapa
    {
        public string prefixoFase;
        public string cenaAnterior; // vazio = primeiro mapa
        public string cenaProxima;  // vazio = último mapa
    }

    private static readonly ConfigMapa[] Mapas =
    {
        new ConfigMapa { prefixoFase = "natural",    cenaAnterior = "",              cenaProxima = "Mapa_Gelo"       },
        new ConfigMapa { prefixoFase = "gelo",       cenaAnterior = "Mapa_Natural",  cenaProxima = "Mapa_Magma"      },
        new ConfigMapa { prefixoFase = "magma",      cenaAnterior = "Mapa_Gelo",     cenaProxima = "Mapa_Deserto"    },
        new ConfigMapa { prefixoFase = "deserto",    cenaAnterior = "Mapa_Magma",    cenaProxima = "Mapa_DarkFlorest"},
        new ConfigMapa { prefixoFase = "darkflorest",cenaAnterior = "Mapa_Deserto",  cenaProxima = ""                },
    };

    // ── Menus ─────────────────────────────────────────────────────────────

    [MenuItem("Tower Defense/Setup Mapas/Mapa Natural (bandeiras + setas)")]
    public static void SetupNatural()    => CriarSetupMapa(Mapas[0]);

    [MenuItem("Tower Defense/Setup Mapas/Mapa Gelo (bandeiras + setas)")]
    public static void SetupGelo()       => CriarSetupMapa(Mapas[1]);

    [MenuItem("Tower Defense/Setup Mapas/Mapa Magma (bandeiras + setas)")]
    public static void SetupMagma()      => CriarSetupMapa(Mapas[2]);

    [MenuItem("Tower Defense/Setup Mapas/Mapa Deserto (bandeiras + setas)")]
    public static void SetupDeserto()    => CriarSetupMapa(Mapas[3]);

    [MenuItem("Tower Defense/Setup Mapas/Mapa Dark Florest (bandeiras + setas)")]
    public static void SetupDarkFlorest()=> CriarSetupMapa(Mapas[4]);

    // ── Lógica ────────────────────────────────────────────────────────────

    private static void CriarSetupMapa(ConfigMapa cfg)
    {
        CriarBandeiras(cfg.prefixoFase);
        CriarSetas(cfg.cenaAnterior, cfg.cenaProxima);

        Debug.Log($"[MapaNavegacaoSetup] Setup '{cfg.prefixoFase}' criado. " +
                  "Posicione os objetos no mapa e arraste os sprites de seta no Inspector.");
    }

    private static void CriarBandeiras(string prefixo)
    {
        var container = new GameObject($"Bandeiras_{prefixo}");
        Undo.RegisterCreatedObjectUndo(container, $"Criar Bandeiras {prefixo}");

        for (int i = 1; i <= 10; i++)
        {
            string faseId  = $"{prefixo}_{i}";
            string preReq  = i == 1 ? "" : $"{prefixo}_{i - 1}";

            var go = new GameObject($"Bandeira_{i}");
            go.transform.SetParent(container.transform);
            go.transform.localPosition = new Vector3((i - 1) * 3f, 0f, 0f);

            go.AddComponent<SpriteRenderer>();
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 2f);

            var ctrl = go.AddComponent<BandeiraFaseController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("faseId").stringValue = faseId;
            so.FindProperty("nomeCena").stringValue = "Mapa_1";
            so.FindProperty("fasePreRequisito").stringValue = preReq;
            so.ApplyModifiedPropertiesWithoutUndo();

            Undo.RegisterCreatedObjectUndo(go, $"Criar Bandeiras {prefixo}");
        }
    }

    private static void CriarSetas(string cenaAnterior, string cenaProxima)
    {
        var containerSetas = new GameObject("Setas_Navegacao");
        Undo.RegisterCreatedObjectUndo(containerSetas, "Criar Setas Navegacao");

        if (!string.IsNullOrEmpty(cenaAnterior))
        {
            var seta = CriarObjetoSeta($"Seta_Anterior ({cenaAnterior})", cenaAnterior);
            seta.transform.SetParent(containerSetas.transform);
            seta.transform.localPosition = new Vector3(-5f, 0f, 0f);
            Undo.RegisterCreatedObjectUndo(seta, "Criar Setas Navegacao");
        }

        if (!string.IsNullOrEmpty(cenaProxima))
        {
            var seta = CriarObjetoSeta($"Seta_Proxima ({cenaProxima})", cenaProxima);
            seta.transform.SetParent(containerSetas.transform);
            seta.transform.localPosition = new Vector3(5f, 0f, 0f);
            Undo.RegisterCreatedObjectUndo(seta, "Criar Setas Navegacao");
        }

        Selection.activeGameObject = containerSetas;
    }

    private static GameObject CriarObjetoSeta(string nome, string cenaDestino)
    {
        var go = new GameObject(nome);
        go.AddComponent<SpriteRenderer>(); // arraste o sprite de seta aqui
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.5f, 1.5f);

        var ctrl = go.AddComponent<SetaNavegacaoMapa>();
        var so = new SerializedObject(ctrl);
        so.FindProperty("nomeCenaDestino").stringValue = cenaDestino;
        so.ApplyModifiedPropertiesWithoutUndo();

        return go;
    }
}
