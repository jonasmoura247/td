using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Agrupa os campos de UI de estatisticas de uma unica raca.
/// Referenciado como array em PainelEstatisticas para as 4 racas.
/// </summary>
[Serializable]
public class UIItemEstatisticasRaca
{
    public TipoRaca raca;

    [Header("Labels")]
    public TextMeshProUGUI textoPartidas;
    public TextMeshProUGUI textoVitorias;
    public TextMeshProUGUI textoDerrotas;
    public TextMeshProUGUI textoWinRate;
    public TextMeshProUGUI textoKills;
    public TextMeshProUGUI textoOuro;
}
