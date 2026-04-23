#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Menu: TD/Setup Sistemas (Planos 1-7)
/// Cria automaticamente os ScriptableObjects necessários para os sistemas
/// implementados nos Planos 1–7 (AntiExploit, Tendas, Relíquias, Herói Pirata, Contratos).
/// </summary>
public static class SetupSistemasPlanos
{
    private const string DirDataHerois     = "Assets/Data/Herois";
    private const string DirPrefabsHerois  = "Assets/Prefabs/Herois";
    private const string DirPrefabsUI      = "Assets/Prefabs/UI/Herois";
    private const string PathHeroiDatabase = DirDataHerois + "/HeroiDatabase.asset";
    private const string PathHeroiPirata   = DirDataHerois + "/HeroiDef_Pirata.asset";

    // Caminhos dos ReliquiaDef existentes
    private const string PathRelOlho    = "Assets/Prefabs/Reliquias/ReliquiaDef.asset";
    private const string PathRelBussola = "Assets/Data/Reliquias/ReliquiaDef_BussolaVerme.asset";
    private const string PathRelCarvao  = "Assets/Prefabs/Reliquias/ReliquiaDef_CarvaoEnxofreSalitre.asset";

    [MenuItem("TD/Setup Sistemas (Planos 1-7)")]
    public static void Executar()
    {
        CriarPastas();
        var pirata = CriarOuAtualizarHeroiDefPirata();
        CriarOuAtualizarHeroiDatabase(pirata);
        AtualizarRaridadesReliquias();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "[SetupSistemas] ✓ ScriptableObjects criados e configurados!\n" +
            "Próximos passos MANUAIS:\n" +
            "  1. Crie o Prefab do Pirata (veja Assets/Prefabs/Herois/)\n" +
            "  2. Arraste o prefab no HeroiDef_Pirata → campo Prefab\n" +
            "  3. Adicione HeroisManager na cena e arraste o HeroiDatabase\n" +
            "  4. Adicione SistemaTendasManager na cena\n" +
            "  5. Monte o painel de UI de heróis no Canvas do Lobby\n" +
            "Consulte o guia: Planos/setup_herois.html"
        );

        // Abre o HeroiDatabase no Inspector para conferir
        var db = AssetDatabase.LoadAssetAtPath<HeroiDatabase>(PathHeroiDatabase);
        if (db != null)
        {
            EditorGUIUtility.PingObject(db);
            Selection.activeObject = db;
        }
    }

    // ── Pastas ────────────────────────────────────────────────────────────

    private static void CriarPastas()
    {
        GarantirPasta("Assets", "Data");
        GarantirPasta("Assets/Data", "Herois");
        GarantirPasta("Assets/Prefabs", "Herois");
        GarantirPasta("Assets/Prefabs", "UI");
        GarantirPasta("Assets/Prefabs/UI", "Herois");
        Debug.Log("[SetupSistemas] Pastas garantidas.");
    }

    private static void GarantirPasta(string pai, string filho)
    {
        if (!AssetDatabase.IsValidFolder($"{pai}/{filho}"))
            AssetDatabase.CreateFolder(pai, filho);
    }

    // ── HeroiDef Pirata ───────────────────────────────────────────────────

    private static HeroiDef CriarOuAtualizarHeroiDefPirata()
    {
        var existente = AssetDatabase.LoadAssetAtPath<HeroiDef>(PathHeroiPirata);
        if (existente != null)
        {
            Debug.Log("[SetupSistemas] HeroiDef_Pirata já existe — mantendo.");
            return existente;
        }

        var def = ScriptableObject.CreateInstance<HeroiDef>();
        def.idHeroi      = HeroiIds.Pirata;
        def.nomeExibicao = "Pirata";
        def.raridade     = HeroiRaridade.Azul;
        def.descricao    = "O primeiro herói do jogo. Combatente versátil nos mares e em terra.";

        // Tenta carregar sprite do Pirata automaticamente
        def.icone = CarregarSpritePirata();

        AssetDatabase.CreateAsset(def, PathHeroiPirata);
        Debug.Log("[SetupSistemas] HeroiDef_Pirata criado em " + PathHeroiPirata);
        return def;
    }

    private static Sprite CarregarSpritePirata()
    {
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites/Herois/Pirate" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var spr = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (spr != null) return spr;
        }
        return null;
    }

    // ── HeroiDatabase ─────────────────────────────────────────────────────

    private static void CriarOuAtualizarHeroiDatabase(HeroiDef pirata)
    {
        var existente = AssetDatabase.LoadAssetAtPath<HeroiDatabase>(PathHeroiDatabase);

        if (existente != null)
        {
            // Garante que o pirata está registrado
            bool jaTemPirata = false;
            if (existente.entradas != null)
                foreach (var e in existente.entradas)
                    if (e != null && e.idHeroi == HeroiIds.Pirata)
                    { jaTemPirata = true; break; }

            if (!jaTemPirata && pirata != null)
            {
                int tamanhoAnterior = existente.entradas?.Length ?? 0;
                var novasEntradas = new HeroiDef[tamanhoAnterior + 1];
                for (int i = 0; i < tamanhoAnterior; i++)
                    novasEntradas[i] = existente.entradas[i];
                novasEntradas[tamanhoAnterior] = pirata;
                existente.entradas = novasEntradas;
                EditorUtility.SetDirty(existente);
                Debug.Log("[SetupSistemas] Pirata adicionado ao HeroiDatabase existente.");
            }
            else
            {
                Debug.Log("[SetupSistemas] HeroiDatabase já existe e já contém o Pirata.");
            }
            return;
        }

        var db = ScriptableObject.CreateInstance<HeroiDatabase>();
        db.entradas = pirata != null ? new HeroiDef[] { pirata } : new HeroiDef[0];
        AssetDatabase.CreateAsset(db, PathHeroiDatabase);
        Debug.Log("[SetupSistemas] HeroiDatabase criado em " + PathHeroiDatabase);
    }

    // ── Raridades das Relíquias ───────────────────────────────────────────

    private static void AtualizarRaridadesReliquias()
    {
        AtualizarRaridade(PathRelOlho,    RaridadeReliquia.Incomum, "OlhoQueNadaVe");
        AtualizarRaridade(PathRelBussola, RaridadeReliquia.Raro,    "BussolaVerme");
        AtualizarRaridade(PathRelCarvao,  RaridadeReliquia.Comum,   "CarvaoEnxofreSalitre");
    }

    private static void AtualizarRaridade(string path, RaridadeReliquia raridade, string nome)
    {
        var def = AssetDatabase.LoadAssetAtPath<ReliquiaDef>(path);
        if (def == null)
        {
            Debug.LogWarning($"[SetupSistemas] ReliquiaDef não encontrado: {path}");
            return;
        }
        if (def.raridade == raridade) return;
        def.raridade = raridade;
        EditorUtility.SetDirty(def);
        Debug.Log($"[SetupSistemas] {nome} → raridade {raridade}");
    }
}
#endif
