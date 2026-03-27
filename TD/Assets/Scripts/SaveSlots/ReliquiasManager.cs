using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Relíquias por save: possuídas e até 5 slots de equipamento. Persistência via DadosSlotSave.
/// </summary>
public class ReliquiasManager : MonoBehaviour
{
    public static ReliquiasManager Instance { get; private set; }

    private const int MaxSlotsEquipamento = 5;

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
        }
    }

    public static bool PodeEditarEquipamento()
    {
        return !EstadoPartidaReliquias.EmPartida;
    }

    public IReadOnlyList<string> GetReliquiasPossuidas()
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo)
            return new List<string>();
        var p = PerfilJogadorAtivo.Dados?.reliquiasPossuidas;
        return p ?? new List<string>();
    }

    /// <summary>Ids equipados em ordem dos slots 0–4 (ignora strings vazias).</summary>
    public List<string> GetReliquiasEquipadasParaEfeitos()
    {
        var lista = new List<string>();
        if (!PerfilJogadorAtivo.TemPerfilAtivo)
            return lista;

        var slots = PerfilJogadorAtivo.Dados?.reliquiasPorSlot;
        if (slots == null)
            return lista;

        for (int i = 0; i < slots.Length && i < MaxSlotsEquipamento; i++)
        {
            if (!string.IsNullOrEmpty(slots[i]))
                lista.Add(slots[i]);
        }

        return lista;
    }

    /// <summary>Compatível com código que ainda chama GetReliquiasEquipadas.</summary>
    public IReadOnlyList<string> GetReliquiasEquipadas()
    {
        return GetReliquiasEquipadasParaEfeitos();
    }

    public string GetReliquiaNoSlot(int indiceSlot)
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || PerfilJogadorAtivo.Dados == null ||
            indiceSlot < 0 || indiceSlot >= MaxSlotsEquipamento)
            return null;
        var slots = PerfilJogadorAtivo.Dados.reliquiasPorSlot;
        if (slots == null || indiceSlot >= slots.Length)
            return null;
        return string.IsNullOrEmpty(slots[indiceSlot]) ? null : slots[indiceSlot];
    }

    public bool PossuiReliquia(string idReliquia)
    {
        if (string.IsNullOrEmpty(idReliquia) || !PerfilJogadorAtivo.TemPerfilAtivo)
            return false;
        return PerfilJogadorAtivo.Dados.reliquiasPossuidas != null &&
               PerfilJogadorAtivo.Dados.reliquiasPossuidas.Contains(idReliquia);
    }

    /// <summary>Para drops futuros (boss). Salva no slot ativo.</summary>
    public bool AdicionarReliquiaPossuida(string idReliquia)
    {
        if (string.IsNullOrEmpty(idReliquia) || !PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return false;
        var possui = PerfilJogadorAtivo.Dados.reliquiasPossuidas;
        if (possui == null)
        {
            possui = new List<string>();
            PerfilJogadorAtivo.Dados.reliquiasPossuidas = possui;
        }
        if (possui.Contains(idReliquia))
            return false;
        possui.Add(idReliquia);
        SaveSlotManager.Instance.AtualizarSlotAtivo(_ => { });
        return true;
    }

    public bool EquiparNoSlot(int indiceSlot, string idReliquia)
    {
        if (!PodeEditarEquipamento())
            return false;
        if (!PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return false;
        if (indiceSlot < 0 || indiceSlot >= MaxSlotsEquipamento)
            return false;
        if (string.IsNullOrEmpty(idReliquia) || !PossuiReliquia(idReliquia))
            return false;

        GarantirArrays();

        for (int s = 0; s < MaxSlotsEquipamento; s++)
        {
            if (PerfilJogadorAtivo.Dados.reliquiasPorSlot[s] == idReliquia)
                PerfilJogadorAtivo.Dados.reliquiasPorSlot[s] = "";
        }

        PerfilJogadorAtivo.Dados.reliquiasPorSlot[indiceSlot] = idReliquia;
        SaveSlotManager.Instance.AtualizarSlotAtivo(_ => { });
        return true;
    }

    public bool DesequiparSlot(int indiceSlot)
    {
        if (!PodeEditarEquipamento())
            return false;
        if (!PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return false;
        if (indiceSlot < 0 || indiceSlot >= MaxSlotsEquipamento)
            return false;

        GarantirArrays();
        PerfilJogadorAtivo.Dados.reliquiasPorSlot[indiceSlot] = "";
        SaveSlotManager.Instance.AtualizarSlotAtivo(_ => { });
        return true;
    }

    private bool ReliquiaEstaEquipadaEmAlgumSlot(string idReliquia)
    {
        if (string.IsNullOrEmpty(idReliquia))
            return false;
        for (int s = 0; s < MaxSlotsEquipamento; s++)
        {
            if (GetReliquiaNoSlot(s) == idReliquia)
                return true;
        }
        return false;
    }

    public int ContarReliquiasNaoEquipadas()
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || PerfilJogadorAtivo.Dados?.reliquiasPossuidas == null)
            return 0;
        var list = PerfilJogadorAtivo.Dados.reliquiasPossuidas;
        int n = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (!ReliquiaEstaEquipadaEmAlgumSlot(list[i]))
                n++;
        }
        return n;
    }

    public bool MoverReliquiaPossuidaParaPosicaoVisual(string idReliquia, int indiceSlotVisualDestino)
    {
        if (!PodeEditarEquipamento() || !PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return false;
        var list = PerfilJogadorAtivo.Dados.reliquiasPossuidas;
        if (list == null || string.IsNullOrEmpty(idReliquia) || !list.Contains(idReliquia))
            return false;

        list.Remove(idReliquia);

        int nNaoEq = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (!ReliquiaEstaEquipadaEmAlgumSlot(list[i]))
                nNaoEq++;
        }

        int pos = Mathf.Clamp(indiceSlotVisualDestino, 0, nNaoEq);

        int seen = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (!ReliquiaEstaEquipadaEmAlgumSlot(list[i]))
            {
                if (seen == pos)
                {
                    list.Insert(i, idReliquia);
                    SaveSlotManager.Instance.AtualizarSlotAtivo(_ => { });
                    return true;
                }
                seen++;
            }
        }

        list.Add(idReliquia);
        SaveSlotManager.Instance.AtualizarSlotAtivo(_ => { });
        return true;
    }

    public bool DesequiparParaSlotDisponivel(int indiceSlotEquipamento, int indiceSlotVisualPainel)
    {
        if (!PodeEditarEquipamento())
            return false;
        string id = GetReliquiaNoSlot(indiceSlotEquipamento);
        if (string.IsNullOrEmpty(id))
            return false;
        if (!DesequiparSlot(indiceSlotEquipamento))
            return false;
        int n = ContarReliquiasNaoEquipadas();
        int pos = Mathf.Clamp(indiceSlotVisualPainel, 0, Mathf.Max(0, n - 1));
        return MoverReliquiaPossuidaParaPosicaoVisual(id, pos);
    }

    private void GarantirArrays()
    {
        if (PerfilJogadorAtivo.Dados.reliquiasPorSlot == null ||
            PerfilJogadorAtivo.Dados.reliquiasPorSlot.Length != MaxSlotsEquipamento)
            PerfilJogadorAtivo.Dados.reliquiasPorSlot = new string[MaxSlotsEquipamento] { "", "", "", "", "" };
    }
}
