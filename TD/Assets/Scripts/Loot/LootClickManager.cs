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
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider == null)
            return;

        LootColetavel loot = hit.collider.GetComponent<LootColetavel>();

        if (loot != null)
        {
            loot.Coletar();
        }
    }
}