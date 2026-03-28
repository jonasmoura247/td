using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PainelBestiario : MonoBehaviour
{
    [SerializeField] private BestiarioDatabase database;
    [SerializeField] private TextMeshProUGUI textoPontos;
    [SerializeField] private TextMeshProUGUI textoTituloDetalhe;
    [SerializeField] private TextMeshProUGUI textoInfoCriatura;
    [SerializeField] private RectTransform areaListaCriaturas;
    [SerializeField] private RectTransform areaBotoesRunas;
    [SerializeField] private Vector2 tamanhoCelula = new Vector2(100f, 100f);
    [SerializeField] private float espacamentoColunas = 24f;
    [SerializeField] private float espacamentoLinhas = 16f;
    [SerializeField] private float larguraMinimaColuna = 128f;
    [SerializeField] private bool contentAlturaAutomatica;

    private TalentoTooltipUI tooltipCache;
    private Coroutine rotinaMontar;
    private string criaturaSelecionadaId;

    public void AoAbrirPainel()
    {
        MontarGrid();
        AtualizarTextos();
        AtualizarPainelDetalhe();
    }

    private void OnEnable()
    {
        AtualizarTextos();
    }

    public void AtualizarTextos()
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || PerfilJogadorAtivo.Dados == null)
            return;
        PerfilJogadorAtivo.Dados.NormalizarBestiario();
        if (textoPontos != null)
            textoPontos.text = "Pontos bestiario: " + PerfilJogadorAtivo.Dados.bestiario.pontosBestiario;
    }

    public void SelecionarCriatura(string criaturaId)
    {
        criaturaSelecionadaId = criaturaId;
        AtualizarPainelDetalhe();
    }

    public void TentarAlternarRuna(string runaId)
    {
        if (SaveSlotManager.Instance == null || !PerfilJogadorAtivo.TemPerfilAtivo)
            return;
        if (string.IsNullOrEmpty(criaturaSelecionadaId) || string.IsNullOrEmpty(runaId))
            return;

        var db = BestiarioCatalogoUtil.CarregarOuNull(database);
        if (db == null)
            return;

        string equipada = BestiarioRegras.ObterRunaEquipadaNaCriatura(
            PerfilJogadorAtivo.Dados.bestiario, criaturaSelecionadaId);
        if (equipada == runaId)
        {
            SaveSlotManager.Instance.AtualizarSlotAtivo(d =>
                BestiarioRegras.TentarDesequiparRuna(criaturaSelecionadaId, runaId, db, d));
        }
        else
        {
            SaveSlotManager.Instance.AtualizarSlotAtivo(d =>
                BestiarioRegras.TentarEquiparRuna(criaturaSelecionadaId, runaId, db, d));
        }

        AtualizarTextos();
        AtualizarPainelDetalhe();
        MontarGrid();
    }

    private void GarantirTooltip()
    {
        if (tooltipCache != null)
            return;
        Canvas canvas = GetComponentInParent<Canvas>();
        tooltipCache = TalentoTooltipUI.GarantirNaCanvas(canvas);
    }

    private static void LimparFilhos(Transform t)
    {
        if (t == null)
            return;
        for (int i = t.childCount - 1; i >= 0; i--)
            Destroy(t.GetChild(i).gameObject);
    }

    public void MontarGrid()
    {
        if (areaListaCriaturas == null)
            return;
        if (!PerfilJogadorAtivo.TemPerfilAtivo || PerfilJogadorAtivo.Dados == null)
            return;

        if (rotinaMontar != null)
            StopCoroutine(rotinaMontar);
        rotinaMontar = StartCoroutine(MontarGridCoroutine());
    }

    private IEnumerator MontarGridCoroutine()
    {
        GarantirTooltip();
        var db = BestiarioCatalogoUtil.CarregarOuNull(database);
        if (db == null || db.criaturas == null || db.criaturas.Length == 0)
        {
            rotinaMontar = null;
            yield break;
        }

        LimparFilhos(areaListaCriaturas);
        yield return null;

        MontarListaVisual(db);
        rotinaMontar = null;
    }

    private void MontarListaVisual(BestiarioDatabase db)
    {
        var hor = areaListaCriaturas.gameObject.GetComponent<HorizontalLayoutGroup>();
        if (hor == null)
            hor = areaListaCriaturas.gameObject.AddComponent<HorizontalLayoutGroup>();
        hor.spacing = espacamentoColunas;
        hor.childAlignment = TextAnchor.UpperCenter;
        hor.childControlWidth = true;
        hor.childControlHeight = true;
        hor.childForceExpandWidth = false;
        hor.childForceExpandHeight = contentAlturaAutomatica;

        for (int c = 0; c < 3; c++)
        {
            GameObject colGo = new GameObject("Coluna" + c, typeof(RectTransform));
            colGo.transform.SetParent(areaListaCriaturas, false);
            var vl = colGo.AddComponent<VerticalLayoutGroup>();
            vl.spacing = espacamentoLinhas;
            vl.childAlignment = TextAnchor.UpperCenter;
            vl.childControlWidth = false;
            vl.childControlHeight = false;
            var leCol = colGo.AddComponent<LayoutElement>();
            leCol.flexibleWidth = 1f;
            leCol.minWidth = larguraMinimaColuna;
        }

        var lista = new List<BestiarioCriaturaDef>();
        for (int i = 0; i < db.criaturas.Length; i++)
        {
            if (db.criaturas[i] != null && !string.IsNullOrEmpty(db.criaturas[i].id))
                lista.Add(db.criaturas[i]);
        }

        lista.Sort((a, b) => string.CompareOrdinal(a.nomeExibicao, b.nomeExibicao));

        PerfilJogadorAtivo.Dados.NormalizarBestiario();

        for (int i = 0; i < lista.Count; i++)
        {
            var def = lista[i];
            int mortes = BestiarioRegras.ObterMortes(PerfilJogadorAtivo.Dados, def.id);
            int col = i % 3;
            if (areaListaCriaturas.childCount <= col)
                continue;
            Transform colTf = areaListaCriaturas.GetChild(col);
            BestiarioSlotUI.CriarSlot(colTf, def, mortes, this, tooltipCache, tamanhoCelula);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(areaListaCriaturas);
    }

    private void AtualizarPainelDetalhe()
    {
        if (textoTituloDetalhe != null)
            textoTituloDetalhe.text = "Runas";

        var db = BestiarioCatalogoUtil.CarregarOuNull(database);
        if (db == null || areaBotoesRunas == null || !PerfilJogadorAtivo.TemPerfilAtivo)
            return;

        LimparFilhos(areaBotoesRunas);

        if (string.IsNullOrEmpty(criaturaSelecionadaId))
        {
            if (textoInfoCriatura != null)
                textoInfoCriatura.text = "Selecione uma criatura.";
            return;
        }

        var criatura = db.ObterCriatura(criaturaSelecionadaId);
        if (criatura == null)
        {
            if (textoInfoCriatura != null)
                textoInfoCriatura.text = "";
            return;
        }

        PerfilJogadorAtivo.Dados.NormalizarBestiario();
        int mortes = BestiarioRegras.ObterMortes(PerfilJogadorAtivo.Dados, criaturaSelecionadaId);
        int estagios = BestiarioRegras.ObterEstagiosCompletos(criatura, mortes);
        string equipada = BestiarioRegras.ObterRunaEquipadaNaCriatura(
            PerfilJogadorAtivo.Dados.bestiario, criaturaSelecionadaId);

        if (textoInfoCriatura != null)
        {
            textoInfoCriatura.text =
                $"{criatura.nomeExibicao}\nMortes: {mortes}\nEstagios completos: {estagios}\nRuna equipada: {(string.IsNullOrEmpty(equipada) ? "nenhuma" : equipada)}";
        }

        var runas = db.ObterRunasDaCriatura(criaturaSelecionadaId);
        var vl = areaBotoesRunas.gameObject.GetComponent<VerticalLayoutGroup>();
        if (vl == null)
        {
            vl = areaBotoesRunas.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 8f;
            vl.childAlignment = TextAnchor.UpperLeft;
            vl.childControlWidth = true;
            vl.childControlHeight = true;
        }

        for (int i = 0; i < runas.Length; i++)
        {
            var r = runas[i];
            if (r == null)
                continue;

            bool pode = BestiarioRegras.PodeEquiparRuna(criaturaSelecionadaId, r.id, db, PerfilJogadorAtivo.Dados, out string motivo);
            bool jaEquipada = equipada == r.id;
            if (jaEquipada)
                pode = true;

            GameObject linha = new GameObject("Runa_" + r.id, typeof(RectTransform));
            linha.transform.SetParent(areaBotoesRunas, false);
            var h = linha.AddComponent<HorizontalLayoutGroup>();
            h.childAlignment = TextAnchor.MiddleLeft;
            h.spacing = 12f;
            h.childControlWidth = false;
            h.childForceExpandWidth = false;

            GameObject txtGo = new GameObject("Txt", typeof(RectTransform));
            txtGo.transform.SetParent(linha.transform, false);
            var tmp = txtGo.AddComponent<TextMeshProUGUI>();
            if (TMP_Settings.defaultFontAsset != null)
                tmp.font = TMP_Settings.defaultFontAsset;
            tmp.fontSize = 14f;
            tmp.text = $"{r.nomeExibicao} — custo {r.custoPontos} — min. estagios {r.estagiosMinimos}";
            if (!pode && !jaEquipada)
                tmp.text += $"  ({motivo})";
            if (jaEquipada)
                tmp.text += "  [equipada — clique para remover]";
            var leTxt = txtGo.AddComponent<LayoutElement>();
            leTxt.preferredWidth = 420f;
            leTxt.flexibleWidth = 1f;

            GameObject btnGo = new GameObject("Btn", typeof(RectTransform));
            btnGo.transform.SetParent(linha.transform, false);
            var img = btnGo.AddComponent<Image>();
            img.color = jaEquipada ? new Color(0.5f, 0.2f, 0.2f) : new Color(0.2f, 0.35f, 0.5f);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.interactable = pode || jaEquipada;
            var leBtn = btnGo.AddComponent<LayoutElement>();
            leBtn.preferredWidth = 100f;
            leBtn.preferredHeight = 28f;

            GameObject btnLabel = new GameObject("Label", typeof(RectTransform));
            btnLabel.transform.SetParent(btnGo.transform, false);
            var rt = btnLabel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var tmpBtn = btnLabel.AddComponent<TextMeshProUGUI>();
            if (TMP_Settings.defaultFontAsset != null)
                tmpBtn.font = TMP_Settings.defaultFontAsset;
            tmpBtn.alignment = TextAlignmentOptions.Midline;
            tmpBtn.fontSize = 12f;
            tmpBtn.text = jaEquipada ? "Remover" : "Equipar";

            string rid = r.id;
            btn.onClick.AddListener(() => TentarAlternarRuna(rid));
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(areaBotoesRunas);
    }

    private void OnDisable()
    {
        if (rotinaMontar != null)
        {
            StopCoroutine(rotinaMontar);
            rotinaMontar = null;
        }
    }
}
