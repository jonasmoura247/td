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
        // Sempre atualizar ao abrir - evita itens "sumindo" por painel inativo no Start
        if (vaiAbrir)
            AtualizarInventario();
    }

    public void AtualizarInventario()
    {
        if (gradeItens == null || prefabSlot == null || LootManager.Instance == null) return;

        // Se o painel está fechado, não recriar slots (evita "piscar" e bugs de layout inativo)
        if (painelInventario != null && !painelInventario.activeSelf)
            return;

        // Destroy imediato para evitar frame com slots duplicados (Destroy é deferred)
        for (int i = gradeItens.childCount - 1; i >= 0; i--)
            DestroyImmediate(gradeItens.GetChild(i).gameObject);

        List<string> itens = LootManager.Instance.GetItens();
        if (itens == null || itens.Count == 0)
            return;

        foreach (string nomeItem in itens)
        {
            Sprite sprite = LootManager.Instance.GetSpriteParaItem(nomeItem);
            GameObject slot = Instantiate(prefabSlot, gradeItens);
            SlotItemUI slotUI = slot.GetComponent<SlotItemUI>();
            if (slotUI != null)
                slotUI.Configurar(sprite);
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