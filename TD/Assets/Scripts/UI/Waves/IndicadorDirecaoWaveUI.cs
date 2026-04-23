using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Componente de UI puro: exibe seta indicando a direção da próxima wave.
/// Controlado exclusivamente por EfeitoBussolaVerme.
/// </summary>
public class IndicadorDirecaoWaveUI : MonoBehaviour
{
    [SerializeField] private Image imagemSeta;
    [SerializeField] private TextMeshProUGUI textoLado;

    public bool EstaVisivel => gameObject.activeSelf;

    public void Mostrar(LadoSpawn lado)
    {
        gameObject.SetActive(true);

        if (imagemSeta != null)
            imagemSeta.rectTransform.localRotation =
                Quaternion.Euler(0f, 0f, BussolaVermeLogica.RotacaoZPorLado(lado));

        if (textoLado != null)
            textoLado.text = BussolaVermeLogica.NomePorLado(lado);
    }

    public void Esconder()
    {
        gameObject.SetActive(false);
    }
}
