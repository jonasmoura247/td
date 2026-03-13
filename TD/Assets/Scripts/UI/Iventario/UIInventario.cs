using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInventario : MonoBehaviour
{
    public static UIInventario Instance;

    public GameObject painelInventario;
    public Transform gradeItens;
    public GameObject prefabSlot;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (LootManager.Instance != null)
            AtualizarInventario();
    }

    public bool PainelAberto()
    {
        return painelInventario != null && painelInventario.activeSelf;
    }

    public void AlternarInventario()
    {
        bool vaiAbrir = !painelInventario.activeSelf;
        painelInventario.SetActive(vaiAbrir);
        if (vaiAbrir)
            AtualizarInventario();
    }

    public void AtualizarInventario()
    {
        if (gradeItens == null || prefabSlot == null || LootManager.Instance == null) return;

        if (painelInventario != null && !painelInventario.activeSelf)
            return;

        for (int i = gradeItens.childCount - 1; i >= 0; i--)
            DestroyImmediate(gradeItens.GetChild(i).gameObject);

        List<ItemSlot> slotsLista = LootManager.Instance.GetSlots();
        if (slotsLista == null || slotsLista.Count == 0)
            return;

        foreach (ItemSlot slot in slotsLista)
        {
            Sprite sprite = LootManager.Instance.GetSpriteParaItem(slot.idItem);
            GameObject slotObj = Instantiate(prefabSlot, gradeItens);
            SlotItemUI slotUI = slotObj.GetComponent<SlotItemUI>();
            if (slotUI != null)
                slotUI.Configurar(sprite, slot.idItem, slot.quantidade);
        }

        if (gradeItens is RectTransform rt)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    public void AdicionarSlot(Sprite spriteItem)
    {
        GameObject slot = Instantiate(prefabSlot, gradeItens);
        SlotItemUI slotUI = slot.GetComponent<SlotItemUI>();
        if (slotUI != null)
            slotUI.Configurar(spriteItem);
    }
}
