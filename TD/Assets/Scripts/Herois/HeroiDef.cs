using UnityEngine;

/// <summary>
/// Definição de um herói: metadados e prefab para spawn.
/// Criado como ScriptableObject no editor.
/// </summary>
[CreateAssetMenu(fileName = "HeroiDef", menuName = "TD/Herois/Heroi Def")]
public class HeroiDef : ScriptableObject
{
    public string idHeroi;
    public string nomeExibicao;
    [TextArea(2, 4)]
    public string descricao;
    public Sprite icone;
    public HeroiRaridade raridade = HeroiRaridade.Azul;
    public GameObject prefab;
}
