// CasteloController.cs - Versão atualizada
using UnityEngine;

public class CasteloController : MonoBehaviour
{
    [Header("Configurações de Dano")]
    public int danoPorColisao = 1;
    public float forcaRecuo = 8f; // Aumentei um pouco
    public float tempoEntreDanos = 1f;

    // Referência para o collider principal (para debug)
    private Collider2D casteloCollider;

    void Start()
    {
        casteloCollider = GetComponent<Collider2D>();

        Debug.Log("=== DIAGNÓSTICO DO CASTELO ===");
        Debug.Log($"Nome: {gameObject.name}");
        Debug.Log($"Layer: {LayerMask.LayerToName(gameObject.layer)} (índice: {gameObject.layer})");
        Debug.Log($"Tag: {gameObject.tag}");

        // Verifica Rigidbody
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Debug.Log($"Rigidbody2D: {(rb != null ? rb.bodyType.ToString() : "NÃO TEM")}");

        // Verifica Collider
        Debug.Log($"Collider: {(casteloCollider != null ? "OK" : "NÃO TEM")}");
        if (casteloCollider != null)
        {
            Debug.Log($"  - Tipo: {casteloCollider.GetType()}");
            Debug.Log($"  - Is Trigger: {casteloCollider.isTrigger}");
            Debug.Log($"  - Enabled: {casteloCollider.enabled}");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<GoblinController>(out GoblinController goblin))
        {
            Debug.Log("Colisão física detectada!");
            AplicarDanoERecuo(goblin, collision.contacts[0].normal);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<GoblinController>(out GoblinController goblin))
        {
            if (Time.time >= goblin.GetProximoTempoDano())
            {
                AplicarDanoERecuo(goblin, collision.contacts[0].normal);
            }
        }
    }

    private void AplicarDanoERecuo(GoblinController goblin, Vector2 normalColisao)
    {
        goblin.ReceberDano(danoPorColisao);

        // Usa a normal da colisão para direção do recuo
        Vector2 direcaoRecuo = normalColisao;

        goblin.AplicarRecuo(direcaoRecuo, forcaRecuo);
        Debug.Log($"Goblin tomou dano! Vida restante: {goblin.GetVidaAtual()}");
    }
}