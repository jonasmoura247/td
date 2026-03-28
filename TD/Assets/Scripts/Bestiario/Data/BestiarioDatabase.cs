using System;
using UnityEngine;

[Serializable]
public class BestiarioCriaturaDef
{
    public string id;
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
    /// <summary>Criatura que pode receber esta runa.</summary>
    public string criaturaAlvoId;
    public int custoPontos = 1;
    /// <summary>Estágios completos necessários (ex.: 3 = ter passado o 3º marco).</summary>
    public int estagiosMinimos = 3;
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

    public BestiarioRunaDef[] ObterRunasDaCriatura(string criaturaId)
    {
        if (string.IsNullOrEmpty(criaturaId) || runas == null)
            return Array.Empty<BestiarioRunaDef>();
        var lista = new System.Collections.Generic.List<BestiarioRunaDef>();
        for (int i = 0; i < runas.Length; i++)
        {
            if (runas[i] != null && runas[i].criaturaAlvoId == criaturaId)
                lista.Add(runas[i]);
        }
        return lista.ToArray();
    }
}
