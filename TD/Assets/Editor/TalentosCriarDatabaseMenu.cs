#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class TalentosCriarDatabaseMenu
{
    [MenuItem("TD/Talentos/Criar database Goblin em Resources")]
    public static void CriarDatabaseGoblin()
    {
        const string dir = "Assets/Resources/Talentos";
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets/Resources", "Talentos");

        var db = ScriptableObject.CreateInstance<TalentTreeDatabase>();
        db.niveisPorNivelGanho = 2;
        db.pontosPorGanho = 1;

        var bundle = new TalentRaceBundle
        {
            raca = TipoRaca.Goblin,
            nos = CriarNosGoblinExemplo()
        };
        db.arvoresPorRaca = new[] { bundle };

        const string path = dir + "/TalentTreeDatabase.asset";
        AssetDatabase.CreateAsset(db, path);
        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(db);
        Selection.activeObject = db;
        Debug.Log("[Talentos] Criado: " + path + ". Arraste no WaveManager (campo Talent Tree Database) ou use Resources.Load.");
    }

    private static TalentNodeEntry[] CriarNosGoblinExemplo()
    {
        return new[]
        {
            new TalentNodeEntry
            {
                id = "gob_l2_a",
                nomeExibicao = "Forca guerreiro",
                descricaoTooltip = "Guerreiros goblin causam +1 de dano.",
                coluna = 0,
                linha = 0,
                custoPontos = 1,
                nivelMinimoRaca = 2,
                grupoEscolhaId = "gob_lvl2",
                requisitosItemIds = new string[0],
                efeitos = new[]
                {
                    new TalentEffectEntry
                    {
                        alvo = AlvoEfeitoTalento.MesmaRacaPerfil,
                        subtipoFiltro = UnidadeSubtipo.Guerreiro,
                        racaAlvoInimigo = TipoRaca.Goblin,
                        stat = StatTalento.Dano,
                        valorFlat = 1f,
                        valorPercentual = 0f
                    }
                }
            },
            new TalentNodeEntry
            {
                id = "gob_l2_b",
                nomeExibicao = "Resistencia lanceiro",
                descricaoTooltip = "Lanceiros goblin ganham +1 de vida maxima.",
                coluna = 1,
                linha = 0,
                custoPontos = 1,
                nivelMinimoRaca = 2,
                grupoEscolhaId = "gob_lvl2",
                requisitosItemIds = new string[0],
                efeitos = new[]
                {
                    new TalentEffectEntry
                    {
                        alvo = AlvoEfeitoTalento.MesmaRacaPerfil,
                        subtipoFiltro = UnidadeSubtipo.Lanceiro,
                        racaAlvoInimigo = TipoRaca.Goblin,
                        stat = StatTalento.VidaMaxima,
                        valorFlat = 1f,
                        valorPercentual = 0f
                    }
                }
            },
            new TalentNodeEntry
            {
                id = "gob_l2_c",
                nomeExibicao = "Precisao tribal",
                descricaoTooltip = "Todos os goblins ganham +5% de dano.",
                coluna = 2,
                linha = 0,
                custoPontos = 1,
                nivelMinimoRaca = 2,
                grupoEscolhaId = "gob_lvl2",
                requisitosItemIds = new string[0],
                efeitos = new[]
                {
                    new TalentEffectEntry
                    {
                        alvo = AlvoEfeitoTalento.MesmaRacaPerfil,
                        subtipoFiltro = UnidadeSubtipo.Indefinido,
                        racaAlvoInimigo = TipoRaca.Goblin,
                        stat = StatTalento.Dano,
                        valorFlat = 0f,
                        valorPercentual = 5f
                    }
                }
            },
            new TalentNodeEntry
            {
                id = "gob_l10_adaga",
                nomeExibicao = "Ritual da adaga",
                descricaoTooltip = "Requer Adaga Enferrujada no inventario. +15 de dano para todos os goblins.",
                coluna = 1,
                linha = 1,
                custoPontos = 2,
                nivelMinimoRaca = 10,
                grupoEscolhaId = "",
                requisitosItemIds = new[] { "Adaga Enferrujada" },
                efeitos = new[]
                {
                    new TalentEffectEntry
                    {
                        alvo = AlvoEfeitoTalento.MesmaRacaPerfil,
                        subtipoFiltro = UnidadeSubtipo.Indefinido,
                        racaAlvoInimigo = TipoRaca.Goblin,
                        stat = StatTalento.Dano,
                        valorFlat = 15f,
                        valorPercentual = 0f
                    }
                }
            },
            new TalentNodeEntry
            {
                id = "gob_debuff_humano",
                nomeExibicao = "Odiar humanos",
                descricaoTooltip = "Humanos inimigos possuem -5% de vida maxima.",
                coluna = 0,
                linha = 1,
                custoPontos = 1,
                nivelMinimoRaca = 4,
                grupoEscolhaId = "",
                requisitosItemIds = new string[0],
                efeitos = new[]
                {
                    new TalentEffectEntry
                    {
                        alvo = AlvoEfeitoTalento.InimigoDeRaca,
                        subtipoFiltro = UnidadeSubtipo.Indefinido,
                        racaAlvoInimigo = TipoRaca.Humano,
                        stat = StatTalento.VidaMaxima,
                        valorFlat = 0f,
                        valorPercentual = -5f
                    }
                }
            }
        };
    }
}
#endif
