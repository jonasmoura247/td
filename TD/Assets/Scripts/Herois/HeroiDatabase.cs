using UnityEngine;

/// <summary>
/// Banco de dados de todos os heróis disponíveis no jogo.
/// Referenciado pelo HeroisManager para spawn e lookup por ID.
/// </summary>
[CreateAssetMenu(fileName = "HeroiDatabase", menuName = "TD/Herois/Heroi Database")]
public class HeroiDatabase : ScriptableObject
{
    public HeroiDef[] entradas;

    public HeroiDef ObterPorId(string idHeroi)
    {
        if (string.IsNullOrEmpty(idHeroi) || entradas == null)
            return null;
        foreach (var def in entradas)
            if (def != null && def.idHeroi == idHeroi)
                return def;
        return null;
    }
}
