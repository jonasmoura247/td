using UnityEngine;

public static class UnidadeIdentidadeResolver
{
    public static void ObterOuInferir(UnidadeController u, out TipoRaca raca, out UnidadeSubtipo sub)
    {
        var id = u.GetComponent<UnidadeIdentidade>();
        if (id != null)
        {
            raca = id.raca;
            sub = id.subtipo;
            return;
        }

        raca = TipoRaca.Goblin;
        sub = UnidadeSubtipo.Indefinido;

        switch (u)
        {
            case GoblinGuerreiro:
                raca = TipoRaca.Goblin;
                sub = UnidadeSubtipo.Guerreiro;
                return;
            case GoblinLanceiro:
                raca = TipoRaca.Goblin;
                sub = UnidadeSubtipo.Lanceiro;
                return;
            case BossGoblinNivel3:
                raca = TipoRaca.Goblin;
                sub = UnidadeSubtipo.Boss;
                return;
            case GoblinController:
                raca = TipoRaca.Goblin;
                return;
            case ElfoAssassino:
                raca = TipoRaca.Elfo;
                sub = UnidadeSubtipo.Assassino;
                return;
            case ElfoMago:
                raca = TipoRaca.Elfo;
                sub = UnidadeSubtipo.Mago;
                return;
            case ElfoController:
                raca = TipoRaca.Elfo;
                return;
            case HumanoArqueiroController:
                raca = TipoRaca.Humano;
                sub = UnidadeSubtipo.Arqueiro;
                return;
            case HumanoFeiticeiraController:
                raca = TipoRaca.Humano;
                sub = UnidadeSubtipo.Feiticeira;
                return;
            case HumanoMineiroController:
                raca = TipoRaca.Humano;
                sub = UnidadeSubtipo.Mineiro;
                return;
            case GoblinMineiroController:
                raca = TipoRaca.Goblin;
                sub = UnidadeSubtipo.Mineiro;
                return;
            case ElfoMineiroController:
                raca = TipoRaca.Elfo;
                sub = UnidadeSubtipo.Mineiro;
                return;
            case AnaoMineiroController:
                raca = TipoRaca.Anao;
                sub = UnidadeSubtipo.Mineiro;
                return;
            case HumanoGuerreiro:
                raca = TipoRaca.Humano;
                sub = UnidadeSubtipo.Guerreiro;
                return;
            case HumanoController:
                raca = TipoRaca.Humano;
                return;
            case AnaoGuerreiro:
                raca = TipoRaca.Anao;
                sub = UnidadeSubtipo.Guerreiro;
                return;
            case AnoesController:
                raca = TipoRaca.Anao;
                return;
        }
    }
}
