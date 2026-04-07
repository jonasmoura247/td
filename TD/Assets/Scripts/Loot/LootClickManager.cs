using UnityEngine;
using UnityEngine.InputSystem;

public class LootClickManager : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            DetectarClique();
        }
    }

    void DetectarClique()
    {
        if (ServicoColocacaoTorre.Instancia != null && ServicoColocacaoTorre.Instancia.ModoColocacaoAtivo)
            return;

        if (ServicoColocacaoUnidade.Instancia != null && ServicoColocacaoUnidade.Instancia.ModoColocacaoAtivo)
            return;

        if (Camera.main == null) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        float distZ = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldPos3D = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distZ));
        Vector2 worldPos = new Vector2(worldPos3D.x, worldPos3D.y);

        Collider2D col = Physics2D.OverlapPoint(worldPos);
        if (col == null) return;

        LootColetavel loot = col.GetComponent<LootColetavel>();
        if (loot != null)
            loot.Coletar();
    }
}