using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tela principal com 4 slots de save.
/// Ao clicar em slot vazio: abre UICriacaoPerfil.
/// Ao clicar em slot preenchido: carrega o perfil e inicia o jogo.
/// </summary>
public class UISlotsPrincipal : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private UISlotSaveItem[] slots = new UISlotSaveItem[4];

    [Header("Painel de criação de perfil")]
    [SerializeField] private UICriacaoPerfil painelCriacao;

    [Header("Painel de opções (slot preenchido) - Iniciar ou Excluir")]
    [SerializeField] private GameObject painelOpcoesSlot;
    [SerializeField] private UnityEngine.UI.Button botaoIniciarPartida;
    [SerializeField] private UnityEngine.UI.Button botaoExcluirSave;
    [SerializeField] private UnityEngine.UI.Button botaoCancelarOpcoes;

    [Header("Painel de confirmação de exclusão")]
    [SerializeField] private GameObject painelConfirmarExcluir;
    [SerializeField] private UnityEngine.UI.Button botaoSimExcluir;
    [SerializeField] private UnityEngine.UI.Button botaoNaoExcluir;

    [Header("Limpar histórico (opcional)")]
    [SerializeField] private UnityEngine.UI.Button botaoLimparTodos;

    private int slotPendente = -1;

    private void Start()
    {
        if (SaveSlotManager.Instance == null)
        {
            Debug.LogError("[UISlotsPrincipal] SaveSlotManager nao existe na cena! Adicione um GameObject com SaveSlotManager.");
            return;
        }

        // Garante que temos 4 slots
        if (slots == null || slots.Length < 4)
        {
            var encontrados = GetComponentsInChildren<UISlotSaveItem>(true);
            slots = encontrados.Length >= 4 ? new UISlotSaveItem[4] : new UISlotSaveItem[Mathf.Max(1, encontrados.Length)];
            for (int i = 0; i < Mathf.Min(4, encontrados.Length); i++)
                slots[i] = encontrados[i];
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].Configurar(i, this);
                var dados = SaveSlotManager.Instance.GetSlot(i);
                slots[i].AtualizarVisual(dados);
            }
        }

        SaveSlotManager.Instance.OnSlotsCarregados += AtualizarTodosSlots;

        if (painelCriacao != null)
            painelCriacao.gameObject.SetActive(false);

        if (painelOpcoesSlot != null)
            painelOpcoesSlot.SetActive(false);
        if (painelConfirmarExcluir != null)
            painelConfirmarExcluir.SetActive(false);

        if (botaoIniciarPartida != null)
            botaoIniciarPartida.onClick.AddListener(IniciarPartida);
        if (botaoExcluirSave != null)
            botaoExcluirSave.onClick.AddListener(MostrarConfirmacaoExcluir);
        if (botaoCancelarOpcoes != null)
            botaoCancelarOpcoes.onClick.AddListener(FecharOpcoesSlot);
        if (botaoSimExcluir != null)
            botaoSimExcluir.onClick.AddListener(ConfirmarExcluir);
        if (botaoNaoExcluir != null)
            botaoNaoExcluir.onClick.AddListener(EsconderConfirmacaoExcluir);

        if (botaoLimparTodos != null)
            botaoLimparTodos.onClick.AddListener(LimparTodosSlots);
    }

    private void LimparTodosSlots()
    {
        if (SaveSlotManager.Instance == null) return;
        SaveSlotManager.Instance.LimparTodosOsSlots();
    }

    private void OnDestroy()
    {
        if (SaveSlotManager.Instance != null)
            SaveSlotManager.Instance.OnSlotsCarregados -= AtualizarTodosSlots;
    }

    /// <summary>Chamado por UISlotSaveItem ao clicar. Slot vazio = criar, preenchido = carregar.</summary>
    public void AoSolicitarSlot(int indice)
    {
        if (SaveSlotManager.Instance == null) return;

        if (SaveSlotManager.Instance.SlotVazio(indice))
        {
            if (painelCriacao != null)
                painelCriacao.Abrir(indice);
        }
        else
        {
            slotPendente = indice;
            AbrirOpcoesSlot();
        }
    }

    private void AbrirOpcoesSlot()
    {
        if (painelConfirmarExcluir != null)
            painelConfirmarExcluir.SetActive(false);

        if (painelOpcoesSlot != null)
            painelOpcoesSlot.SetActive(true);
    }

    private void FecharOpcoesSlot()
    {
        slotPendente = -1;
        if (painelOpcoesSlot != null)
            painelOpcoesSlot.SetActive(false);
        if (painelConfirmarExcluir != null)
            painelConfirmarExcluir.SetActive(false);
    }

    private void IniciarPartida()
    {
        if (slotPendente < 0) return;
        SaveSlotManager.Instance.CarregarPerfilEIniciar(slotPendente);
        slotPendente = -1;
        FecharOpcoesSlot();
    }

    private void MostrarConfirmacaoExcluir()
    {
        if (painelConfirmarExcluir != null)
            painelConfirmarExcluir.SetActive(true);
    }

    private void EsconderConfirmacaoExcluir()
    {
        if (painelConfirmarExcluir != null)
            painelConfirmarExcluir.SetActive(false);
    }

    private void ConfirmarExcluir()
    {
        if (slotPendente < 0) return;
        SaveSlotManager.Instance.ApagarSlot(slotPendente);
        slotPendente = -1;
        FecharOpcoesSlot();
    }

    private void AtualizarTodosSlots(List<DadosSlotSave> lista)
    {
        for (int i = 0; i < slots.Length && i < lista.Count; i++)
        {
            if (slots[i] != null)
                slots[i].AtualizarVisual(lista[i]);
        }
    }
}
