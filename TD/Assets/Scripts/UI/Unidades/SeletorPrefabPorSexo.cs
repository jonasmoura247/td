using UnityEngine;

/// <summary>
/// Utilitario para escolher prefab masculino/feminino e montar nome da instancia.
/// </summary>
public static class SeletorPrefabPorSexo
{
    public static GameObject Selecionar(GameObject prefabMasculino, GameObject prefabFeminino, out bool femininoSelecionado)
    {
        if (prefabMasculino == null)
        {
            femininoSelecionado = prefabFeminino != null;
            return prefabFeminino;
        }

        if (prefabFeminino == null)
        {
            femininoSelecionado = false;
            return prefabMasculino;
        }

        femininoSelecionado = UnityEngine.Random.value >= 0.5f;
        return femininoSelecionado ? prefabFeminino : prefabMasculino;
    }

    public static string MontarNomeInstancia(string nomeBase, bool femininoSelecionado)
    {
        if (!femininoSelecionado)
            return nomeBase;

        if (string.IsNullOrEmpty(nomeBase))
            return "UnidadeFeminina";

        if (nomeBase.Contains("Feminina"))
            return nomeBase;

        return nomeBase + "Feminina";
    }
}
