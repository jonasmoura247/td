using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Painel de unidades: um botão principal por raça (mesma posição); só o da raça ativa fica visível.
/// Ao abrir o painel, mostra apenas o grupo de botões (mineiro/arqueiro/feiticeiro) da raça do jogador.
/// </summary>
public class PainelUnidadesController : MonoBehaviour
{
    [Header("Botões principais (um por raça)")]
    [SerializeField] private Button botaoUnidadesHumano;
    [SerializeField] private Button botaoUnidadesGoblin;
    [SerializeField] private Button botaoUnidadesElfo;
    [SerializeField] private Button botaoUnidadesAnao;

    [Header("Grupos de botões dentro do painel")]
    [SerializeField] private GameObject grupoUnidadesHumano;
    [SerializeField] private GameObject grupoUnidadesGoblin;
    [SerializeField] private GameObject grupoUnidadesElfo;
    [SerializeField] private GameObject grupoUnidadesAnao;

    [Header("Painel e overlay")]
    [SerializeField] private GameObject painelUnidades;
    [SerializeField] private Button overlayFundo;

    private void OnEnable()
    {
        if (overlayFundo != null)
            overlayFundo.onClick.AddListener(FecharPainel);
    }

    private void OnDisable()
    {
        RemoverListenersBotoesPrincipais();
        if (overlayFundo != null)
            overlayFundo.onClick.RemoveListener(FecharPainel);
    }

    private void Start()
    {
        AplicarRacaDoPerfil();
        RegistrarListenerBotaoPrincipalVisivel();
    }

    private void RegistrarListenerBotaoPrincipalVisivel()
    {
        RemoverListenersBotoesPrincipais();
        Button ativo = ObterBotaoParaRaca(ObterRacaAtiva());
        if (ativo != null)
            ativo.onClick.AddListener(TogglePainel);
    }

    private void RemoverListenersBotoesPrincipais()
    {
        if (botaoUnidadesHumano != null)
            botaoUnidadesHumano.onClick.RemoveListener(TogglePainel);
        if (botaoUnidadesGoblin != null)
            botaoUnidadesGoblin.onClick.RemoveListener(TogglePainel);
        if (botaoUnidadesElfo != null)
            botaoUnidadesElfo.onClick.RemoveListener(TogglePainel);
        if (botaoUnidadesAnao != null)
            botaoUnidadesAnao.onClick.RemoveListener(TogglePainel);
    }

    private Button ObterBotaoParaRaca(TipoRaca raca)
    {
        return raca switch
        {
            TipoRaca.Humano => botaoUnidadesHumano,
            TipoRaca.Goblin => botaoUnidadesGoblin,
            TipoRaca.Elfo => botaoUnidadesElfo,
            TipoRaca.Anao => botaoUnidadesAnao,
            _ => botaoUnidadesGoblin
        };
    }

    private void AplicarRacaDoPerfil()
    {
        TipoRaca raca = ObterRacaAtiva();

        if (botaoUnidadesHumano != null)
            botaoUnidadesHumano.gameObject.SetActive(raca == TipoRaca.Humano);
        if (botaoUnidadesGoblin != null)
            botaoUnidadesGoblin.gameObject.SetActive(raca == TipoRaca.Goblin);
        if (botaoUnidadesElfo != null)
            botaoUnidadesElfo.gameObject.SetActive(raca == TipoRaca.Elfo);
        if (botaoUnidadesAnao != null)
            botaoUnidadesAnao.gameObject.SetActive(raca == TipoRaca.Anao);

        AtualizarVisibilidadeGrupos(painelUnidades != null && painelUnidades.activeSelf);
    }

    private static TipoRaca ObterRacaAtiva()
    {
        if (PerfilJogadorAtivo.TemPerfilAtivo)
            return PerfilJogadorAtivo.Raca;
        return TipoRaca.Goblin;
    }

    private void TogglePainel()
    {
        if (painelUnidades == null) return;
        bool abrir = !painelUnidades.activeSelf;
        painelUnidades.SetActive(abrir);
        AtualizarVisibilidadeGrupos(abrir);
    }

    private void FecharPainel()
    {
        if (painelUnidades != null)
            painelUnidades.SetActive(false);
        AtualizarVisibilidadeGrupos(false);
    }

    private void AtualizarVisibilidadeGrupos(bool painelAberto)
    {
        TipoRaca raca = ObterRacaAtiva();

        SetGrupoAtivo(grupoUnidadesHumano, painelAberto && raca == TipoRaca.Humano);
        SetGrupoAtivo(grupoUnidadesGoblin, painelAberto && raca == TipoRaca.Goblin);
        SetGrupoAtivo(grupoUnidadesElfo, painelAberto && raca == TipoRaca.Elfo);
        SetGrupoAtivo(grupoUnidadesAnao, painelAberto && raca == TipoRaca.Anao);
    }

    private static void SetGrupoAtivo(GameObject grupo, bool ativo)
    {
        if (grupo != null)
            grupo.SetActive(ativo);
    }
}
