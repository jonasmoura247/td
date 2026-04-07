using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ponto central para unidades aliadas ativas (espelha <see cref="TorresManager"/>).
/// </summary>
public class UnidadesManager : MonoBehaviour
{
    public static UnidadesManager Instancia { get; private set; }

    private readonly List<UnidadeController> _unidades = new List<UnidadeController>();

    public IReadOnlyList<UnidadeController> UnidadesRegistadas => _unidades;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
    }

    private void OnDestroy()
    {
        if (Instancia == this)
            Instancia = null;
    }

    public void Registar(UnidadeController unidade)
    {
        if (unidade != null && !_unidades.Contains(unidade))
            _unidades.Add(unidade);
    }

    public void Desregistar(UnidadeController unidade)
    {
        if (unidade != null)
            _unidades.Remove(unidade);
    }

    /// <summary>Instancia unidade aliada a partir da UI (colocação no mapa).</summary>
    public UnidadeController InstanciarUnidade(GameObject prefabUnidade, Vector3 posicao, Quaternion rotacao, string nomeInstancia)
    {
        if (prefabUnidade == null)
            return null;

        GameObject instancia = Instantiate(prefabUnidade, posicao, rotacao);
        instancia.transform.SetParent(transform, true);

        if (!string.IsNullOrEmpty(nomeInstancia))
            instancia.name = nomeInstancia;

        return instancia.GetComponent<UnidadeController>();
    }
}
