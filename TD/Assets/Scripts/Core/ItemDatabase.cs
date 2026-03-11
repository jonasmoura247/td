using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventario/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [System.Serializable]
    public class ItemDef
    {
        public string nome;
        public Sprite sprite;
    }

    [SerializeField] private List<ItemDef> itens = new List<ItemDef>();

    public Sprite GetSprite(string nome)
    {
        if (string.IsNullOrEmpty(nome) || itens == null) return null;

        foreach (var item in itens)
        {
            if (item != null && item.nome == nome)
                return item.sprite;
        }
        return null;
    }
}
