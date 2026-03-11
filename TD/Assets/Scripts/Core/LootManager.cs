using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LootManager : MonoBehaviour
{
    public static LootManager Instance;

    [Header("Database")]
    [SerializeField] private ItemDatabase itemDatabase;

    private List<string> itens = new List<string>();

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

        for (int i = 0; i < quantidade; i++)
            itens.Add(nomeItem);

        Debug.Log($"Item adicionado: {quantidade}x {nomeItem}");

        SalvarInventario();

        // Só atualizar UI se o painel estiver aberto (evita rebuild com hierarquia inativa)
        if (UIInventario.Instance != null && UIInventario.Instance.PainelAberto())
            UIInventario.Instance.AtualizarInventario();
    }

    public List<string> GetItens()
    {
        return itens;
    }

    public Sprite GetSpriteParaItem(string nomeItem)
    {
        if (itemDatabase == null) return null;
        return itemDatabase.GetSprite(nomeItem);
    }

    void SalvarInventario()
    {
        DadosInventario dados = new DadosInventario();
        dados.itens = itens;

        string json = JsonUtility.ToJson(dados, true);

        File.WriteAllText(caminhoSave, json);
    }

    void CarregarInventario()
    {
        if (!File.Exists(caminhoSave))
            return;

        string json = File.ReadAllText(caminhoSave);

        DadosInventario dados = JsonUtility.FromJson<DadosInventario>(json);

        if (dados?.itens == null)
        {
            itens = new List<string>();
            return;
        }

        itens = dados.itens;

        const int maxItens = 200;
        if (itens.Count > maxItens)
        {
            Debug.LogWarning($"[LootManager] Inventario com {itens.Count} itens truncado para {maxItens}. Delete o arquivo de save se o problema persistir.");
            itens = itens.GetRange(0, maxItens);
            SalvarInventario();
        }
    }
}
