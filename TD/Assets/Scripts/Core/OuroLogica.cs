using System;

/// <summary>
/// Lógica pura de gerenciamento de ouro — sem MonoBehaviour, sem Unity runtime.
/// OuroManager delega para esta classe, permitindo testes unitários isolados.
/// </summary>
public class OuroLogica
{
    private int _ouroAtual;

    public event Action<int> OnOuroAlterado;

    public OuroLogica(int ouroInicial = 0)
    {
        _ouroAtual = Math.Max(0, ouroInicial);
    }

    public int GetOuro() => _ouroAtual;

    /// <summary>Adiciona ouro. Retorna a quantidade efetivamente adicionada (0 se inválido).</summary>
    public int AdicionarOuro(int quantidade)
    {
        if (quantidade <= 0) return 0;
        _ouroAtual += quantidade;
        OnOuroAlterado?.Invoke(_ouroAtual);
        return quantidade;
    }

    /// <summary>Gasta ouro se houver saldo suficiente. Retorna true em caso de sucesso.</summary>
    public bool GastarOuro(int quantidade)
    {
        if (quantidade <= 0 || _ouroAtual < quantidade)
            return false;
        _ouroAtual -= quantidade;
        OnOuroAlterado?.Invoke(_ouroAtual);
        return true;
    }

    /// <summary>Define o ouro diretamente (ex.: ao carregar save). Nunca fica negativo.</summary>
    public void SetOuro(int valor)
    {
        _ouroAtual = Math.Max(0, valor);
        OnOuroAlterado?.Invoke(_ouroAtual);
    }
}
