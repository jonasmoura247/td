using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PainelArvoreTalentos : MonoBehaviour
{
    [SerializeField] private TalentTreeDatabase database;
    [SerializeField] private TextMeshProUGUI textoPontos;
    [SerializeField] private TextMeshProUGUI textoNivelRaca;
    [SerializeField] private RectTransform areaListaNos;
    [SerializeField] private Vector2 tamanhoCelulaQuadrado = new Vector2(100f, 100f);
    [SerializeField] private float espacamentoEntreColunas = 24f;
    [SerializeField] private float espacamentoEntreLinhas = 16f;
    [SerializeField] private float larguraMinimaColuna = 128f;

#if UNITY_EDITOR
    [SerializeField] private TipoRaca racaPreviewEditor = TipoRaca.Goblin;
    [SerializeField] private bool previewComoTodosDesbloqueados;
#endif

    [SerializeField] private bool contentAlturaAutomatica;

    private TalentoTooltipUI tooltipCache;
    private Coroutine rotinaMontarLista;

    public void AoAbrirPainel()
    {
        MontarListaDeNos();
        AtualizarTexto();
    }

    private void OnEnable()
    {
        AtualizarTexto();
    }

    public void AtualizarTexto()
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || PerfilJogadorAtivo.Dados == null)
            return;

        var db = ModificadoresPartidaTalentos.CarregarDatabaseOuNull(database);
        if (db != null)
            db.RecalcularPontosDisponiveis(PerfilJogadorAtivo.Dados);

        if (textoPontos != null)
            textoPontos.text = "Pontos: " + PerfilJogadorAtivo.Dados.talentos.pontosDisponiveis;

        if (textoNivelRaca != null)
        {
            int exp = PerfilJogadorAtivo.Dados.ObterExpPermanenteRaca(PerfilJogadorAtivo.Dados.raca);
            int nv = NivelRacaCalculadora.CalcularNivelPorExpTotal(exp);
            textoNivelRaca.text = "Nivel " + nv + " — " + PerfilJogadorAtivo.Dados.raca;
        }
    }

    public void TentarAlternarNoPorId(string noId)
    {
        if (SaveSlotManager.Instance == null || !PerfilJogadorAtivo.TemPerfilAtivo)
            return;
        SaveSlotManager.Instance.AtualizarSlotAtivo(d =>
        {
            d.NormalizarTalentos();
            if (d.talentos.nosDesbloqueados != null && d.talentos.nosDesbloqueados.Contains(noId))
                TalentosRegras.TentarReverter(noId, database, d);
            else
                TalentosRegras.TentarDesbloquear(noId, database, d);
        });
        AtualizarTexto();
        MontarListaDeNos();
    }

    private void OnDisable()
    {
        if (rotinaMontarLista != null)
        {
            StopCoroutine(rotinaMontarLista);
            rotinaMontarLista = null;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (contentAlturaAutomatica || areaListaNos == null || Application.isPlaying)
            return;
        if (areaListaNos.GetComponent<ContentSizeFitter>() == null)
            return;
        EditorApplication.delayCall += RemoverContentSizeFitterAposValidate;
    }

    private void RemoverContentSizeFitterAposValidate()
    {
        if (this == null || areaListaNos == null || contentAlturaAutomatica)
            return;
        ContentSizeFitter csf = areaListaNos.GetComponent<ContentSizeFitter>();
        if (csf != null)
            DestroyImmediate(csf);
    }
#endif

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
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(t.GetChild(i).gameObject);
            else
#endif
                Destroy(t.GetChild(i).gameObject);
        }
    }

    private static void DestruirComponenteSeExistir<T>(GameObject go) where T : Component
    {
        if (go == null)
            return;
        T c = go.GetComponent<T>();
        if (c == null)
            return;
#if UNITY_EDITOR
        if (!Application.isPlaying)
            DestroyImmediate(c);
        else
#endif
        Destroy(c);
    }

#if UNITY_EDITOR
    public void MontarPreviewNoEditor()
    {
        if (Application.isPlaying || areaListaNos == null)
            return;
        GarantirTooltip();
        var db = ModificadoresPartidaTalentos.CarregarDatabaseOuNull(database);
        if (db == null)
            return;

        List<string> desbloqueados;
        if (previewComoTodosDesbloqueados)
        {
            desbloqueados = new List<string>();
            var nosPrev = db.ObterNos(racaPreviewEditor);
            if (nosPrev != null)
            {
                for (int i = 0; i < nosPrev.Length; i++)
                {
                    if (nosPrev[i] != null && !string.IsNullOrEmpty(nosPrev[i].id))
                        desbloqueados.Add(nosPrev[i].id);
                }
            }
        }
        else
        {
            desbloqueados = new List<string>();
        }

        Undo.RegisterFullObjectHierarchyUndo(areaListaNos.gameObject, "Preview arvore talentos");
        LimparFilhos(areaListaNos);
        MontarListaVisual(db, racaPreviewEditor, desbloqueados);
        EditorUtility.SetDirty(areaListaNos.gameObject);
        EditorUtility.SetDirty(gameObject);
    }

    public void LimparAreaListaNoEditor()
    {
        if (Application.isPlaying || areaListaNos == null)
            return;
        Undo.RegisterFullObjectHierarchyUndo(areaListaNos.gameObject, "Limpar arvore talentos");
        LimparFilhos(areaListaNos);
        EditorUtility.SetDirty(areaListaNos.gameObject);
        EditorUtility.SetDirty(gameObject);
    }
#endif

    public void MontarListaDeNos()
    {
        if (areaListaNos == null)
            return;
        if (!PerfilJogadorAtivo.TemPerfilAtivo || PerfilJogadorAtivo.Dados == null)
            return;

        if (rotinaMontarLista != null)
            StopCoroutine(rotinaMontarLista);
        rotinaMontarLista = StartCoroutine(MontarListaDeNosCoroutine());
    }

    private IEnumerator MontarListaDeNosCoroutine()
    {
        GarantirTooltip();

        var db = ModificadoresPartidaTalentos.CarregarDatabaseOuNull(database);
        if (db == null)
        {
            rotinaMontarLista = null;
            yield break;
        }

        TipoRaca raca = PerfilJogadorAtivo.Dados.raca;
        var nos = db.ObterNos(raca);
        if (nos == null || nos.Length == 0)
        {
            rotinaMontarLista = null;
            yield break;
        }

        LimparFilhos(areaListaNos);
        yield return null;

        MontarListaVisual(db, raca, PerfilJogadorAtivo.Dados.talentos.nosDesbloqueados);
        rotinaMontarLista = null;
    }

    private void MontarListaVisual(TalentTreeDatabase db, TipoRaca raca, List<string> desbloqueados)
    {
        if (db == null || areaListaNos == null)
            return;
        var nos = db.ObterNos(raca);
        if (nos == null || nos.Length == 0)
            return;

        DestruirComponenteSeExistir<VerticalLayoutGroup>(areaListaNos.gameObject);

        HorizontalLayoutGroup hor = areaListaNos.GetComponent<HorizontalLayoutGroup>();
        if (hor == null)
            hor = areaListaNos.gameObject.AddComponent<HorizontalLayoutGroup>();
        hor.spacing = espacamentoEntreColunas;
        hor.childAlignment = TextAnchor.UpperCenter;
        hor.childControlWidth = true;
        hor.childControlHeight = true;
        hor.childForceExpandWidth = false;
        hor.childForceExpandHeight = contentAlturaAutomatica;

        ContentSizeFitter csf = areaListaNos.GetComponent<ContentSizeFitter>();
        if (contentAlturaAutomatica)
        {
            if (csf == null)
                csf = areaListaNos.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        else
        {
            if (csf != null)
                DestruirComponenteSeExistir<ContentSizeFitter>(areaListaNos.gameObject);
        }

        for (int c = 0; c < 3; c++)
        {
            GameObject colGo = new GameObject("Coluna" + c, typeof(RectTransform));
            colGo.transform.SetParent(areaListaNos, false);
            VerticalLayoutGroup vl = colGo.AddComponent<VerticalLayoutGroup>();
            vl.spacing = espacamentoEntreLinhas;
            vl.childAlignment = TextAnchor.UpperCenter;
            vl.childControlWidth = false;
            vl.childControlHeight = false;
            vl.childForceExpandWidth = false;
            vl.childForceExpandHeight = false;
            LayoutElement leCol = colGo.AddComponent<LayoutElement>();
            leCol.flexibleWidth = 1f;
            leCol.minWidth = larguraMinimaColuna;
        }

        var lista = new List<TalentNodeEntry>();
        for (int i = 0; i < nos.Length; i++)
        {
            if (nos[i] != null && !string.IsNullOrEmpty(nos[i].id))
                lista.Add(nos[i]);
        }
        lista.Sort((a, b) =>
        {
            int cmp = a.coluna.CompareTo(b.coluna);
            if (cmp != 0) return cmp;
            return a.linha.CompareTo(b.linha);
        });

        for (int i = 0; i < lista.Count; i++)
        {
            TalentNodeEntry no = lista[i];
            int col = Mathf.Clamp(no.coluna, 0, 2);
            if (areaListaNos.childCount <= col)
                continue;
            Transform colunaTf = areaListaNos.GetChild(col);
            bool ja = desbloqueados != null && desbloqueados.Contains(no.id);
            TalentoNoSlotUI.CriarSlot(
                colunaTf,
                no,
                ja,
                this,
                tooltipCache,
                tamanhoCelulaQuadrado);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(areaListaNos);
    }
}
