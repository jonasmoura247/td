using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD da partida: até 5 ícones conforme slots equipados; somente leitura (sem drag na partida).
/// </summary>
public class ReliquiaHudPartida : MonoBehaviour
{
    [SerializeField] private ReliquiaDatabase database;
    [SerializeField] private Image[] slotsEsquerda = new Image[5];
    [SerializeField] private Image destaqueDireita;

    private void Start()
    {
        AtualizarHud();
    }

    public void AtualizarHud()
    {
        if (database == null)
            return;

        for (int i = 0; i < slotsEsquerda.Length; i++)
        {
            if (slotsEsquerda[i] == null)
                continue;
            slotsEsquerda[i].sprite = null;
            slotsEsquerda[i].enabled = false;
        }

        if (destaqueDireita != null)
        {
            destaqueDireita.sprite = null;
            destaqueDireita.enabled = false;
        }

        if (ReliquiasManager.Instance == null || !PerfilJogadorAtivo.TemPerfilAtivo)
            return;

        for (int i = 0; i < slotsEsquerda.Length && i < 5; i++)
        {
            string id = ReliquiasManager.Instance.GetReliquiaNoSlot(i);
            if (slotsEsquerda[i] == null || string.IsNullOrEmpty(id))
                continue;
            ReliquiaDef def = database.ObterPorId(id);
            if (def != null && def.icone != null)
            {
                slotsEsquerda[i].sprite = def.icone;
                slotsEsquerda[i].enabled = true;
            }
        }

        if (destaqueDireita != null)
        {
            string primeiro = ReliquiasManager.Instance.GetReliquiaNoSlot(0);
            if (!string.IsNullOrEmpty(primeiro))
            {
                ReliquiaDef d = database.ObterPorId(primeiro);
                if (d != null && d.icone != null)
                {
                    destaqueDireita.sprite = d.icone;
                    destaqueDireita.enabled = true;
                }
            }
        }
    }
}
