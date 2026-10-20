# Fuga na Baía

Jogo 2D de sobrevivência com tema de piratas e surf: o relógio é a sua vida. Pegue tesouros para ganhar tempo e fuja dos piratas pelo maior tempo possível.

**Jogar no navegador:** [erperego.itch.io/mytutorial-jogo001](https://erperego.itch.io/mytutorial-jogo001)

## Como jogar

- O relógio começa em **30 s** e desce o tempo todo. Quando zera, a partida acaba.
- A pontuação é o **tempo sobrevivido**. O melhor tempo fica salvo como recorde.
- O jogo é dividido em **ondas**. Cada onda espalha baús, moedas, ampulhetas e boias pela baía. Pegue **todos os baús** da onda para começar a próxima e ganhar **+5 s**.
- A cada onda aparecem mais baús e mais navios piratas, e os perseguidores ficam mais rápidos.
- A seta amarela em volta do surfista aponta para o baú mais próximo.
- **Remada forte:** segure Shift para ir 1,8× mais rápido enquanto houver stamina. Se a barra zerar, você só volta a remar forte depois de recuperar 20%.

| Item | Efeito |
|---|---|
| Baú | +4 s |
| Moeda | +1 s |
| Ampulheta | +5 s |
| Boia | Escudo que absorve o próximo golpe |

| Perigo | Efeito |
|---|---|
| Navio pirata (vela preta) | Persegue você contornando as ilhas. Encostar tira **5 s**. |
| Navio canhoneiro (vela vermelha) | Ancorado. Atira quando você chega perto; o canhão pisca vermelho antes do tiro. A bala tira **3 s**. |
| Torres do forte (Ilha da Caveira) | Atiram por cima da ilha. A bala tira **3 s**. |
| Tubarão | Patrulha o recife. A mordida tira **3 s**. |

Depois de um golpe, o surfista fica 2 s invencível, piscando.

## Controles

| Ação | Teclado | Controle |
|---|---|---|
| Mover | WASD ou setas | Analógico esquerdo |
| Remada forte | Shift esquerdo | X |
| Pausar | ESC ou P | Start |
| Confirmar nos menus | Enter ou clique do mouse | A |

## Funcionalidades implementadas

- **Mecânica de tempo**
  - Relógio de vida regressivo no formato MM:SS, que pisca em vermelho abaixo de 10 s.
  - O tempo é a mecânica central: itens somam tempo, golpes tiram e completar uma onda dá bônus.
  - Tempo sobrevivido no HUD e na tela final, com recorde salvo entre partidas.
- **Inimigos com impacto**
  - Navio perseguidor que contorna ilhas usando um campo de fluxo (BFS) sobre o mapa.
  - Navios canhoneiros e torres que disparam com aviso visual, e tubarões que patrulham rotas fixas.
- **Ondas com dificuldade crescente:** mais baús, mais piratas e piratas mais rápidos.
- **UI:** relógio, onda, baús da onda, tempo sobrevivido, recorde, barra de stamina, bússola, textos "+4s"/"-5s", aviso de onda, pausa, tela final e menu com "Como jogar".
- **Áudio:** música em loop e efeitos para moeda, baú, ampulheta, boia, golpe, canhão, explosão, respingo, nova onda, fim de jogo e cliques.
- **Suporte a controle:** movimento, remada forte, pausa e navegação nos menus.
- **Sem dead-end ou softlock:** a partida sempre termina, porque o relógio só desce. A pausa e a tela final têm Reiniciar e Menu. Testes automáticos garantem que toda a água do mapa é alcançável.

## Como rodar o projeto

1. Instale o **Unity 6000.3.24f1** pelo Unity Hub.
2. Clone o repositório:
   ```bash
   git clone https://github.com/EnzoPerego/MyTutorial.git
   ```
3. No Unity Hub, clique em **Add → Add project from disk** e selecione a pasta clonada.
4. Abra a cena `Assets/Scenes/MainMenu.unity` e aperte **Play**.

### Mudar o mapa

As cenas e os prefabs são gerados por `Assets/Editor/BayBuilder.cs` a partir do arquivo de texto `Assets/Level/baia.txt`. Para mudar o mapa, edite o texto e use o menu **Fuga na Baía → Gerar tudo** no Unity. Ajustes feitos à mão nas cenas são sobrescritos quando você gera de novo.

| Caractere | Significado |
|---|---|
| `.` | Água |
| `#` | Ilha (retangular; com 4×4 ou mais ganha grama no centro) |
| `p` | Palmeira (sobre uma ilha) |
| `w` | Muralha do forte (sobre uma ilha) |
| `T` | Torre de canhão (sobre uma ilha) |
| `r` | Pedra na água |
| `C` | Navio canhoneiro |
| `S` | Início do surfista |
| `P` | Entrada de navio pirata |
| `1`–`9` | Pontas de uma rota de tubarão (cada dígito aparece 2 vezes) |

### Testes e build

- **Testes:** *Window → General → Test Runner*, nas abas EditMode e PlayMode. Os testes verificam regras, navegação, se todo o mapa é alcançável, as ondas, os perigos, a pausa e o fim de jogo.
- **Build WebGL:** menu **Fuga na Baía → Build WebGL**. A saída vai para `WebBuild/FugaNaBaia`, sem compressão, para rodar direto no itch.io. O jogo é feito para **600×480** e usa o modelo de página `Assets/WebGLTemplates/FugaNaBaia`, que mantém a proporção 5:4 e mostra uma barra de carregamento. No itch, configure a janela (viewport) com 600×480.

## Estrutura do projeto

```
Assets/
├── Art/          Kenney (tiles, navios, efeitos), Custom (sprites próprios), Fonts, Tiles
├── Audio/        Music e Sfx
├── Editor/       BayBuilder.cs (gera tiles, fontes, prefabs, cenas e o build)
├── Level/        baia.txt (layout do mapa)
├── Prefabs/      gerados pelo BayBuilder
├── Scenes/       MainMenu.unity e Baia.unity
├── Scripts/      scripts do jogo (assembly FugaNaBaia)
└── Tests/        EditMode e PlayMode
Tools/            gen_art.py e gen_sfx.py (geram os sprites e sons próprios)
docs/             documento de design
```

| Script | O que faz |
|---|---|
| `GameController.cs` | Estado da partida: onda atual, baús da onda e fim de jogo. |
| `GameRules.cs` | Todos os números do jogo (tempos, tabela das ondas). |
| `GameTimer.cs` | Relógio de vida, tempo sobrevivido e formatação MM:SS. |
| `WaveSpawner.cs` | Começa as ondas, espalha itens e coloca/acelera os piratas. |
| `LevelLayout.cs` | Lê o mapa em texto: ilhas, pedras, rotas e pontos especiais. |
| `NavGrid.cs` | Grade de navegação e campo de fluxo (BFS) dos piratas. |
| `GameMap.cs` | Mantém o layout e o campo de fluxo atualizados durante o jogo. |
| `PlayerMovement.cs` | Surfista: movimento, remada forte, golpes, escudo e invencibilidade. |
| `EnemyChaser.cs` | Navio pirata que persegue o surfista contornando ilhas. |
| `CannonShooter.cs` | Canhão de navio ou torre: mira, aviso e tiro. |
| `Cannonball.cs` | Bala de canhão: explode em obstáculos ou cai na água. |
| `SharkPatrol.cs` | Tubarão que vai e volta entre dois pontos. |
| `Hazard.cs` | Marca o que tira tempo do surfista ao encostar. |
| `Pickup.cs` | Baú, moeda, ampulheta e boia. |
| `Compass.cs` | Seta que aponta para o baú mais próximo. |
| `CameraFollow.cs` | Câmera que segue o surfista, presa ao mapa, com tremida nos golpes. |
| `UIManager.cs` | Tela de fim de jogo com tempo, onda e recorde. |
| `ScoreUI.cs` | Onda, baús e recorde no HUD. |
| `StaminaBar.cs` | Barra de stamina que muda de cor. |
| `WaveBanner.cs` | Aviso "Onda N!" no começo de cada onda. |
| `TimePopups.cs` / `FloatingText.cs` | Textos "+4s"/"-5s" que sobem e somem. |
| `PauseMenu.cs` | Pausa com ESC, P ou Start. |
| `BestScore.cs` | Salva e lê o recorde. |
| `MenuAction.cs` | Botões dos menus: jogar, reiniciar, voltar ao menu, "Como jogar". |
| `AudioManager.cs` | Música e efeitos sonoros. |
| `WakeEmitter.cs` | Rastro de espuma atrás de quem se move na água. |
| `OceanDecor.cs` | Anima as ondas decorativas. |
| `SpriteFlipbook.cs` | Efeitos curtos (explosão, respingo, fumaça). |
| `MenuCameraPan.cs` | Passeio da câmera pela baía no menu. |

## Créditos

- **Arte:** [Kenney Pirate Pack](https://kenney.nl/assets/pirate-pack) (CC0): ilhas, navios, forte, pedras, palmeiras, balas e explosões.
- **Sprites próprios** (surfista, tubarão, baú, moeda, ampulheta, boia, bússola e interface): desenhados por código para o projeto (`Tools/gen_art.py`).
- **Fontes:** Pirata One (Rodrigo Fuenzalida e Nicolas Massi) e Lilita One (Juan Montoreano), ambas sob a SIL Open Font License.
- **Música** "Bit Bound Run" (`bitbound-run.mp3`): gerada com ElevenLabs.
- **Efeito sonoro de lâmina** (`elevenlabs-blade.mp3`): gerado com ElevenLabs.
- **Som de moeda** (`chieuk-coin-257878.mp3`).
- **Demais efeitos sonoros:** sintetizados para o projeto (`Tools/gen_sfx.py`).

---

Projeto individual da disciplina Jogos Digitais (Insper, 2026.2), feito por Enzo Rios Perego.
