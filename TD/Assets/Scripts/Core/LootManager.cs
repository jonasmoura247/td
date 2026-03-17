using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

[DefaultExecutionOrder(-100)]
public class LootManager : MonoBehaviour
{
    public static LootManager Instance;

    [Header("Database")]
    [SerializeField] private ItemDatabase itemDatabase;

    private List<ItemSlot> slots = new List<ItemSlot>();
    private string ultimoUuidCarregado;

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
            return;
        }

        SceneManager.sceneLoaded += OnCenaCarregada;
        CarregarInventarioDoSlotAtivo();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnCenaCarregada;
    }

    private void OnCenaCarregada(Scene cena, LoadSceneMode modo)
    {
        CarregarInventarioDoSlotAtivo();
    }

    /// <summary>Retorna o caminho do arquivo de inventário vinculado ao UUID do perfil ativo.</summary>
    private string ObterCaminhoSave()
    {
        string uuid = PerfilJogadorAtivo.Uuid;
        if (string.IsNullOrEmpty(uuid))
            return null;

        return Path.Combine(Application.persistentDataPath, $"inventario_{uuid}.json");
    }

    /// <summary>Carrega o inventário do perfil ativo. Chamado ao entrar em cena ou trocar de perfil.</summary>
    private void CarregarInventarioDoSlotAtivo()
    {
        string uuid = PerfilJogadorAtivo.Uuid;
        if (string.IsNullOrEmpty(uuid))
        {
            slots.Clear();
            ultimoUuidCarregado = null;
            return;
        }

        if (ultimoUuidCarregado == uuid)
            return;

        ultimoUuidCarregado = uuid;
        string caminho = ObterCaminhoSave();
        CarregarInventario(caminho);
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
        SalvarInventarioSePossivel();

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

        SalvarInventarioSePossivel();

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
        slots.Clear();
        SalvarInventarioSePossivel();
        if (UIInventario.Instance != null && UIInventario.Instance.PainelAberto())
            UIInventario.Instance.AtualizarInventario();
        Debug.Log("[LootManager] Inventario limpo. Arquivo salvo.");
    }

    private void SalvarInventarioSePossivel()
    {
        string caminho = ObterCaminhoSave();
        if (string.IsNullOrEmpty(caminho)) return;

        DadosInventario dados = new DadosInventario();
        dados.slots = slots;
        string json = JsonUtility.ToJson(dados, true);
        File.WriteAllText(caminho, json);
    }

    private void CarregarInventario(string caminho)
    {
        if (string.IsNullOrEmpty(caminho))
        {
            slots = new List<ItemSlot>();
            return;
        }

        if (!File.Exists(caminho))
        {
            slots = new List<ItemSlot>();
            MigrarInventarioLegadoSeExistir();
            return;
        }

        string json = File.ReadAllText(caminho);
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
                SalvarInventarioSePossivel();
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
            SalvarInventarioSePossivel();
        }
    }

    /// <summary>Migra inventario.json ou inventario_slot_N.json antigos para o perfil atual.</summary>
    private void MigrarInventarioLegadoSeExistir()
    {
        if (string.IsNullOrEmpty(PerfilJogadorAtivo.Uuid)) return;

        string basePath = Application.persistentDataPath;

        string caminhoLegado = Path.Combine(basePath, "inventario.json");
        if (File.Exists(caminhoLegado) && CarregarDeArquivoLegado(caminhoLegado))
        {
            SalvarInventarioSePossivel();
            try { File.Delete(caminhoLegado); } catch { }
            Debug.Log("[LootManager] Inventario legado migrado para o perfil atual.");
            return;
        }

        int slot = PerfilJogadorAtivo.IndiceSlot;
        string caminhoSlotLegado = Path.Combine(basePath, $"inventario_slot_{slot}.json");
        if (File.Exists(caminhoSlotLegado) && CarregarDeArquivoLegado(caminhoSlotLegado))
        {
            SalvarInventarioSePossivel();
            try { File.Delete(caminhoSlotLegado); } catch { }
            Debug.Log($"[LootManager] Inventario do slot {slot} migrado para UUID.");
        }
    }

    private bool CarregarDeArquivoLegado(string caminho)
    {
        try
        {
            string json = File.ReadAllText(caminho);
            var dados = JsonUtility.FromJson<DadosInventario>(json);
            if (dados?.slots != null && dados.slots.Count > 0)
            {
                slots = dados.slots;
                return true;
            }
            var legado = JsonUtility.FromJson<DadosInventarioLegado>(json);
            if (legado?.itens != null && legado.itens.Count > 0)
            {
                slots = new List<ItemSlot>();
                foreach (string nome in legado.itens)
                {
                    var existente = slots.Find(s => s.idItem == nome);
                    if (existente != null) existente.quantidade++;
                    else slots.Add(new ItemSlot(nome, 1));
                }
                return true;
            }
        }
        catch { }
        return false;
    }

    [System.Serializable]
    private class DadosInventarioLegado
    {
        public List<string> itens = new List<string>();
    }
}
