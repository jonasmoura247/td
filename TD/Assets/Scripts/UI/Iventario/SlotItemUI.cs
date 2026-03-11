using UnityEngine;
using UnityEngine.UI;

public class SlotItemUI : MonoBehaviour
{
    public Image iconeItem;

    private void Awake()
    {
        if (iconeItem == null)
        {
            Transform icone = transform.Find("IconeItem");
            if (icone != null)
                iconeItem = icone.GetComponent<Image>();

            if (iconeItem == null)
            {
                Image[] images = GetComponentsInChildren<Image>(true);
                foreach (Image img in images)
                {
                    if (img.gameObject != gameObject)
                    {
                        iconeItem = img;
                        break;
                    }
                }
            }
        }

        if (iconeItem != null)
            iconeItem.enabled = false;
    }

    public void Configurar(Sprite spriteItem)
    {
        if (iconeItem == null)
        {
            Debug.LogWarning("[SlotItemUI] IconeItem nao encontrado. Verifique o prefab SlotItem.");
            return;
        }

        if (spriteItem == null)
        {
            iconeItem.enabled = false;
            return;
        }

        iconeItem.enabled = true;
        iconeItem.sprite = spriteItem;
        iconeItem.color = Color.white;
        iconeItem.preserveAspect = true;
    }
}