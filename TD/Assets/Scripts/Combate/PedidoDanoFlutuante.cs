using System;
using UnityEngine;

/// <summary>Dados para exibir dano flutuante (valor, tipo, crítico, overrides opcionais).</summary>
[Serializable]
public struct PedidoDanoFlutuante
{
    public int ValorExibicao;
    public TipoDanoCombate Tipo;
    public bool Critico;
    [Tooltip("Se true, usa CorOverride em vez da biblioteca (exceto se o mapa forçar cor).")]
    public bool UsarCorOverride;
    public Color CorOverride;

    public static PedidoDanoFlutuante Criar(int valor, TipoDanoCombate tipo, bool critico = false)
    {
        return new PedidoDanoFlutuante
        {
            ValorExibicao = valor,
            Tipo = tipo,
            Critico = critico,
            UsarCorOverride = false,
            CorOverride = Color.white
        };
    }

    public static PedidoDanoFlutuante CriarComCorOverride(int valor, TipoDanoCombate tipo, Color cor, bool critico = false)
    {
        return new PedidoDanoFlutuante
        {
            ValorExibicao = valor,
            Tipo = tipo,
            Critico = critico,
            UsarCorOverride = true,
            CorOverride = cor
        };
    }
}
