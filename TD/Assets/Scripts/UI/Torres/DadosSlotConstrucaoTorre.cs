using UnityEngine;

/// <summary>
/// Definição de um slot da HUD de construção (prefab + dados). Extensível para custo, bloqueio, etc.
/// </summary>
[System.Serializable]
public class DadosSlotConstrucaoTorre
{
    [Tooltip("Prefab da torre a instanciar (ex.: TorreGoblinLanceira).")]
    public GameObject prefabTorre;

    [Tooltip("Stats exibidos no preview de alcance e futuros custos.")]
    public TorreDadosSO dados;
}
