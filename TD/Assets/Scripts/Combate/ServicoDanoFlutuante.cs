using UnityEngine;

/// <summary>Ponto único para spawn de <see cref="TextoDanoFlutuante"/> no mundo.</summary>
public static class ServicoDanoFlutuante
{
    private const float DeslocamentoVerticalPadrao = 0.6f;

    public static void Mostrar(Vector3 posicaoMundo, PedidoDanoFlutuante pedido)
    {
        if (pedido.ValorExibicao <= 0)
            return;

        Vector3 pos = posicaoMundo + Vector3.up * DeslocamentoVerticalPadrao;
        TextoDanoFlutuante flutuante = null;

        var cfg = ConfiguracaoTextoDanoFlutuanteMapa.Instancia;
        if (cfg != null && cfg.PrefabTextoFlutuante != null)
        {
            GameObject inst = Object.Instantiate(cfg.PrefabTextoFlutuante, pos, Quaternion.identity);
            flutuante = inst.GetComponent<TextoDanoFlutuante>();
            if (flutuante == null)
                Object.Destroy(inst);
        }

        if (flutuante == null)
            flutuante = TextoDanoFlutuante.Criar(pos);

        BibliotecaEstilosTipoDanoSO biblioteca = cfg != null ? cfg.BibliotecaEstilosTipoDano : null;
        flutuante.Inicializar(pedido, biblioteca);
    }
}
