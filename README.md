# Tower Defense

Jogo de defesa de torres em **Unity 6** (C#), com quatro raças, heróis, talentos, relíquias, bestiário e progressão salva em slots. Feito em dupla (Jonas e Miguel), com Git, pull requests e uma divisão de tarefas documentada.

## O que tem

- **Quatro raças** com lobby próprio: anões, elfos, goblins e humanos.
- **Mapas e fases:** cinco mapas de mundo (natural, gelo, magma, deserto e floresta sombria) e fases com bandeiras e identificação de fase.
- **Combate:** torres, unidades, mobs e castelo, com ondas de inimigos, ouro, resistência e regeneração, e sistema de modificadores de unidade.
- **Heróis, talentos e relíquias:** gerados na tenda a cada partida, com salão de relíquias por raça.
- **Bestiário** dos inimigos e **sistema de loot**.
- **Progressão e slots de save:** nível por raça, experiência da partida, estatísticas por raça no lobby e seleção de slots.
- **Aleatoriedade determinística:** os eventos da partida usam um gerador com seed fixa, o que torna as partidas reproduzíveis e testáveis.

## Como abrir

1. Instale o **Unity Hub** e o editor **6000.3.8f1** (a versão fica em `TD/ProjectSettings/ProjectVersion.txt`).
2. No Hub, adicione a pasta **`TD/`** deste repositório e abra o projeto.
3. As cenas do jogo ficam em `TD/Assets/Scenes/` (seleção de slots, lobbies, salão de relíquias e mapas).

## Estrutura

```
TD/Assets/
  Scripts/      código do jogo, por sistema: Core, Combate, Torres, Unidades, Mobs, Racas,
                Herois, Talentos, Reliquias, Tendas, Bestiario, Loot, Progressao, SaveSlots, UI...
  Scenes/       menus, lobbies por raça, salões de relíquia e mapas
  Tests/
    EditMode/   testes de lógica pura (NUnit)
    PlayMode/   testes de integração com cenas e componentes
  Editor/       ferramentas de editor
TD/Docs/        guias e planejamento (sistema de torres, HUD por raça, divisão de trabalho)
TD/Planos/      planos de desenvolvimento por etapa
```

## Testes

No Unity, abra **Window → General → Test Runner** e rode as abas **EditMode** e **PlayMode**. A lógica do jogo (ouro, ondas, experiência, save, estatísticas, sorteios) fica em classes puras para ser testada sem cena; o PlayMode cobre combate, movimento, talentos e outros sistemas com componentes.

## Equipe

Desenvolvimento em dupla, dividido por sistemas conforme `TD/Docs/DivisaoTrabalho_TD.html`, com revisão por pull request na `main`.
