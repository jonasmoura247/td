using System;
using System.Collections.Generic;
using UnityEngine;

public enum TipoStatTorre
{
    Dano,
    Alcance,
    IntervaloAtaque,
    VidaMaxima
}

[Serializable]
public struct ModificadorTorreEntrada
{
    public TipoStatTorre tipo;
    public float aditivo;
    public float multiplicador;
}

/// <summary>
/// Base extensível para buffs/debuffs permanentes ou temporários (lista manipulável no futuro).
/// </summary>
public class TorreSistemaModificadoresCore
{
    private readonly List<ModificadorTorreEntrada> _modificadores = new List<ModificadorTorreEntrada>();

    public IReadOnlyList<ModificadorTorreEntrada> Modificadores => _modificadores;

    public void Limpar() => _modificadores.Clear();

    public void Adicionar(ModificadorTorreEntrada m) => _modificadores.Add(m);

    public bool RemoverPrimeiro(Predicate<ModificadorTorreEntrada> match) => _modificadores.RemoveAll(match) > 0;

    public float SomarAditivos(TipoStatTorre tipo)
    {
        float s = 0f;
        for (int i = 0; i < _modificadores.Count; i++)
        {
            if (_modificadores[i].tipo == tipo)
                s += _modificadores[i].aditivo;
        }
        return s;
    }

    public float ProdutoMultiplicadores(TipoStatTorre tipo)
    {
        float p = 1f;
        for (int i = 0; i < _modificadores.Count; i++)
        {
            if (_modificadores[i].tipo == tipo)
            {
                float m = _modificadores[i].multiplicador;
                if (m > 0f)
                    p *= m;
            }
        }
        return p;
    }
}
