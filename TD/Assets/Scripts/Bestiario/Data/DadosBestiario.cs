using System;
using System.Collections.Generic;

[Serializable]
public class BestiarioCriaturaProgresso
{
    public string criaturaId;
    public int mortes;
    /// <summary>Quantos marcos de estágio já deram pontos (0..N).</summary>
    public int estagiosRecompensados;
}

[Serializable]
public class BestiarioRunaEquipada
{
    public string runaId;
    public string criaturaId;
}

[Serializable]
public class DadosBestiario
{
    public int pontosBestiario;
    public List<BestiarioCriaturaProgresso> progressoPorCriatura = new List<BestiarioCriaturaProgresso>();
    public List<BestiarioRunaEquipada> runasEquipadas = new List<BestiarioRunaEquipada>();
}
