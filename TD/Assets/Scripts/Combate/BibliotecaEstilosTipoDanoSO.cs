using UnityEngine;

[System.Serializable]
public class EntradaEstiloTipoDano
{
    public TipoDanoCombate tipo = TipoDanoCombate.Fisico;
    public Color corTexto = Color.white;
    [Min(1f)] public float tamanhoFonteBase = 56f;
    [Min(1f)] public float tamanhoFonteCritico = 72f;
    [Tooltip("Concatenado após o valor quando Critico = true (ex.: !).")]
    public string sufixoSeCritico = "!";
    public string prefixoOpcional = "";
    [Tooltip("Reservado para ícone / animação futura.")]
    public string idPresetVisualFuturo;
}

[CreateAssetMenu(fileName = "BibliotecaEstilosTipoDano", menuName = "TD/Combate/Biblioteca Estilos Tipo Dano")]
public class BibliotecaEstilosTipoDanoSO : ScriptableObject
{
    [SerializeField] private EntradaEstiloTipoDano[] entradas;
    [SerializeField] private EntradaEstiloTipoDano fallback = new EntradaEstiloTipoDano
    {
        tipo = TipoDanoCombate.Fisico,
        corTexto = Color.white,
        tamanhoFonteBase = 56f,
        tamanhoFonteCritico = 72f,
        sufixoSeCritico = "!"
    };

    public EntradaEstiloTipoDano Resolver(TipoDanoCombate tipo)
    {
        if (entradas != null)
        {
            for (int i = 0; i < entradas.Length; i++)
            {
                if (entradas[i] != null && entradas[i].tipo == tipo)
                    return entradas[i];
            }
        }

        return fallback;
    }
}
