using UnityEngine;

[CreateAssetMenu(fileName = "ReliquiaDef", menuName = "TD/Reliquias/Reliquia Def")]
public class ReliquiaDef : ScriptableObject
{
    public string idReliquia;
    public string nomeExibicao;
    [TextArea(2, 6)]
    public string descricao;
    public Sprite icone;
    public UnityEngine.Object prefabEfeitoPartida;
    public RaridadeReliquia raridade = RaridadeReliquia.Comum;
}
