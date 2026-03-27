using UnityEngine;

/// <summary>
/// Coloque um único objeto com este script na cena do mapa/partida.
/// Awake marca que estamos em partida; OnDestroy (ao sair da cena) libera edição no salão.
/// </summary>
public class MarcadorEstadoPartidaReliquias : MonoBehaviour
{
    private void Awake()
    {
        EstadoPartidaReliquias.EntrarNaPartida();
    }

    private void OnDestroy()
    {
        EstadoPartidaReliquias.SairDaPartida();
    }
}
