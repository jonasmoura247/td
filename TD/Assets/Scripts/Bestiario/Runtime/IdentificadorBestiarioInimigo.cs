using UnityEngine;

/// <summary>Define o id da entrada no BestiarioDatabase para este prefab inimigo.</summary>
public class IdentificadorBestiarioInimigo : MonoBehaviour
{
    [SerializeField] private string criaturaId = "";

    public string CriaturaId => criaturaId;

    public void DefinirId(string id) => criaturaId = id;
}
