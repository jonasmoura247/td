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
                nomeExibicao = "Goblin Guerreiro",
                descricaoTooltip = "Guerreiro goblin comum das ondas.",
                icone = sprGuerreiro,
                iconeSilhueta = sprGuerreiro,
                marcosMortes = new[] { 100, 300, 500 },
                pontosPorEstagio = new[] { 1, 2, 3 }
            },
            new BestiarioCriaturaDef
            {
                id = "gob_lanceiro",
                nomeExibicao = "Goblin Lanceiro",
                descricaoTooltip = "Lanceiro goblin que atira no castelo.",
                icone = CarregarSpriteGoblinLanceiro(),
                iconeSilhueta = CarregarSpriteGoblinLanceiro(),
                marcosMortes = new[] { 100, 300, 500 },
                pontosPorEstagio = new[] { 1, 2, 3 }
            }
        };

        db.runas = new[]
        {
            new BestiarioRunaDef
            {
                id = "runa_eco_guerreiro",
                nomeExibicao = "Eco de dor",
                descricaoTooltip = "Chance de sofrer dano extra ao receber dano (fracao da vida maxima).",
                criaturaAlvoId = "gob_guerreiro",
                custoPontos = 2,
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
                criaturaAlvoId = "gob_guerreiro",
                custoPontos = 3,
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
