using System;
using UnityEngine;

/// <summary>
/// Vida e facção da torre (receber dano de inimigos no futuro).
/// </summary>
public class TorreBaseCore
{
    private int _vidaMaxima;
    private int _vidaAtual;
    private bool _viva = true;

    public bool EstaViva => _viva;
    public int VidaAtual => _vidaAtual;
    public int VidaMaxima => _vidaMaxima;

    public event Action<TorreBaseCore> AoMorrer;

    public void Inicializar(int vidaMaxima)
    {
        _vidaMaxima = Mathf.Max(1, vidaMaxima);
        _vidaAtual = _vidaMaxima;
        _viva = true;
    }

    public void DefinirVidaMaxima(int novaMax, bool curarAteMaximo = false)
    {
        _vidaMaxima = Mathf.Max(1, novaMax);
        if (curarAteMaximo)
            _vidaAtual = _vidaMaxima;
        else
            _vidaAtual = Mathf.Min(_vidaAtual, _vidaMaxima);
    }

    public void ReceberDano(int quantidade)
    {
        if (!_viva || quantidade <= 0)
            return;

        _vidaAtual -= quantidade;
        if (_vidaAtual <= 0)
            Morrer();
    }

    public void Curar(int quantidade)
    {
        if (!_viva || quantidade <= 0)
            return;
        _vidaAtual = Mathf.Min(_vidaAtual + quantidade, _vidaMaxima);
    }

    private void Morrer()
    {
        if (!_viva)
            return;
        _viva = false;
        AoMorrer?.Invoke(this);
    }
}
