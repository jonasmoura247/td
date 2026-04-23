using UnityEngine;

/// <summary>
/// Faz o mob voar diretamente ao castelo ignorando waypoints.
/// Requer MovimentoUnidade — ativa modo livre no Start e persegue o castelo pelo SistemaIAUnidade.
/// </summary>
[RequireComponent(typeof(MovimentoUnidade))]
public class ComportamentoVoador : MonoBehaviour
{
    private MovimentoUnidade _movimento;

    private void Awake()
    {
        _movimento = GetComponent<MovimentoUnidade>();
    }

    private void Start()
    {
        _movimento.EntrarModoLivre();
    }
}
