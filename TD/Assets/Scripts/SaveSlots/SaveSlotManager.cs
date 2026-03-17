using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gerencia os 4 slots de save. Cada slot = um perfil de jogo completo.
/// Persistência em JSON, carregamento de perfil ativo para o jogo.
/// </summary>
public class SaveSlotManager : MonoBehaviour
{
    public static SaveSlotManager Instance { get; private set; }

    private const int QUANTIDADE_SLOTS = 4;
    private const string NOME_ARQUIVO = "slots_save.json";

    [Header("Lobby por raça (cena carregada conforme raça do perfil)")]
    [SerializeField] private string lobbyGoblin = "LobbyGoblin";
    [SerializeField] private string lobbyElfo = "LobbyElfo";
    [SerializeField] private string lobbyAnao = "LobbyAnoes";
    [SerializeField] private string lobbyHumano = "LobbyHumano";

    public event Action<List<DadosSlotSave>> OnSlotsCarregados;

    private DadosSlotSave[] slots;
    private int slotAtivo = -1;
    private string caminhoSave;

    private void Awake()
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

        caminhoSave = Path.Combine(Application.persistentDataPath, NOME_ARQUIVO);
        slots = new DadosSlotSave[QUANTIDADE_SLOTS];

        for (int i = 0; i < QUANTIDADE_SLOTS; i++)
        {
            if (slots[i] == null)
                slots[i] = new DadosSlotSave { indiceSlot = i };
        }

        CarregarSlots();
    }

    /// <summary>Retorna todos os slots (0 a 3).</summary>
    public IReadOnlyList<DadosSlotSave> GetSlots()
    {
        return slots;
    }

    /// <summary>Retorna o slot no índice. Índice deve ser 0 a 3.</summary>
    public DadosSlotSave GetSlot(int indice)
    {
        if (indice < 0 || indice >= QUANTIDADE_SLOTS)
            return null;
        return slots[indice];
    }

    /// <summary>Slot vazio = pode criar novo perfil.</summary>
    public bool SlotVazio(int indice)
    {
        var s = GetSlot(indice);
        return s == null || !s.ocupado;
    }

    /// <summary>Cria um novo perfil no slot e inicia o jogo.</summary>
    public void CriarPerfilEIniciar(int indiceSlot, TipoRaca raca, GeneroChefe genero)
    {
        if (indiceSlot < 0 || indiceSlot >= QUANTIDADE_SLOTS)
        {
            Debug.LogError($"[SaveSlotManager] Indice invalido: {indiceSlot}");
            return;
        }

        slots[indiceSlot] = new DadosSlotSave(indiceSlot, raca, genero);
        slotAtivo = indiceSlot;
        SalvarSlots();

        // Notifica outros sistemas sobre o perfil ativo
        PerfilJogadorAtivo.DefinirPerfil(slots[indiceSlot]);

        // Carrega o lobby da raça escolhida
        Time.timeScale = 1f;
        SceneManager.LoadScene(ObterCenaLobby(raca));
    }

    /// <summary>Carrega um perfil existente e inicia/continua o jogo.</summary>
    public void CarregarPerfilEIniciar(int indiceSlot)
    {
        var dados = GetSlot(indiceSlot);
        if (dados == null || !dados.ocupado)
        {
            Debug.LogWarning($"[SaveSlotManager] Slot {indiceSlot} esta vazio. Use CriarPerfilEIniciar para novo jogo.");
            return;
        }

        slotAtivo = indiceSlot;
        dados.dataUltimoJogo = DateTime.Now.ToString("O");
        SalvarSlots();

        PerfilJogadorAtivo.DefinirPerfil(dados);

        Time.timeScale = 1f;
        SceneManager.LoadScene(ObterCenaLobby(dados.raca));
    }

    /// <summary>Índice do slot atualmente em jogo (-1 se nenhum).</summary>
    public int GetSlotAtivo() => slotAtivo;

    /// <summary>Apaga todos os slots e remove arquivos associados a cada UUID.</summary>
    [ContextMenu("Limpar todos os slots")]
    public void LimparTodosOsSlots()
    {
        for (int i = 0; i < QUANTIDADE_SLOTS; i++)
        {
            if (slots[i]?.ocupado == true && !string.IsNullOrEmpty(slots[i].uuid))
                RemoverArquivosDoPerfil(slots[i].uuid);
            slots[i] = new DadosSlotSave { indiceSlot = i };
        }

        slotAtivo = -1;
        SalvarSlots();

        OnSlotsCarregados?.Invoke(new List<DadosSlotSave>(slots));
        Debug.Log("[SaveSlotManager] Todos os slots foram limpos.");
    }

    /// <summary>Apaga o conteúdo do slot e remove arquivos vinculados ao UUID (inventário, etc.).</summary>
    public void ApagarSlot(int indice)
    {
        if (indice < 0 || indice >= QUANTIDADE_SLOTS) return;

        var dados = slots[indice];
        if (dados?.ocupado == true && !string.IsNullOrEmpty(dados.uuid))
            RemoverArquivosDoPerfil(dados.uuid);

        slots[indice] = new DadosSlotSave { indiceSlot = indice };
        if (slotAtivo == indice)
            slotAtivo = -1;

        SalvarSlots();
        OnSlotsCarregados?.Invoke(new List<DadosSlotSave>(slots));
    }

    /// <summary>Remove todos os arquivos vinculados ao UUID do perfil (inventário, etc.).</summary>
    private void RemoverArquivosDoPerfil(string uuid)
    {
        if (string.IsNullOrEmpty(uuid)) return;

        string pathInventario = Path.Combine(Application.persistentDataPath, $"inventario_{uuid}.json");
        if (File.Exists(pathInventario))
        {
            try { File.Delete(pathInventario); } catch { }
        }
    }

    /// <summary>Atualiza dados do slot ativo (ex: tempo de jogo, progresso).</summary>
    public void AtualizarSlotAtivo(Action<DadosSlotSave> alterar)
    {
        if (slotAtivo < 0 || slotAtivo >= QUANTIDADE_SLOTS) return;
        alterar?.Invoke(slots[slotAtivo]);
        SalvarSlots();
    }

    private string ObterCenaLobby(TipoRaca raca)
    {
        return raca switch
        {
            TipoRaca.Goblin => lobbyGoblin,
            TipoRaca.Elfo => lobbyElfo,
            TipoRaca.Anao => lobbyAnao,
            TipoRaca.Humano => lobbyHumano,
            _ => lobbyGoblin
        };
    }

    private void SalvarSlots()
    {
        try
        {
            var wrapper = new WrapperSlots { slots = new List<DadosSlotSave>(slots) };
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(caminhoSave, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSlotManager] Erro ao salvar: {e.Message}");
        }
    }

    private void CarregarSlots()
    {
        if (!File.Exists(caminhoSave))
        {
            OnSlotsCarregados?.Invoke(new List<DadosSlotSave>(slots));
            return;
        }

        try
        {
            string json = File.ReadAllText(caminhoSave);
            var wrapper = JsonUtility.FromJson<WrapperSlots>(json);
            if (wrapper?.slots != null && wrapper.slots.Count > 0)
            {
                for (int i = 0; i < Mathf.Min(QUANTIDADE_SLOTS, wrapper.slots.Count); i++)
                {
                    slots[i] = wrapper.slots[i];
                    slots[i].indiceSlot = i;
                    slots[i].GarantirUuid();
                    if (slots[i].reliquiasEquipadas == null) slots[i].reliquiasEquipadas = new List<string>();
                    if (slots[i].heroisContratados == null) slots[i].heroisContratados = new List<string>();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveSlotManager] Erro ao carregar: {e.Message}");
        }

        OnSlotsCarregados?.Invoke(new List<DadosSlotSave>(slots));
    }

    [Serializable]
    private class WrapperSlots
    {
        public List<DadosSlotSave> slots = new List<DadosSlotSave>();
    }
}
