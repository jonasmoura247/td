using System.Collections.Generic;
using UnityEngine;
using System.IO;

[DefaultExecutionOrder(-100)]
public class LootManager : MonoBehaviour
{
    public static LootManager Instance;

    [Header("Database")]
    [SerializeField] private ItemDatabase itemDatabase;

    private List<ItemSlot> slots = new List<ItemSlot>();
    private string caminhoSave;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        caminhoSave = Application.persistentDataPath + "/inventario.json";
        CarregarInventario();
    }

    public void AdicionarItem(string nomeItem, Sprite spriteItem, int quantidade = 1)
    {
        quantidade = Mathf.Max(1, quantidade);

        ItemSlot existente = slots.Find(s => s.idItem == nomeItem);
        if (existente != null)
            existente.quantidade += quantidade;
        else
            slots.Add(new ItemSlot(nomeItem, quantidade));

        Debug.Log($"Item adicionado: {quantidade}x {nomeItem}");
        SalvarInventario();

        if (UIInventario.Instance != null && UIInventario.Instance.PainelAberto())
            UIInventario.Instance.AtualizarInventario();
    }

    public void RemoverItem(string idItem, int quantidade = 1)
    {
        ItemSlot slot = slots.Find(s => s.idItem == idItem);
        if (slot == null) return;

        quantidade = Mathf.Clamp(quantidade, 1, slot.quantidade);
        slot.quantidade -= quantidade;

        if (slot.quantidade <= 0)
            slots.Remove(slot);

        SalvarInventario();

        if (UIInventario.Instance != null && UIInventario.Instance.PainelAberto())
            UIInventario.Instance.AtualizarInventario();
    }

    public List<ItemSlot> GetSlots()
    {
        return slots;
    }

    public Sprite GetSpriteParaItem(string nomeItem)
    {
        if (itemDatabase == null) return null;
        return itemDatabase.GetSprite(nomeItem);
    }

    /// <summary>
    /// Limpa todo o inventário e salva. Use para testar do zero.
    /// Funciona no Editor (Context Menu) ou em runtime.
    /// </summary>
    [ContextMenu("Limpar Inventario (Testar do Zero)")]
    public void LimparInventario()
    {
        if (string.IsNullOrEmpty(caminhoSave))
            caminhoSave = Application.persistentDataPath + "/inventario.json";
        slots.Clear();
        SalvarInventario();
        if (UIInventario.Instance != null && UIInventario.Instance.PainelAberto())
            UIInventario.Instance.AtualizarInventario();
        Debug.Log("[LootManager] Inventario limpo. Arquivo salvo.");
    }

    void SalvarInventario()
    {
        DadosInventario dados = new DadosInventario();
        dados.slots = slots;
        string json = JsonUtility.ToJson(dados, true);
        File.WriteAllText(caminhoSave, json);
    }

    void CarregarInventario()
    {
        if (!File.Exists(caminhoSave))
        {
            slots = new List<ItemSlot>();
            return;
        }

        string json = File.ReadAllText(caminhoSave);
        DadosInventario dados = JsonUtility.FromJson<DadosInventario>(json);

        if (dados?.slots != null && dados.slots.Count > 0)
        {
            slots = dados.slots;
        }
        else
        {
            // Migração: formato antigo tinha List<string> itens
            DadosInventarioLegado dadosLegado = JsonUtility.FromJson<DadosInventarioLegado>(json);
            if (dadosLegado?.itens != null && dadosLegado.itens.Count > 0)
            {
                slots = new List<ItemSlot>();
                foreach (string nome in dadosLegado.itens)
                {
                    ItemSlot existente = slots.Find(s => s.idItem == nome);
                    if (existente != null)
                        existente.quantidade++;
                    else
                        slots.Add(new ItemSlot(nome, 1));
                }
                SalvarInventario();
                Debug.Log("[LootManager] Inventario migrado do formato antigo.");
                return;
            }
            slots = new List<ItemSlot>();
        }

        const int maxItens = 200;
        int total = 0;
        foreach (var s in slots) total += s.quantidade;
        if (total > maxItens)
        {
            Debug.LogWarning($"[LootManager] Inventario com {total} itens truncado para {maxItens}.");
            int sobra = total - maxItens;
            foreach (var slot in slots)
            {
                if (sobra <= 0) break;
                int remover = Mathf.Min(sobra, slot.quantidade);
                slot.quantidade -= remover;
                sobra -= remover;
            }
            slots.RemoveAll(s => s.quantidade <= 0);
            SalvarInventario();
        }
    }

    [System.Serializable]
    private class DadosInventarioLegado
    {
        public List<string> itens = new List<string>();
    }
}
