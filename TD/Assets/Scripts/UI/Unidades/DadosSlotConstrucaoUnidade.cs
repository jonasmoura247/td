using UnityEngine;

/// <summary>
/// Definição de slot de unidade de combate (HUD): prefab, custo e raio do preview de alcance.
/// </summary>
[System.Serializable]
public class DadosSlotConstrucaoUnidade
{
    [Tooltip("Prefab da unidade (arqueiro, feiticeiro, etc.).")]
    public GameObject prefabUnidade;

    [Tooltip("Cobrado ao confirmar a colocação no mapa.")]
    public int custoOuro = 100;

    [Tooltip("Nome da instância na hierarquia (opcional).")]
    public string nomeInstancia = "";

    [Tooltip("Raio do círculo de alcance no preview (unidades de mundo).")]
    public float alcanceIndicadorPreview = 5f;
}
