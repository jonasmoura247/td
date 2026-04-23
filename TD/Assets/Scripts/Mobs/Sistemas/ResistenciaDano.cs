using UnityEngine;

/// <summary>
/// Componente de resistência a tipos de dano. Adicione ao prefab do mob e configure os valores.
/// Integra-se com UnidadeController.ReceberDano() para reduzir dano antes de aplicar.
/// </summary>
public class ResistenciaDano : MonoBehaviour
{
    [Header("Resistências (0 = nenhuma, 1 = imunidade total)")]
    [Range(0f, 1f)] public float resistenciaFisica;
    [Range(0f, 1f)] public float resistenciaFogo;
    [Range(0f, 1f)] public float resistenciaVeneno;

    public bool EhImuneVeneno => resistenciaVeneno >= 1f;
    public bool EhImuneFogo   => resistenciaFogo   >= 1f;

    public int CalcularDanoFinal(int danoOriginal, TipoDanoCombate tipo)
    {
        float res = tipo switch
        {
            TipoDanoCombate.Fisico  => resistenciaFisica,
            TipoDanoCombate.Fogo    => resistenciaFogo,
            TipoDanoCombate.Veneno  => resistenciaVeneno,
            _                       => 0f
        };
        return ResistenciaDanoLogica.AplicarResistencia(danoOriginal, res);
    }
}
