using UnityEngine;
using UnityEngine.UI;

public class ReliquiaSlotDisponivelSalao : MonoBehaviour
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
