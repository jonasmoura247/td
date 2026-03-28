using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HudProgressaoRacaView : MonoBehaviour
{
    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI textoNomeRaca;
    [SerializeField] private TextMeshProUGUI textoNivel;
    [SerializeField] private TextMeshProUGUI textoExpNoSegmento;
    [SerializeField] private TextMeshProUGUI textoProximoNivelOpcional;

    [Header("Barra (Slider OU Image)")]
    [SerializeField] private Slider sliderProgresso;
    [SerializeField] private Image imagemFill;

    [Header("Sem perfil")]
    [SerializeField] private GameObject raizVisivelSoComPerfil;
    [SerializeField] private bool ocultarRaizSeSemPerfil = true;

    [Header("Formatacao")]
    [SerializeField] private string formatoNomeRaca = "{0}";
    [SerializeField] private string formatoNivel = "Level {0}";
    [SerializeField] private string formatoExpSegmento = "{0} / {1} EXP";
    [SerializeField] private string formatoProximoNivel = "Proximo nivel: {0}";
    [SerializeField] private bool mostrarLinhaProximoNivel = true;

    [Header("Eventos (animacao / VFX futuros)")]
    [SerializeField] private UnityEvent onProgressaoReexibida;
    [SerializeField] private UnityEvent<int> onNivelAumentouEmRelacaoAoAnterior;

    private ProgressaoRacaSnapshot ultimoSnapshotAplicado;
    private bool jaAplicouAlgumSnapshot;

    private void OnEnable()
    {
        RunExpPartida.OnExpTemporariaAlterada += AoExpTemporariaAlterada;
        if (SaveSlotManager.Instance != null)
        {
            SaveSlotManager.Instance.OnSlotsCarregados += AoSlotsOuPerfilMudarem;
            SaveSlotManager.Instance.OnDadosPerfilPersistidos += AoDadosPerfilPersistidos;
        }
        Reexibir();
    }

    private void Start()
    {
        if (SaveSlotManager.Instance != null)
        {
            SaveSlotManager.Instance.OnSlotsCarregados -= AoSlotsOuPerfilMudarem;
            SaveSlotManager.Instance.OnDadosPerfilPersistidos -= AoDadosPerfilPersistidos;
            SaveSlotManager.Instance.OnSlotsCarregados += AoSlotsOuPerfilMudarem;
            SaveSlotManager.Instance.OnDadosPerfilPersistidos += AoDadosPerfilPersistidos;
        }
        Reexibir();
    }

    private void OnDisable()
    {
        RunExpPartida.OnExpTemporariaAlterada -= AoExpTemporariaAlterada;
        if (SaveSlotManager.Instance != null)
        {
            SaveSlotManager.Instance.OnSlotsCarregados -= AoSlotsOuPerfilMudarem;
            SaveSlotManager.Instance.OnDadosPerfilPersistidos -= AoDadosPerfilPersistidos;
        }
    }

    private void AoExpTemporariaAlterada()
    {
        Reexibir();
    }

    private void AoSlotsOuPerfilMudarem(System.Collections.Generic.List<DadosSlotSave> _)
    {
        Reexibir();
    }

    private void AoDadosPerfilPersistidos()
    {
        Reexibir();
    }

    public void Reexibir()
    {
        ProgressaoRacaSnapshot snap = ProgressaoRacaAtivaLeitura.ObterSnapshotRacaAtiva();
        AplicarSnapshot(snap);
    }

    protected virtual void AplicarSnapshot(ProgressaoRacaSnapshot snap)
    {
        bool tem = snap.TemDadosValidos;
        if (raizVisivelSoComPerfil != null && ocultarRaizSeSemPerfil)
            raizVisivelSoComPerfil.SetActive(tem);

        if (!tem)
        {
            if (textoNomeRaca != null) textoNomeRaca.text = "";
            if (textoNivel != null) textoNivel.text = "";
            if (textoExpNoSegmento != null) textoExpNoSegmento.text = "";
            if (textoProximoNivelOpcional != null) textoProximoNivelOpcional.gameObject.SetActive(false);
            AtualizarBarraVisual(0f);
            jaAplicouAlgumSnapshot = true;
            ultimoSnapshotAplicado = ProgressaoRacaSnapshot.Vazio();
            onProgressaoReexibida?.Invoke();
            return;
        }

        if (textoNomeRaca != null)
            textoNomeRaca.text = string.Format(formatoNomeRaca, snap.NomeExibicao);
        if (textoNivel != null)
            textoNivel.text = string.Format(formatoNivel, snap.Nivel);
        if (textoExpNoSegmento != null)
            textoExpNoSegmento.text = string.Format(formatoExpSegmento, snap.ExpAcumuladaNoNivelAtual, snap.ExpTotalParaCompletarSegmentoDoNivel);

        if (textoProximoNivelOpcional != null)
        {
            textoProximoNivelOpcional.gameObject.SetActive(mostrarLinhaProximoNivel);
            if (mostrarLinhaProximoNivel)
                textoProximoNivelOpcional.text = string.Format(formatoProximoNivel, snap.ProximoNivel);
        }

        AtualizarBarraVisual(snap.ProgressoDentroDoNivel01);

        if (jaAplicouAlgumSnapshot && ultimoSnapshotAplicado.TemDadosValidos && snap.Nivel > ultimoSnapshotAplicado.Nivel)
            onNivelAumentouEmRelacaoAoAnterior?.Invoke(snap.Nivel);

        jaAplicouAlgumSnapshot = true;
        ultimoSnapshotAplicado = snap;
        onProgressaoReexibida?.Invoke();
    }

    protected virtual void AtualizarBarraVisual(float progresso01)
    {
        if (sliderProgresso != null)
        {
            sliderProgresso.minValue = 0f;
            sliderProgresso.maxValue = 1f;
            sliderProgresso.value = progresso01;
        }
        if (imagemFill != null)
            imagemFill.fillAmount = progresso01;
    }
}
