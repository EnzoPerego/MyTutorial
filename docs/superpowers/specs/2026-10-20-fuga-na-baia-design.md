# Fuga na Baía — design

Data: 2026-10-20 · Autor: Enzo Rios Perego · Status: aprovado

Reformulação do projeto "Corrida dos Triângulos" em um jogo de sobrevivência com tema de piratas, mar e surf.

## 1. Regras

**Objetivo:** sobreviver o máximo possível. O relógio de vida começa em 30 s e desce continuamente; itens somam tempo, golpes tiram. Quando o relógio zera, a partida acaba.

**Pontuação:** tempo total sobrevivido. O melhor tempo é salvo (PlayerPrefs) e aparece no menu e na tela final.

**Ondas:**
- Cada onda espalha baús, moedas, ampulhetas e boias em células de água alcançáveis, longe do surfista.
- A onda termina quando todos os baús dela são coletados. Moedas, ampulhetas e boias que sobrarem continuam no mapa.
- Onda completa: +5 s e aviso "Onda N!".

| Onda n | Valor |
|---|---|
| Baús | min(2 + n, 8) |
| Moedas | min(4 + 2n, 16) |
| Ampulhetas | 1 (2 a partir da onda 4) |
| Boia | 1 nas ondas pares |
| Perseguidores | 1 (ondas 1–2), 2 (3–4), 3 (5+) |
| Velocidade dos perseguidores | × min(1 + 0,05·(n−1), 1,5) |

**Tempo:**
| Evento | Tempo |
|---|---|
| Início | 30 s |
| Baú | +4 s |
| Moeda | +1 s |
| Ampulheta | +5 s |
| Onda completa | +5 s |
| Encostar em navio pirata | −5 s |
| Bala de canhão | −3 s |
| Tubarão | −3 s |

Após um golpe: 2 s de invencibilidade piscando. A boia é um escudo que absorve o próximo golpe.

**Surfista:** 8 direções, gira na direção do movimento, rastro de espuma. Remada forte (sprint) com stamina: 1,8× de velocidade, gasta 35/s, recupera 20/s, ao zerar só volta com 20.

**Inimigos:**
- Navio pirata perseguidor: vai direto ao surfista quando tem linha de visão; senão segue um campo de fluxo (BFS no grid) que contorna ilhas. Após encostar, recua parado por 1 s.
- Navio canhoneiro ancorado: alcance 7, uma bala a cada 2 s com aviso visual antes do tiro. Ilhas e pedras bloqueiam a mira e as balas.
- Torres do forte (Ilha da Caveira): mesmo comportamento, intervalo menor.
- Tubarões: vão e voltam entre dois pontos fixos.

**Interface:** relógio de vida, onda, baús x/y, tempo sobrevivido, barra de stamina, bússola (seta em volta do surfista apontando para o baú mais próximo), textos flutuantes (+4s/−5s), aviso de onda, pausa (ESC/P/Start: Continuar, Reiniciar, Menu) e tela final com tempo sobrevivido, onda alcançada e recorde.

**Controles:** WASD/setas ou analógico esquerdo; Shift esquerdo ou X para remada forte; ESC/P ou Start para pausar; Enter/A ou mouse nos menus.

## 2. Mapa e visual

- Grid de 48 × 30 tiles (1 tile = 1 unidade = 64 px), definido em `Assets/Level/baia.txt`.
- Câmera ortográfica (tamanho 6) seguindo o surfista com suavização, presa às bordas do mapa.
- Áreas: Praia do Início (sul), Mar Aberto (oeste, canhoneiros), Recife dos Tubarões (leste, pedras e tubarões), Ilha da Caveira (norte, forte com 2 torres), ilhotas com palmeiras.
- Arte: Kenney Pirate Pack (CC0) para ilhas, palmeiras, pedras, navios, forte, balas e explosões. Desenhados no mesmo estilo: surfista, tubarão, baú, moeda, ampulheta, boia, ondas decorativas, espuma, painéis e botões.
- Fontes (OFL): Pirata One no título e avisos, Lilita One na interface. A AldotheApache não tem letras acentuadas e não é usada.
- Áudio: mantém música, moeda e golpe; efeitos novos gerados por código (canhão, explosão, power-up, baú, boia quebrando, nova onda, fim de jogo).

### Legenda do layout

| Caractere | Significado |
|---|---|
| `.` | água |
| `#` | ilha (retângulos; ≥ 4×4 ganham grama no centro) |
| `r` | pedra na água (obstáculo) |
| `p` | palmeira (sobre ilha) |
| `S` | início do surfista |
| `P` | ponto de entrada de perseguidor |
| `C` | navio canhoneiro |
| `T` | torre de canhão (sobre ilha) |
| `1`–`4` | extremos de rota de tubarão (cada dígito aparece 2 vezes) |

## 3. Código

- Scripts de jogo em `Assets/Scripts` (assembly `FugaNaBaia`); os 9 scripts antigos são movidos para lá com `git mv` e adaptados.
- Novos: `LevelLayout`, `NavGrid`, `GameRules`, `GameMap`, `WaveSpawner`, `Pickup`, `Hazard`, `CannonShooter`, `Cannonball`, `SharkPatrol`, `Compass`, `CameraFollow`, `PauseMenu`, `BestScore`, `FloatingText`, `WaveBanner`, `WakeEmitter`, `SpriteFlipbook`, `MenuCameraPan`, `OceanDecor`.
- `Assets/Editor/BayBuilder.cs` gera tiles, fontes TMP, prefabs, as cenas `MainMenu` e `Baia` e o build WebGL. Roda pelo menu "Fuga na Baía" ou em batchmode.
- Saem: `SampleScene`, `Triangle.prefab` e o `Player.prefab` antigo (substituídos pelos gerados).

## 4. Testes

- EditMode: parser do layout, campo de fluxo contorna ilhas, toda célula de água é alcançável a partir do início (sem softlock), rotas de tubarão sobre água, fórmulas das ondas, recorde salva e carrega.
- PlayMode (fumaça): carrega `Baia`, simula alguns segundos, confere ausência de erros, onda 1 com o número certo de baús, perseguidor se aproximando, e salva screenshot.
- Build WebGL em batchmode servido localmente. Equilíbrio e sensação de jogo validados pelo autor jogando.
