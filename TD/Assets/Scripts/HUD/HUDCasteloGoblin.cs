using UnityEngine;
using TMPro;

public class HUDCastelo : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private CasteloController casteloReferencia;
    [SerializeField] private TextMeshProUGUI textoVida;

    private void LateUpdate()
    {
        if (casteloReferencia != null && textoVida != null)
        {
            int vida = Mathf.Max(0, casteloReferencia.GetVidaAtual());
            textoVida.text = $"{vida}/{casteloReferencia.vidaMaxima}";
        }
        else if (casteloReferencia == null)
        {
            textoVida.text = "0/15";
        }
    }
}