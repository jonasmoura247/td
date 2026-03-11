using UnityEngine;
using TMPro;

public class MinerioDeOuro : RecursoController
{
    [Header("Texto Quantidade")]
    [SerializeField] private float larguraTexto = 4f;

    private TextMeshPro textoQuantidade;

    protected override void Start()
    {
        quantidadeMaxima = Random.Range(100, 251);
        quantidadeAtual = quantidadeMaxima;
        CriarTextoQuantidade();
    }

    private void LateUpdate()
    {
        if (textoQuantidade != null)
            textoQuantidade.text = Mathf.Max(0, quantidadeAtual).ToString();
    }

    protected override void Esgotar()
    {
        base.Esgotar();
    }

    private void CriarTextoQuantidade()
    {
        GameObject textGO = new GameObject("TextoQuantidade");
        textGO.transform.SetParent(transform);
        textGO.transform.localPosition = new Vector3(0, 3.5f, 0);
        textGO.transform.localRotation = Quaternion.identity;
        textGO.transform.localScale = Vector3.one;

        textoQuantidade = textGO.AddComponent<TextMeshPro>();
        textoQuantidade.text = quantidadeAtual.ToString();
        textoQuantidade.fontSize = 8;
        textoQuantidade.alignment = TextAlignmentOptions.Center;
        textoQuantidade.color = Color.white;

        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font != null)
            textoQuantidade.font = font;
        else
            textoQuantidade.font = TMP_Settings.defaultFontAsset;

        RectTransform rect = textGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(larguraTexto, 1f);
    }
}
