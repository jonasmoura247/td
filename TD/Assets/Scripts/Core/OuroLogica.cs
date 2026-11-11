using System;

/// <summary>
/// Lógica pura de gerenciamento de ouro — sem MonoBehaviour, sem Unity runtime.
/// OuroManager delega para esta classe, permitindo testes unitários isolados.
/// </summary>
public class OuroLogica
{
    private int _ouroAtual;

    public event Action<int> OnOuroAlterado;
    /// <summary>Disparado ao adicionar ouro (parametro: delta adicionado, total atual).</summary>
    public event Action<int, int> OnOuroAdicionado;
    /// <summary>Disparado ao gastar ouro (parametro: delta gasto).</summary>
    public event Action<int> OnOuroGasto;

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
        OnOuroAdicionado?.Invoke(quantidade, _ouroAtual);
        OnOuroAlterado?.Invoke(_ouroAtual);
        return quantidade;
    }

    /// <summary>Gasta ouro se houver saldo suficiente. Retorna true em caso de sucesso.</summary>
    public bool GastarOuro(int quantidade)
    {
        if (quantidade <= 0 || _ouroAtual < quantidade)
            return false;
        _ouroAtual -= quantidade;
        OnOuroGasto?.Invoke(quantidade);
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
