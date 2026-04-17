using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInventario : MonoBehaviour
{
    public static UIInventario Instance;

    public GameObject painelInventario;
    public Transform gradeItens;
    public GameObject prefabSlot;

    [Header("Responsividade do Grid")]
    [Tooltip("Tamanho do slot na resolucao de referencia 1920x1080 (px).")]
    [SerializeField] private float tamanhoSlotReferencia = 80f;
    [Tooltip("Espacamento entre slots na resolucao de referencia (px).")]
    [SerializeField] private float spacingReferencia = 8f;
    [Tooltip("Resolucao de referencia para calcular escala.")]
    [SerializeField] private Vector2 resolucaoReferencia = new Vector2(1920f, 1080f);

    private GridLayoutGroup _gridLayout;

    void Awake()
    {
        Instance = this;
        _gridLayout = gradeItens != null ? gradeItens.GetComponent<GridLayoutGroup>() : null;
    }

    void Start()
    {
        AjustarGridResponsivo();
        UIResponsividadeManager.OnLayoutAtualizado += AjustarGridResponsivo;

        if (LootManager.Instance != null)
            AtualizarInventario();
    }

    private void OnDestroy()
    {
        UIResponsividadeManager.OnLayoutAtualizado -= AjustarGridResponsivo;
    }

    /// <summary>
    /// Recalcula o tamanho dos cells e o spacing do GridLayoutGroup
    /// proporcional a resolucao atual.
    /// </summary>
    private void AjustarGridResponsivo()
    {
        if (_gridLayout == null) return;

        float fatorH   = Screen.height / resolucaoReferencia.y;
        float fatorW   = Screen.width  / resolucaoReferencia.x;
        float fator    = Mathf.Min(fatorW, fatorH);

        float tamanhoSlot = Mathf.Clamp(tamanhoSlotReferencia * fator, 40f, 160f);
        float spacing     = Mathf.Clamp(spacingReferencia     * fator,  4f,  20f);

        _gridLayout.cellSize = new Vector2(tamanhoSlot, tamanhoSlot);
        _gridLayout.spacing  = new Vector2(spacing, spacing);
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
