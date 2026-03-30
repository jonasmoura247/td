using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PainelBestiario : MonoBehaviour
{
    private const string CaminhoLayoutResources = "Bestiario/BestiarioUILayout";

    [SerializeField] private BestiarioDatabase database;
    [SerializeField] private BestiarioUILayout layoutVisual;

    [SerializeField] private TextMeshProUGUI textoPontos;
    [SerializeField] private TextMeshProUGUI textoOuro;
    [SerializeField] private TextMeshProUGUI textoTituloDetalhe;
    [SerializeField] private TextMeshProUGUI textoInfoCriatura;

    [Header("Abas")]
    [SerializeField] private Button botaoTabCriaturas;
    [SerializeField] private Button botaoTabRunas;
    [SerializeField] private GameObject painelAbaCriaturas;
    [SerializeField] private GameObject painelAbaCatalogoRunas;

    [Header("Aba criaturas")]
    [SerializeField] private RectTransform areaListaCriaturas;
    [SerializeField] private RectTransform areaBotoesRunas;

    [Header("Aba catalogo runas")]
    [SerializeField] private RectTransform areaListaCatalogoRunas;

    [Header("Modo teste (runas)")]
    [Tooltip("Liga bypass de pontos, estágios e ouro ao remover runa. Desligar em build final.")]
    [SerializeField] private bool modoTesteRunas;

    [Header("Cores — linha de runa (detalhe)")]
    [SerializeField] private Color corBotaoLiberarEquipar = new Color(0.2f, 0.35f, 0.5f, 1f);
    [SerializeField] private Color corBotaoRemover = new Color(0.5f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color corBotaoBloqueado = new Color(0.35f, 0.35f, 0.38f, 1f);

    private TalentoTooltipUI tooltipCache;
    private Coroutine rotinaMontar;
    private Coroutine rotinaMontarCatalogo;
    private string criaturaSelecionadaId;
    private int indiceAbaAtual;
    private bool abasLigadas;

    private BestiarioUILayout LayoutResolvido =>
        layoutVisual != null ? layoutVisual : Resources.Load<BestiarioUILayout>(CaminhoLayoutResources);

    public void AoAbrirPainel()
    {
        BestiarioModoTeste.Ativo = modoTesteRunas;
        GarantirAbas();
        if (string.IsNullOrEmpty(criaturaSelecionadaId) && PerfilJogadorAtivo.TemPerfilAtivo)
        {
            var dbAuto = BestiarioCatalogoUtil.CarregarOuNull(database);
            if (dbAuto != null)
            {
                var visiveis = dbAuto.ObterCriaturasParaBestiarioDoJogador(PerfilJogadorAtivo.Raca);
                if (visiveis.Length > 0)
                {
                    var ord = new List<BestiarioCriaturaDef>(visiveis);
                    ord.Sort((a, b) => string.CompareOrdinal(a.nomeExibicao, b.nomeExibicao));
                    criaturaSelecionadaId = ord[0].id;
                }
            }
        }
        MudarAba(indiceAbaAtual);
        MontarGrid();
        MontarCatalogoRunas();
        AtualizarTextos();
        AtualizarPainelDetalhe();
    }

    private void GarantirAbas()
    {
        if (abasLigadas)
            return;
        abasLigadas = true;
        if (botaoTabCriaturas != null)
            botaoTabCriaturas.onClick.AddListener(() => MudarAba(0));
        if (botaoTabRunas != null)
            botaoTabRunas.onClick.AddListener(() => MudarAba(1));
    }

    private void OnEnable()
    {
        GarantirAbas();
        BestiarioModoTeste.Ativo = modoTesteRunas;
        AtualizarTextos();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            BestiarioModoTeste.Ativo = modoTesteRunas;
    }

    public void MudarAba(int indice)
    {
        indiceAbaAtual = indice;
        if (painelAbaCriaturas != null)
            painelAbaCriaturas.SetActive(indice == 0);
        if (painelAbaCatalogoRunas != null)
            painelAbaCatalogoRunas.SetActive(indice == 1);
    }

    public void SelecionarCriaturaIrParaAbaCriaturas(string criaturaId)
    {
        if (string.IsNullOrEmpty(criaturaId))
            return;
        SelecionarCriatura(criaturaId);
        MudarAba(0);
    }

    public void AtualizarTextos()
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || PerfilJogadorAtivo.Dados == null)
            return;
        PerfilJogadorAtivo.Dados.NormalizarBestiario();
        if (textoPontos != null)
            textoPontos.text = "Pontos bestiario: " + PerfilJogadorAtivo.Dados.bestiario.pontosBestiario;
        if (textoOuro != null)
            textoOuro.text = "Ouro: " + PerfilJogadorAtivo.Dados.ouro;
    }

    public void SelecionarCriatura(string criaturaId)
    {
        criaturaSelecionadaId = criaturaId;
        AtualizarPainelDetalhe();
    }

    public void TentarLiberarRunaPainel(string runaId)
    {
        if (!GarantirPerfilESlot())
            return;
        var db = BestiarioCatalogoUtil.CarregarOuNull(database);
        if (db == null)
            return;
        SaveSlotManager.Instance.AtualizarSlotAtivo(d =>
            BestiarioRegras.TentarLiberarRuna(runaId, db, d));
        RefrescarAposMudancaRuna();
    }

    public void TentarEquiparRunaPainel(string runaId)
    {
        if (!GarantirPerfilESlot())
            return;
        if (string.IsNullOrEmpty(criaturaSelecionadaId))
            return;
        var db = BestiarioCatalogoUtil.CarregarOuNull(database);
        if (db == null)
            return;
        SaveSlotManager.Instance.AtualizarSlotAtivo(d =>
            BestiarioRegras.TentarEquiparRuna(criaturaSelecionadaId, runaId, db, d));
        RefrescarAposMudancaRuna();
    }

    public void TentarRemoverRunaPainel(string runaId)
    {
        if (!GarantirPerfilESlot())
            return;
        if (string.IsNullOrEmpty(criaturaSelecionadaId))
            return;
        var db = BestiarioCatalogoUtil.CarregarOuNull(database);
        if (db == null)
            return;
        SaveSlotManager.Instance.AtualizarSlotAtivo(d =>
            BestiarioRegras.TentarRemoverRunaComOuro(criaturaSelecionadaId, runaId, db, d));
        RefrescarAposMudancaRuna();
    }

    private bool GarantirPerfilESlot()
    {
        return SaveSlotManager.Instance != null && PerfilJogadorAtivo.TemPerfilAtivo;
    }

    private void RefrescarAposMudancaRuna()
    {
        AtualizarTextos();
        AtualizarPainelDetalhe();
        MontarGrid();
        MontarCatalogoRunas();
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
        var layout = LayoutResolvido;
        if (db == null || layout == null || layout.prefabSlotCriatura == null)
        {
            rotinaMontar = null;
            yield break;
        }

        LimparFilhos(areaListaCriaturas);
        yield return null;

        MontarListaVisual(db, layout);
        rotinaMontar = null;
    }

    private void MontarListaVisual(BestiarioDatabase db, BestiarioUILayout layout)
    {
        int colunas = Mathf.Max(1, layout.colunasGrelha);
        var hor = areaListaCriaturas.gameObject.GetComponent<HorizontalLayoutGroup>();
        if (hor == null)
            hor = areaListaCriaturas.gameObject.AddComponent<HorizontalLayoutGroup>();
        hor.spacing = layout.espacamentoColunas;
        hor.childAlignment = TextAnchor.UpperCenter;
        hor.childControlWidth = true;
        hor.childControlHeight = true;
        hor.childForceExpandWidth = false;
        hor.childForceExpandHeight = layout.contentAlturaAutomatica;

        for (int c = 0; c < colunas; c++)
        {
            GameObject colGo = new GameObject("Coluna" + c, typeof(RectTransform));
            colGo.transform.SetParent(areaListaCriaturas, false);
            var vl = colGo.AddComponent<VerticalLayoutGroup>();
            vl.spacing = layout.espacamentoLinhas;
            vl.childAlignment = TextAnchor.UpperCenter;
            vl.childControlWidth = false;
            vl.childControlHeight = false;
            var leCol = colGo.AddComponent<LayoutElement>();
            leCol.flexibleWidth = 1f;
            leCol.minWidth = layout.larguraMinimaColuna;
        }

        var lista = new List<BestiarioCriaturaDef>();
        var doJogador = db.ObterCriaturasParaBestiarioDoJogador(PerfilJogadorAtivo.Raca);
        for (int i = 0; i < doJogador.Length; i++)
        {
            if (doJogador[i] != null && !string.IsNullOrEmpty(doJogador[i].id))
                lista.Add(doJogador[i]);
        }

        lista.Sort((a, b) => string.CompareOrdinal(a.nomeExibicao, b.nomeExibicao));
        PerfilJogadorAtivo.Dados.NormalizarBestiario();

        for (int i = 0; i < lista.Count; i++)
        {
            var def = lista[i];
            int mortes = BestiarioRegras.ObterMortes(PerfilJogadorAtivo.Dados, def.id);
            int col = i % colunas;
            if (areaListaCriaturas.childCount <= col)
                continue;
            Transform colTf = areaListaCriaturas.GetChild(col);
            GameObject inst = Instantiate(layout.prefabSlotCriatura, colTf);
            var slot = inst.GetComponent<BestiarioSlotCriaturaItem>();
            if (slot != null)
                slot.Configurar(def, mortes, this, tooltipCache);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(areaListaCriaturas);
    }

    public void MontarCatalogoRunas()
    {
        if (areaListaCatalogoRunas == null)
            return;
        if (!PerfilJogadorAtivo.TemPerfilAtivo || PerfilJogadorAtivo.Dados == null)
            return;
        if (rotinaMontarCatalogo != null)
            StopCoroutine(rotinaMontarCatalogo);
        rotinaMontarCatalogo = StartCoroutine(MontarCatalogoCoroutine());
    }

    private IEnumerator MontarCatalogoCoroutine()
    {
        GarantirTooltip();
        var db = BestiarioCatalogoUtil.CarregarOuNull(database);
        var layout = LayoutResolvido;
        if (db == null || layout == null || layout.prefabItemCatalogoRuna == null)
        {
            rotinaMontarCatalogo = null;
            yield break;
        }

        LimparFilhos(areaListaCatalogoRunas);
        yield return null;

        var vl = areaListaCatalogoRunas.gameObject.GetComponent<VerticalLayoutGroup>();
        if (vl == null)
        {
            vl = areaListaCatalogoRunas.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 10f;
            vl.childAlignment = TextAnchor.UpperCenter;
            vl.childControlWidth = true;
            vl.childControlHeight = true;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;
        }

        BestiarioRunaDef[] ordenadas = db.ObterRunasOrdenadas();
        PerfilJogadorAtivo.Dados.NormalizarBestiario();

        for (int i = 0; i < ordenadas.Length; i++)
        {
            var r = ordenadas[i];
            if (r == null)
                continue;
            var criLore = string.IsNullOrEmpty(r.criaturaAlvoId) ? null : db.ObterCriatura(r.criaturaAlvoId);
            bool habilitada = BestiarioRegras.RunaHabilitadaNoCatalogo(
                r.id, db, PerfilJogadorAtivo.Dados, out string motivo);
            string cidFoco = ObterCriaturaIdFocoCatalogo(db, r);
            Sprite fallback = criLore != null ? (criLore.icone != null ? criLore.icone : criLore.iconeSilhueta) : null;

            GameObject inst = Instantiate(layout.prefabItemCatalogoRuna, areaListaCatalogoRunas);
            var item = inst.GetComponent<BestiarioCatalogoRunaItem>();
            if (item == null)
                continue;

            item.Configurar(
                r,
                criLore,
                habilitada,
                layout.alphaRunaBloqueadaCatalogo,
                habilitada ? "" : motivo,
                () => { if (!string.IsNullOrEmpty(cidFoco)) SelecionarCriaturaIrParaAbaCriaturas(cidFoco); },
                tooltipCache,
                fallback);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(areaListaCatalogoRunas);
        rotinaMontarCatalogo = null;
    }

    private static string ObterCriaturaIdFocoCatalogo(BestiarioDatabase db, BestiarioRunaDef r)
    {
        if (r == null || db == null)
            return null;
        if (!string.IsNullOrEmpty(r.criaturaAlvoId) && db.ObterCriatura(r.criaturaAlvoId) != null)
            return r.criaturaAlvoId;
        var lista = db.ObterCriaturasParaBestiarioDoJogador(PerfilJogadorAtivo.Raca);
        return lista.Length > 0 ? lista[0].id : null;
    }

    private void AtualizarPainelDetalhe()
    {
        if (textoTituloDetalhe != null)
            textoTituloDetalhe.text = "Runas";

        var db = BestiarioCatalogoUtil.CarregarOuNull(database);
        var layout = LayoutResolvido;
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

        if (layout == null || layout.prefabLinhaRuna == null)
            return;

        PerfilJogadorAtivo.Dados.NormalizarBestiario();
        int mortes = BestiarioRegras.ObterMortes(PerfilJogadorAtivo.Dados, criaturaSelecionadaId);
        int estagios = BestiarioRegras.ObterEstagiosCompletos(criatura, mortes);
        string equipada = BestiarioRegras.ObterRunaEquipadaNaCriatura(
            PerfilJogadorAtivo.Dados.bestiario, criaturaSelecionadaId);
        int custoRem = BestiarioRegras.CalcularCustoOuroRemocaoRuna(PerfilJogadorAtivo.Dados);

        if (textoInfoCriatura != null)
        {
            textoInfoCriatura.text =
                $"{criatura.nomeExibicao}\nMortes: {mortes}\nEstagios completos: {estagios}\nRuna equipada: {(string.IsNullOrEmpty(equipada) ? "nenhuma" : equipada)}\nRemover runa custa {custoRem} ouro (por nivel da raca).";
        }

        var runas = db.ObterRunasOrdenadas();
        var vl = areaBotoesRunas.gameObject.GetComponent<VerticalLayoutGroup>();
        if (vl == null)
        {
            vl = areaBotoesRunas.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 8f;
            vl.childAlignment = TextAnchor.UpperLeft;
            vl.childControlWidth = true;
            vl.childControlHeight = true;
        }

        Sprite fallbackCriatura = criatura.icone != null ? criatura.icone : criatura.iconeSilhueta;

        for (int i = 0; i < runas.Length; i++)
        {
            var r = runas[i];
            if (r == null)
                continue;

            MontarLinhaRunaDetalhe(db, layout, r, fallbackCriatura, equipada, custoRem);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(areaBotoesRunas);
    }

    private void MontarLinhaRunaDetalhe(
        BestiarioDatabase db,
        BestiarioUILayout layout,
        BestiarioRunaDef r,
        Sprite fallbackCriatura,
        string equipadaId,
        int custoRemocaoOuro)
    {
        var b = PerfilJogadorAtivo.Dados.bestiario;
        bool liberada = BestiarioRegras.RunaEstaLiberada(b, r.id);
        bool jaEquipadaAqui = equipadaId == r.id;
        bool outraCriaturaSegura = BestiarioRegras.ObterCriaturaQueEquipaRuna(b, r.id, out string cidOutra)
            && !string.Equals(cidOutra, criaturaSelecionadaId, System.StringComparison.Ordinal);

        string linhaBase = $"{r.nomeExibicao} — {BestiarioTextoRunaUtil.ResumoEfeito(r)}";
        string textoBotao = "";
        bool interact = false;
        Color cor = corBotaoBloqueado;
        System.Action<string> aoClicar = null;
        string extraLinha = "";

        if (!liberada)
        {
            bool podeLib = BestiarioRegras.PodeLiberarRuna(r.id, db, PerfilJogadorAtivo.Dados, out string motLib);
            textoBotao = "Liberar";
            interact = podeLib;
            cor = corBotaoLiberarEquipar;
            aoClicar = _ => TentarLiberarRunaPainel(r.id);
            extraLinha = podeLib ? $"\nLiberar: {r.custoPontosLiberacao} pts." : "\n" + motLib;
        }
        else if (jaEquipadaAqui)
        {
            bool podeRem = BestiarioRegras.PodeRemoverRunaComOuro(
                criaturaSelecionadaId, r.id, db, PerfilJogadorAtivo.Dados, out string motRem);
            textoBotao = $"Remover ({custoRemocaoOuro} ouro)";
            interact = podeRem;
            cor = corBotaoRemover;
            aoClicar = _ => TentarRemoverRunaPainel(r.id);
            extraLinha = podeRem ? "" : "\n" + motRem;
        }
        else if (outraCriaturaSegura)
        {
            var defOutra = db.ObterCriatura(cidOutra);
            string nomeOutra = defOutra != null ? defOutra.nomeExibicao : cidOutra;
            textoBotao = "Ocupada";
            interact = false;
            cor = corBotaoBloqueado;
            aoClicar = null;
            extraLinha = $"\nEquipada em: {nomeOutra}";
        }
        else
        {
            bool podeEq = BestiarioRegras.PodeEquiparRuna(criaturaSelecionadaId, r.id, db, PerfilJogadorAtivo.Dados, out string motEq);
            textoBotao = "Equipar";
            interact = podeEq;
            cor = corBotaoLiberarEquipar;
            aoClicar = _ => TentarEquiparRunaPainel(r.id);
            extraLinha = podeEq ? $"\nRequer {r.estagiosMinimos} estagio(s) nesta criatura." : "\n" + motEq;
        }

        GameObject inst = Instantiate(layout.prefabLinhaRuna, areaBotoesRunas);
        var linha = inst.GetComponent<BestiarioRunaLinhaItem>();
        if (linha == null)
            return;

        linha.Configurar(
            r,
            linhaBase + extraLinha,
            textoBotao,
            interact,
            cor,
            interact ? aoClicar : null,
            fallbackCriatura);
    }

    private void OnDisable()
    {
        if (rotinaMontar != null)
        {
            StopCoroutine(rotinaMontar);
            rotinaMontar = null;
        }
        if (rotinaMontarCatalogo != null)
        {
            StopCoroutine(rotinaMontarCatalogo);
            rotinaMontarCatalogo = null;
        }
    }
}
