using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Item de herói exibido no painel de heróis do lobby.
/// Mostra nome, fases restantes e aviso visual quando na penúltima fase.
/// Configure via inspector: textoNome, textoFases, iconeAlerta, fundo.
/// </summary>
public class HeroiItemController : MonoBehaviour
{
    [SerializeField] private TMP_Text textoNome;
    [SerializeField] private TMP_Text textoFases;
    [SerializeField] private GameObject iconeAlerta;
    [SerializeField] private Image fundo;

    [SerializeField] private Color corNormal = Color.white;
    [SerializeField] private Color corAlerta = new Color(1f, 0.85f, 0f, 1f);

    public void ConfigurarContrato(ContratoHeroi contrato)
    {
        if (contrato == null) return;

        string nomeExibicao = HeroisManager.Instance != null
            ? HeroisManager.Instance.ObterNomeExibicao(contrato.idHeroi)
            : contrato.idHeroi;

        if (textoNome != null)
            textoNome.text = nomeExibicao;

        if (textoFases != null)
        {
            textoFases.text = contrato.NaPenultimaFase
                ? $"Última jornada! ({contrato.fasesRestantes} fase)"
                : $"Fases restantes: {contrato.fasesRestantes}";
        }

        bool emAlerta = contrato.NaPenultimaFase;
        if (iconeAlerta != null) iconeAlerta.SetActive(emAlerta);
        if (fundo != null) fundo.color = emAlerta ? corAlerta : corNormal;
    }
}
