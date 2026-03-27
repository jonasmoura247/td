using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Marca um dos 5 slots de equipamento no salão (apenas dados; o drop é resolvido pelo drag).
/// Coloque no mesmo objeto que tem o Image de fundo do slot (ou no pai direto), com Raycast Target ligado.
/// </summary>
public class ReliquiaSlotDropSalao : MonoBehaviour
{
    [SerializeField] private int indiceSlot;

    public int IndiceSlot => indiceSlot;

    public void DefinirIndice(int indice)
    {
        indiceSlot = indice;
    }

    private void Awake()
    {
        var imgs = GetComponentsInChildren<Image>(true);
        for (int i = 0; i < imgs.Length; i++)
        {
            if (imgs[i] != null)
                imgs[i].raycastTarget = true;
        }
    }
}
