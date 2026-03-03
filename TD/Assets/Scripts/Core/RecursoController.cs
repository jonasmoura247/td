using UnityEngine;

public abstract class RecursoController : MonoBehaviour
{
    [Header("Configurações do Recurso")]
    public int quantidadeMaxima = 10;
    protected int quantidadeAtual;

    protected virtual void Start()
    {
        quantidadeAtual = quantidadeMaxima;
    }

    public virtual int Coletar(int quantidade)
    {
        int coletado = Mathf.Min(quantidade, quantidadeAtual);
        quantidadeAtual -= coletado;

        if (quantidadeAtual <= 0)
        {
            Esgotar();
        }

        return coletado;
    }

    protected virtual void Esgotar()
    {
        Destroy(gameObject);
    }

    public int GetQuantidadeAtual() => quantidadeAtual;
}
