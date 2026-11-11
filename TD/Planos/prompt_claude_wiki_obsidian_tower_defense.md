# Prompt para Claude — Gerar Wiki Obsidian do Projeto Tower Defense

```text
Quero que você atue como um Arquiteto de Software + Documentador Técnico especializado em projetos de jogos Unity/C#.

Sua missão é analisar todo o meu projeto de jogo e gerar uma documentação completa em Markdown (.md) para ser usada no Obsidian.

IMPORTANTE:
Os arquivos .md devem ser criados e organizados neste caminho local:

C:\Users\Jonas\Desktop\TowerDefense\TowerDefense

====================================================
OBJETIVO PRINCIPAL
====================================================

Ler todas as classes, scripts, sistemas e estruturas do projeto.

Entender a função real de cada arquivo.

Criar uma Wiki completa do projeto em Markdown, com links internos compatíveis com Obsidian.

A documentação deve me permitir visualizar toda a arquitetura do jogo de forma clara.

====================================================
ESTRUTURA DE PASTAS NO OBSIDIAN
====================================================

Crie e organize os arquivos .md em pastas como:

/00 - Índice Geral
/01 - Sistemas
/02 - Unidades
/03 - Raças
/04 - Construções
/05 - UI
/06 - Managers
/07 - Economia
/08 - IA
/09 - Gameplay
/10 - Balanceamento
/11 - Bugs e Melhorias
/12 - Roadmap Técnico

====================================================
REGRAS DE ANÁLISE
====================================================

Leia todos os arquivos .cs do projeto.

Identifique:

- Classes principais
- Managers
- Heranças
- Interfaces
- Dependências entre scripts
- Scripts não utilizados
- Código duplicado
- Sistemas incompletos
- Possíveis bugs
- Scripts pesados
- Acoplamento excessivo
- Oportunidades de refatoração

====================================================
PARA CADA SCRIPT ENCONTRADO, CRIE UM .MD
====================================================

Formato:

# NomeDaClasse

## Função
Explique claramente o que essa classe faz no jogo.

## Categoria
Exemplo:
Sistema / Unidade / UI / IA / Manager / Economia

## Responsabilidades

- Item 1
- Item 2
- Item 3

## Variáveis Importantes

- nomeVariavel → função dela

## Métodos Principais

- Start()
- Update()
- SpawnEnemy()
- AttackTarget()

Explique o que cada método faz.

## Dependências

Liste links internos:

[[WaveManager]]
[[EnemyBase]]
[[GoldSystem]]

## Fluxo no Projeto

Explique onde essa classe entra no funcionamento geral do jogo.

## Problemas Encontrados

Liste possíveis erros, riscos ou melhorias.

## Sugestões Técnicas

Sugira melhorias profissionais.

====================================================
LINKS INTERNOS OBRIGATÓRIOS
====================================================

Sempre que citar outra classe, usar:

[[NomeDaClasse]]

====================================================
CRIAR ÍNDICE PRINCIPAL
====================================================

Criar arquivo:

Wiki.md

Com:

# Tower Defense - Wiki Geral

## Sistemas
(lista com links)

## Unidades

## Raças

## Managers

## Gameplay

## Economia

## UI

## Roadmap

====================================================
ANÁLISE PROFUNDA DO PROJETO
====================================================

Além da documentação, quero relatórios extras:

1. Scripts órfãos (não usados)
2. Código duplicado
3. Classes grandes demais
4. Update() desnecessário
5. Possíveis gargalos de performance
6. Sistemas faltando
7. Melhorias para escalar o projeto
8. Sugestões AAA para arquitetura

====================================================
SE O PROJETO FOR TOWER DEFENSE
====================================================

Detecte automaticamente sistemas como:

- Waves
- Spawn
- IA inimiga
- Waypoints
- Torres
- Heróis
- Economia
- Upgrade
- Bosses
- Raças
- Relíquias
- Save system
- Progressão

E documente separadamente.

====================================================
ESTILO DE ESCRITA
====================================================

Profissional.
Claro.
Objetivo.
Bem organizado.
Pensando como documentação interna de estúdio de games.

====================================================
IMPORTANTE
====================================================

Não resuma superficialmente.

Quero análise real de código.

Se encontrar algo relevante, crie novas páginas .md automaticamente.

Se faltar sistema importante, avise.

Se houver arquitetura ruim, fale sem medo.

====================================================
RESULTADO FINAL
====================================================

Quero transformar meu projeto em uma Wiki navegável no Obsidian, totalmente profissional.
```