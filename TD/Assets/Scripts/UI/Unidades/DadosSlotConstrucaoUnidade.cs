using UnityEngine;

/// <summary>
/// Definicao de slot de unidade de combate (HUD): prefab, custo e raio do preview de alcance.
/// </summary>
[System.Serializable]
public class DadosSlotConstrucaoUnidade
{
    [Tooltip("Prefab da unidade (arqueiro, feiticeiro, etc.).")]
    public GameObject prefabUnidade;

    [Tooltip("Prefab opcional da variante feminina da unidade. Se vazio, usa apenas o prefab principal.")]
    public GameObject prefabUnidadeFeminina;

    [Tooltip("Cobrado ao confirmar a colocacao no mapa.")]
    public int custoOuro = 100;

    [Tooltip("Nome da instancia na hierarquia (opcional).")]
    public string nomeInstancia = "";

    [Tooltip("Raio do circulo de alcance no preview (unidades de mundo).")]
    public float alcanceIndicadorPreview = 5f;

    public GameObject ObterPrefabAleatorio()
    {
        return ObterPrefabAleatorio(out _);
    }

    public GameObject ObterPrefabAleatorio(out bool feminino)
    {
        return SeletorPrefabPorSexo.Selecionar(prefabUnidade, prefabUnidadeFeminina, out feminino);
    }

    public string ObterNomeInstancia(GameObject prefabSelecionado, bool feminino)
    {
        string baseNome = string.IsNullOrEmpty(nomeInstancia)
            ? (prefabSelecionado != null ? prefabSelecionado.name : string.Empty)
            : nomeInstancia;
        return SeletorPrefabPorSexo.MontarNomeInstancia(baseNome, feminino);
    }
}
