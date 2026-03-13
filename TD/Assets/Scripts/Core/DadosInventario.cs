using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemSlot
{
    public string idItem;
    public int quantidade;

    public ItemSlot(string id, int qtd = 1)
    {
        idItem = id;
        quantidade = Mathf.Max(1, qtd);
    }
}

[Serializable]
public class DadosInventario
{
    public List<ItemSlot> slots = new List<ItemSlot>();
}
