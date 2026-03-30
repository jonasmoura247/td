using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class BestiarioCriaturaDef
{
    public string id;
    /// <summary>Raça/família da criatura (ex.: Goblin, Humano). Usado para excluir a grelha do bestiário da raça do jogador.</summary>
    public string racaId;
    public NivelDificuldadeBestiario nivelDificuldade = NivelDificuldadeBestiario.Facil;
    public string nomeExibicao;
    [TextArea(2, 6)]
    public string descricaoTooltip;
    public Sprite icone;
    public Sprite iconeSilhueta;
    /// <summary>Marcos de mortes para cada estágio (ex.: 100, 300, 500).</summary>
    public int[] marcosMortes = Array.Empty<int>();
    /// <summary>Pontos de bestiário por estágio completado (mesmo comprimento que marcos).</summary>
    public int[] pontosPorEstagio = Array.Empty<int>();

    public int QuantidadeEstagios => marcosMortes != null ? marcosMortes.Length : 0;
}

[Serializable]
public class BestiarioRunaDef
{
    public string id;
    public string nomeExibicao;
    [TextArea(2, 6)]
    public string descricaoTooltip;
    /// <summary>Texto opcional sempre visível no cartão do catálogo de runas (aba "Todas as runas").</summary>
    [TextArea(2, 8)]
    public string descricaoCatalogo;
    public Sprite icone;
    public Sprite iconeSilhueta;
    /// <summary>Opcional: criatura para texto/lore ou foco no catálogo; não restringe equipamento.</summary>
    public string criaturaAlvoId;
    /// <summary>Pontos de bestiário gastos uma única vez ao <b>liberar</b> a runa no perfil.</summary>
    [FormerlySerializedAs("custoPontos")]
    public int custoPontosLiberacao = 1;
    /// <summary>Estágios completos necessários (ex.: 3 = ter passado o 3º marco).</summary>
    public int estagiosMinimos = 3;
    /// <summary>Se preenchido, o catálogo mostra isto em vez do texto automático de requisitos (liberar / equipar / lore).</summary>
    [TextArea(3, 10)]
    public string textoRequisitosCatalogo;
    public TipoEfeitoRuna tipoEfeito = TipoEfeitoRuna.Nenhum;
    [Tooltip("DanoEco: chance 0–1. Critico: ignorado.")]
    public float parametroChance = 0.15f;
    [Tooltip("DanoEco: fração da vida máxima (ex.: 0.05). Critico: multiplicador quando crítico (ex.: 1.5).")]
    public float parametroValor = 0.05f;
}

[CreateAssetMenu(fileName = "BestiarioDatabase", menuName = "TD/Bestiario/Bestiario Database")]
public class BestiarioDatabase : ScriptableObject
{
    [Header("Combate — crítico base (sem runa)")]
    [Range(0f, 1f)] public float chanceCriticoBase = 0.2f;
    public float multiplicadorCriticoBase = 1.2f;

    public BestiarioCriaturaDef[] criaturas = Array.Empty<BestiarioCriaturaDef>();
    public BestiarioRunaDef[] runas = Array.Empty<BestiarioRunaDef>();

    public BestiarioCriaturaDef ObterCriatura(string id)
    {
        if (string.IsNullOrEmpty(id) || criaturas == null)
            return null;
        for (int i = 0; i < criaturas.Length; i++)
        {
            if (criaturas[i] != null && criaturas[i].id == id)
                return criaturas[i];
        }
        return null;
    }

    /// <summary>Criaturas que o jogador pode registar no bestiário: exclui a raça do próprio perfil (não enfrenta a mesma família).</summary>
    public BestiarioCriaturaDef[] ObterCriaturasParaBestiarioDoJogador(TipoRaca racaJogador)
    {
        if (criaturas == null || criaturas.Length == 0)
            return Array.Empty<BestiarioCriaturaDef>();
        string idRacaJogador = BestiarioRacaIdUtil.ParaId(racaJogador);
        var lista = new List<BestiarioCriaturaDef>();
        for (int i = 0; i < criaturas.Length; i++)
        {
            var c = criaturas[i];
            if (c == null || string.IsNullOrEmpty(c.id))
                continue;
            if (string.IsNullOrEmpty(c.racaId))
                continue;
            if (string.Equals(c.racaId, idRacaJogador, StringComparison.Ordinal))
                continue;
            lista.Add(c);
        }
        return lista.ToArray();
    }

    /// <summary>Se <paramref name="racaFiltro"/> for vazio, devolve todas as criaturas válidas; senão filtra por <see cref="BestiarioCriaturaDef.racaId"/>.</summary>
    public BestiarioCriaturaDef[] ObterCriaturasPorRaca(string racaFiltro)
    {
        if (criaturas == null || criaturas.Length == 0)
            return Array.Empty<BestiarioCriaturaDef>();
        var lista = new List<BestiarioCriaturaDef>();
        for (int i = 0; i < criaturas.Length; i++)
        {
            var c = criaturas[i];
            if (c == null || string.IsNullOrEmpty(c.id))
                continue;
            if (string.IsNullOrEmpty(racaFiltro))
                lista.Add(c);
            else if (string.Equals(c.racaId, racaFiltro, StringComparison.Ordinal))
                lista.Add(c);
        }
        return lista.ToArray();
    }

    public BestiarioRunaDef ObterRuna(string id)
    {
        if (string.IsNullOrEmpty(id) || runas == null)
            return null;
        for (int i = 0; i < runas.Length; i++)
        {
            if (runas[i] != null && runas[i].id == id)
                return runas[i];
        }
        return null;
    }

    /// <summary>Legado: runas cujo <see cref="BestiarioRunaDef.criaturaAlvoId"/> coincide (lore). Para UI use <see cref="ObterRunasOrdenadas"/>.</summary>
    public BestiarioRunaDef[] ObterRunasDaCriatura(string criaturaId)
    {
        if (string.IsNullOrEmpty(criaturaId) || runas == null)
            return Array.Empty<BestiarioRunaDef>();
        var lista = new List<BestiarioRunaDef>();
        for (int i = 0; i < runas.Length; i++)
        {
            if (runas[i] != null && runas[i].criaturaAlvoId == criaturaId)
                lista.Add(runas[i]);
        }
        return lista.ToArray();
    }

    /// <summary>Todas as runas válidas ordenadas por nome (catálogo global).</summary>
    public BestiarioRunaDef[] ObterRunasOrdenadas()
    {
        if (runas == null || runas.Length == 0)
            return Array.Empty<BestiarioRunaDef>();
        var lista = new List<BestiarioRunaDef>();
        for (int i = 0; i < runas.Length; i++)
        {
            if (runas[i] != null && !string.IsNullOrEmpty(runas[i].id))
                lista.Add(runas[i]);
        }
        lista.Sort((a, b) => string.CompareOrdinal(a.nomeExibicao ?? "", b.nomeExibicao ?? ""));
        return lista.ToArray();
    }
}
