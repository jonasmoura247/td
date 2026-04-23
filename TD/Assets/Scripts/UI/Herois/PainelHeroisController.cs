using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Controla o painel de heróis no lobby.
/// Exibe lista de heróis contratados, fases restantes de cada contrato,
/// e destaca heróis na penúltima fase com cor de alerta.
/// Configure via inspector: painel, containerItens, prefabItemHeroi.
/// </summary>
public class PainelHeroisController : MonoBehaviour
{
    [SerializeField] private GameObject painel;
    [SerializeField] private RectTransform areaConteudoPainel;
    [SerializeField] private Transform containerItens;
    [SerializeField] private GameObject prefabItemHeroi;
    [SerializeField] private bool fecharAoClicarFora = true;
    [SerializeField] private bool fecharComEsc = true;

    private void Start()
    {
        painel?.SetActive(false);

        if (HeroisManager.Instance != null)
        {
            HeroisManager.Instance.OnContratoExpirando += OnContratoExpirando;
            HeroisManager.Instance.OnContratoExpirado += OnContratoExpirado;
        }
    }

    private void OnDestroy()
    {
        if (HeroisManager.Instance != null)
        {
            HeroisManager.Instance.OnContratoExpirando -= OnContratoExpirando;
            HeroisManager.Instance.OnContratoExpirado -= OnContratoExpirado;
        }
    }

    private void Update()
    {
        if (painel == null || !painel.activeSelf)
            return;

        if (fecharComEsc && Input.GetKeyDown(KeyCode.Escape))
        {
            FecharPainel();
            return;
        }

        if (fecharAoClicarFora && Input.GetMouseButtonDown(0) && CliqueFoiForaDoConteudo())
            FecharPainel();
    }

    public void AbrirPainel()
    {
        painel?.SetActive(true);
        AtualizarLista();
    }

    public void FecharPainel()
    {
        painel?.SetActive(false);
    }

    public void AtualizarLista()
    {
        if (containerItens == null || prefabItemHeroi == null) return;

        LimparItens();

        IReadOnlyList<ContratoHeroi> contratos = HeroisManager.Instance?.GetContratosAtivos();
        if (contratos == null) return;

        foreach (var contrato in contratos)
        {
            var item = Instantiate(prefabItemHeroi, containerItens);
            var controller = item.GetComponent<HeroiItemController>();
            controller?.ConfigurarContrato(contrato);
        }
    }

    private void LimparItens()
    {
        foreach (Transform filho in containerItens)
            Destroy(filho.gameObject);
    }

    private void OnContratoExpirando(ContratoHeroi contrato)
    {
        // Atualiza a lista para refletir o estado de alerta
        if (painel != null && painel.activeSelf)
            AtualizarLista();
    }

    private void OnContratoExpirado(ContratoHeroi contrato)
    {
        if (painel != null && painel.activeSelf)
            AtualizarLista();
    }

    private bool CliqueFoiForaDoConteudo()
    {
        RectTransform area = ObterAreaConteudo();
        if (area == null)
            return false;

        bool dentro = RectTransformUtility.RectangleContainsScreenPoint(area, Input.mousePosition, null);
        return !dentro;
    }

    private RectTransform ObterAreaConteudo()
    {
        if (areaConteudoPainel != null)
            return areaConteudoPainel;

        return painel != null ? painel.transform as RectTransform : null;
    }
}
