using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SalaoReliquiaGoblinUI : MonoBehaviour
{
    private const int NumSlotsDisponiveisPadrao = 12;

    [SerializeField] private ReliquiaDatabase database;
    [SerializeField] private GameObject prefabIconeReliquia;
    [SerializeField] private Transform parentIconesPossuidas;
    [SerializeField] private RectTransform[] slotsReliquiasDisponiveis;
    [SerializeField] private Image[] imagensSlots = new Image[5];

    private readonly List<GameObject> instanciasPool = new List<GameObject>();
    private RectTransform[] slotsDisponiveisRuntime;

    private void Awake()
    {
        SaveSlotManager.GarantirExistencia();

        GarantirSlotsDisponiveis();
        GarantirDropEmSlots();
    }

    private RectTransform[] ObterSlotsDisponiveis()
    {
        if (slotsReliquiasDisponiveis != null && slotsReliquiasDisponiveis.Length > 0)
            return slotsReliquiasDisponiveis;
        return slotsDisponiveisRuntime;
    }

    private void GarantirSlotsDisponiveis()
    {
        if (slotsReliquiasDisponiveis != null && slotsReliquiasDisponiveis.Length > 0)
        {
            for (int i = 0; i < slotsReliquiasDisponiveis.Length; i++)
            {
                if (slotsReliquiasDisponiveis[i] == null)
                    continue;
                var s = slotsReliquiasDisponiveis[i].GetComponent<ReliquiaSlotDisponivelSalao>();
                if (s == null)
                    s = slotsReliquiasDisponiveis[i].gameObject.AddComponent<ReliquiaSlotDisponivelSalao>();
                s.DefinirIndice(i);
            }
            return;
        }

        if (parentIconesPossuidas == null)
            return;

        var parent = parentIconesPossuidas as RectTransform;
        if (parent == null)
            return;

        var hlg = parent.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null)
        {
            hlg = parent.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
        }

        slotsDisponiveisRuntime = new RectTransform[NumSlotsDisponiveisPadrao];
        for (int i = 0; i < NumSlotsDisponiveisPadrao; i++)
        {
            var go = new GameObject("SlotReliquiaDisp_" + i, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.02f);
            img.raycastTarget = true;
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 56f;
            le.preferredHeight = 56f;
            le.minWidth = 56f;
            le.minHeight = 56f;
            var slot = go.AddComponent<ReliquiaSlotDisponivelSalao>();
            slot.DefinirIndice(i);
            slotsDisponiveisRuntime[i] = rt;
        }
    }

    private void GarantirDropEmSlots()
    {
        for (int i = 0; i < imagensSlots.Length; i++)
        {
            if (imagensSlots[i] == null)
                continue;
            var drop = imagensSlots[i].GetComponent<ReliquiaSlotDropSalao>();
            if (drop == null)
                drop = imagensSlots[i].gameObject.AddComponent<ReliquiaSlotDropSalao>();
            drop.DefinirIndice(i);
        }
    }

    private void OnEnable()
    {
        AtualizarTudo();
    }

    public void AtualizarTudo()
    {
        LimparPool();

        if (ReliquiasManager.Instance == null || database == null || prefabIconeReliquia == null || parentIconesPossuidas == null)
            return;

        if (!PerfilJogadorAtivo.TemPerfilAtivo)
            return;

        var slotsDisp = ObterSlotsDisponiveis();
        if (slotsDisp == null || slotsDisp.Length == 0)
            return;

        var possuidas = ReliquiasManager.Instance.GetReliquiasPossuidas();
        int slotVisual = 0;
        for (int p = 0; p < possuidas.Count && slotVisual < slotsDisp.Length; p++)
        {
            string id = possuidas[p];
            if (string.IsNullOrEmpty(id) || ReliquiaEstaEquipada(id))
                continue;

            ReliquiaDef def = database.ObterPorId(id);
            if (def == null)
                continue;

            if (slotsDisp[slotVisual] == null)
                break;

            GameObject go = Instantiate(prefabIconeReliquia, slotsDisp[slotVisual]);
            instanciasPool.Add(go);

            var rtIcon = go.transform as RectTransform;
            if (rtIcon != null)
            {
                rtIcon.anchorMin = Vector2.zero;
                rtIcon.anchorMax = Vector2.one;
                rtIcon.offsetMin = Vector2.zero;
                rtIcon.offsetMax = Vector2.zero;
                rtIcon.localScale = Vector3.one;
            }

            var img = go.GetComponentInChildren<Image>(true);
            if (img != null && def.icone != null)
            {
                img.sprite = def.icone;
                img.enabled = true;
                img.raycastTarget = true;
            }

            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = go.AddComponent<CanvasGroup>();

            var drag = go.GetComponent<ReliquiaDragSalao>();
            if (drag == null)
                drag = go.AddComponent<ReliquiaDragSalao>();
            drag.Configurar(this, id, false, -1, slotVisual);
            slotVisual++;
        }

        for (int s = 0; s < imagensSlots.Length && s < 5; s++)
        {
            if (imagensSlots[s] == null)
                continue;
            string idEq = ReliquiasManager.Instance.GetReliquiaNoSlot(s);
            var goSlot = imagensSlots[s].gameObject;
            var dragExistente = goSlot.GetComponent<ReliquiaDragSalao>();
            if (string.IsNullOrEmpty(idEq))
            {
                imagensSlots[s].enabled = false;
                imagensSlots[s].sprite = null;
                if (dragExistente != null)
                    Destroy(dragExistente);
                continue;
            }

            ReliquiaDef def = database.ObterPorId(idEq);
            if (def != null && def.icone != null)
            {
                imagensSlots[s].sprite = def.icone;
                imagensSlots[s].enabled = true;
            }

            if (goSlot.GetComponent<CanvasGroup>() == null)
                goSlot.AddComponent<CanvasGroup>();
            var dragSlot = goSlot.GetComponent<ReliquiaDragSalao>();
            if (dragSlot == null)
                dragSlot = goSlot.AddComponent<ReliquiaDragSalao>();
            dragSlot.Configurar(this, idEq, true, s, -1);
        }
    }

    private bool ReliquiaEstaEquipada(string idReliquia)
    {
        for (int s = 0; s < 5; s++)
        {
            string x = ReliquiasManager.Instance.GetReliquiaNoSlot(s);
            if (x == idReliquia)
                return true;
        }
        return false;
    }

    private void LimparPool()
    {
        for (int i = 0; i < instanciasPool.Count; i++)
        {
            if (instanciasPool[i] != null)
                Destroy(instanciasPool[i]);
        }
        instanciasPool.Clear();
    }

    public void TentarEquiparEmSlot(int indiceSlot, string idReliquia)
    {
        if (ReliquiasManager.Instance == null)
        {
            Debug.LogWarning("[SalaoReliquiaGoblinUI] ReliquiasManager nao encontrado.");
            return;
        }

        if (ReliquiasManager.Instance.EquiparNoSlot(indiceSlot, idReliquia))
            Debug.Log("[SalaoReliquiaGoblinUI] Equipado slot=" + indiceSlot + " id=" + idReliquia);
        else
        {
            Debug.LogWarning(
                "[SalaoReliquiaGoblinUI] Equipar falhou. " +
                "slot=" + indiceSlot + " id=" + idReliquia +
                " | emPartida=" + EstadoPartidaReliquias.EmPartida +
                " | perfilAtivo=" + PerfilJogadorAtivo.TemPerfilAtivo +
                " | possuiReliquia=" + ReliquiasManager.Instance.PossuiReliquia(idReliquia));
        }
    }

    public void TentarDesequiparSlot(int indiceSlot)
    {
        ReliquiasManager.Instance?.DesequiparSlot(indiceSlot);
    }

    public void TentarDesequiparParaSlotDisponivel(int indiceSlotEquip, int indiceSlotVisualPainel)
    {
        ReliquiasManager.Instance?.DesequiparParaSlotDisponivel(indiceSlotEquip, indiceSlotVisualPainel);
    }

    public void TentarMoverPossuidaParaSlotVisual(int indiceSlotVisualOrigem, int indiceSlotVisualDestino, string idReliquia)
    {
        if (ReliquiasManager.Instance == null || string.IsNullOrEmpty(idReliquia))
            return;
        if (indiceSlotVisualOrigem >= 0 && indiceSlotVisualOrigem == indiceSlotVisualDestino)
            return;
        ReliquiasManager.Instance.MoverReliquiaPossuidaParaPosicaoVisual(idReliquia, indiceSlotVisualDestino);
    }

    public void TentarMoverEntreSlots(int origem, int destino, string idReliquia)
    {
        if (ReliquiasManager.Instance == null)
            return;
        ReliquiasManager.Instance.DesequiparSlot(origem);
        ReliquiasManager.Instance.EquiparNoSlot(destino, idReliquia);
    }
}
