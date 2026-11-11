using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ponto central para torres ativas e futura fábrica/pool de projéteis.
/// </summary>
public class TorresManager : MonoBehaviour
{
    public static TorresManager Instancia { get; private set; }

    /// <summary>Disparado ao construir/registrar uma torre.</summary>
    public static System.Action OnTorreConstruida;
    /// <summary>Disparado ao destruir/desregistrar uma torre.</summary>
    public static System.Action OnTorreDestruida;

    private readonly List<TorreController> _torres = new List<TorreController>();

    public IReadOnlyList<TorreController> TorresRegistadas => _torres;

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

    public void Registar(TorreController torre)
    {
        if (torre != null && !_torres.Contains(torre))
        {
            _torres.Add(torre);
            OnTorreConstruida?.Invoke();
        }
    }

    public void Desregistar(TorreController torre)
    {
        if (torre != null && _torres.Remove(torre))
            OnTorreDestruida?.Invoke();
    }

    /// <summary>Ponto de extensão para construção via UI: instancia o prefab e regista quando o <see cref="TorreController"/> fizer Start.</summary>
    public TorreController InstanciarTorre(GameObject prefabTorre, Vector3 posicao, Quaternion rotacao)
    {
        if (prefabTorre == null)
            return null;

        GameObject instancia = Instantiate(prefabTorre, posicao, rotacao);
        instancia.transform.SetParent(transform, true);
        return instancia.GetComponent<TorreController>();
    }
}
