#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class BestiarioCriarDatabaseMenu
{
    private const string DirResources = "Assets/Resources/Bestiario";
    private const string PathAsset = DirResources + "/BestiarioDatabase.asset";

    /// <summary>Para Unity -batchmode -executeMethod BestiarioCriarDatabaseMenu.CriarDatabaseBatch</summary>
    public static void CriarDatabaseBatch()
    {
        CriarDatabase();
    }

    [MenuItem("TD/Bestiario/Criar database em Resources")]
    public static void CriarDatabase()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(DirResources))
            AssetDatabase.CreateFolder("Assets/Resources", "Bestiario");

        Sprite sprGuerreiro = CarregarSpriteGoblinGuerreiro();

        var db = ScriptableObject.CreateInstance<BestiarioDatabase>();
        db.chanceCriticoBase = 0.2f;
        db.multiplicadorCriticoBase = 1.2f;

        db.criaturas = new[]
        {
            new BestiarioCriaturaDef
            {
                id = "gob_guerreiro",
                racaId = "Goblin",
                nivelDificuldade = NivelDificuldadeBestiario.Facil,
                nomeExibicao = "Goblin Guerreiro",
                descricaoTooltip = "Guerreiro goblin comum das ondas.",
                icone = sprGuerreiro,
                iconeSilhueta = sprGuerreiro,
                marcosMortes = new[] { 50, 150, 300 },
                pontosPorEstagio = new[] { 10, 10, 10 }
            },
            new BestiarioCriaturaDef
            {
                id = "gob_lanceiro",
                racaId = "Goblin",
                nivelDificuldade = NivelDificuldadeBestiario.Facil,
                nomeExibicao = "Goblin Lanceiro",
                descricaoTooltip = "Lanceiro goblin que atira no castelo.",
                icone = CarregarSpriteGoblinLanceiro(),
                iconeSilhueta = CarregarSpriteGoblinLanceiro(),
                marcosMortes = new[] { 50, 150, 300 },
                pontosPorEstagio = new[] { 10, 10, 10 }
            }
        };

        db.runas = new[]
        {
            new BestiarioRunaDef
            {
                id = "runa_eco_guerreiro",
                nomeExibicao = "Eco de dor",
                descricaoTooltip = "Chance de sofrer dano extra ao receber dano (fracao da vida maxima).",
                descricaoCatalogo = "Inimigos com esta runa podem sofrer um segundo valor de dano quando o jogador os atinge.",
                icone = sprGuerreiro,
                iconeSilhueta = sprGuerreiro,
                criaturaAlvoId = "",
                custoPontosLiberacao = 50,
                estagiosMinimos = 3,
                tipoEfeito = TipoEfeitoRuna.DanoEcoAoReceberDano,
                parametroChance = 0.15f,
                parametroValor = 0.05f
            },
            new BestiarioRunaDef
            {
                id = "runa_crit_guerreiro",
                nomeExibicao = "Runa do critico brutal",
                descricaoTooltip = "Criticos ao castelo usam 150% em vez de 120%.",
                descricaoCatalogo = "Aumenta o multiplicador de dano critico quando o goblin atinge o castelo.",
                icone = sprGuerreiro,
                iconeSilhueta = sprGuerreiro,
                criaturaAlvoId = "",
                custoPontosLiberacao = 60,
                estagiosMinimos = 3,
                tipoEfeito = TipoEfeitoRuna.CriticoMultiplicadorAtaque,
                parametroChance = 0f,
                parametroValor = 1.5f
            }
        };

        AssetDatabase.CreateAsset(db, PathAsset);
        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(db);
        Selection.activeObject = db;
        Debug.Log("[Bestiario] Criado " + PathAsset + ". Use TD/Bestiario/Configurar UI no lobby na cena aberta.");
    }

    private static Sprite CarregarSpriteGoblinGuerreiro()
    {
        string[] guids = AssetDatabase.FindAssets("GoblinGuerreiro_Prefab t:Prefab");
        foreach (string g in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            if (p.Contains("Prefabs/Characters/Goblins"))
            {
                var sr = AssetDatabase.LoadAssetAtPath<GameObject>(p)?.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                    return sr.sprite;
            }
        }
        return null;
    }

    private static Sprite CarregarSpriteGoblinLanceiro()
    {
        string[] guids = AssetDatabase.FindAssets("GoblinLanceiro t:Prefab");
        foreach (string g in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            if (p.Contains("Prefabs/Characters/Goblins"))
            {
                var sr = AssetDatabase.LoadAssetAtPath<GameObject>(p)?.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                    return sr.sprite;
            }
        }
        return null;
    }
}
#endif
