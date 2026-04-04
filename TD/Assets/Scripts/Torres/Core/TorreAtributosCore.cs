using UnityEngine;

/// <summary>
/// Combina dados base (ScriptableObject) com modificadores para expor stats efetivos.
/// </summary>
public class TorreAtributosCore
{
    private TorreDadosSO _dados;
    private TorreSistemaModificadoresCore _mods;

    public void Configurar(TorreDadosSO dados, TorreSistemaModificadoresCore modificadores)
    {
        _dados = dados;
        _mods = modificadores;
    }

    public TorreDadosSO Dados => _dados;

    public int ObterDanoEfetivo()
    {
        if (_dados == null) return 0;
        float v = _dados.danoBase * _mods.ProdutoMultiplicadores(TipoStatTorre.Dano) + _mods.SomarAditivos(TipoStatTorre.Dano);
        return Mathf.Max(0, Mathf.RoundToInt(v));
    }

    public float ObterAlcanceEfetivo()
    {
        if (_dados == null) return 0f;
        float v = _dados.alcanceAtaque * _mods.ProdutoMultiplicadores(TipoStatTorre.Alcance) + _mods.SomarAditivos(TipoStatTorre.Alcance);
        return Mathf.Max(0f, v);
    }

    public float ObterIntervaloEfetivo()
    {
        if (_dados == null) return 1f;
        float v = _dados.intervaloEntreAtaques * _mods.ProdutoMultiplicadores(TipoStatTorre.IntervaloAtaque) + _mods.SomarAditivos(TipoStatTorre.IntervaloAtaque);
        return Mathf.Max(0.05f, v);
    }

    public int ObterVidaMaximaEfetiva()
    {
        if (_dados == null) return 1;
        float v = _dados.vidaMaxima * _mods.ProdutoMultiplicadores(TipoStatTorre.VidaMaxima) + _mods.SomarAditivos(TipoStatTorre.VidaMaxima);
        return Mathf.Max(1, Mathf.RoundToInt(v));
    }

    public float ObterVelocidadeProjetil() => _dados != null ? Mathf.Max(0.1f, _dados.velocidadeProjetil) : 8f;

    public GameObject ObterPrefabProjetil() => _dados != null ? _dados.prefabProjetil : null;
}
