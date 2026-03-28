using System;
using System.Collections.Generic;

[System.Serializable]
public class TalentoGrupoEscolhaSave
{
    public string grupoId;
    public string noIdEscolhido;
}

[System.Serializable]
public class DadosTalentos
{
    public int pontosDisponiveis;
    public List<string> nosDesbloqueados = new List<string>();
    public List<TalentoGrupoEscolhaSave> escolhasGrupo = new List<TalentoGrupoEscolhaSave>();
}
