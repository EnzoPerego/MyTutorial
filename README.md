# Corrida dos Triângulos

Jogo 2D de arena contra o relógio: colete os 7 triângulos antes que o tempo acabe, fugindo de um inimigo que persegue você pelo mapa.

**Jogar no navegador:** [LINK_DO_ITCH](https://erperego.itch.io/mytutorial-jogo001)

## Como jogar

- **Objetivo:** coletar os **7 triângulos** antes que o cronômetro chegue a zero.
- Você começa com **10 segundos**.
- Cada triângulo coletado dá **+1 segundo**.
- Se o inimigo vermelho encostar em você, você perde **5 segundos** e fica **invencível por 2 segundos**, piscando.
- Segure **Shift** para correr **1,8× mais rápido**. A barra de stamina esvazia em cerca de 3 segundos de corrida e recarrega quando você para de correr. Se ela zerar, você só volta a correr depois de recuperar 20% da barra.
- **Vitória:** coletar os 7 triângulos. A tela final mostra quanto tempo sobrou.
- **Derrota:** o cronômetro chegar a zero.

## Controles

| Ação | Teclado |
|---|---|---|
| Mover | WASD ou setas |
| Correr | Shift esquerdo |
| Confirmar nos menus | Enter ou clique do mouse |

## Funcionalidades implementadas

- **Mecânica de tempo**
  - Cronômetro regressivo no formato MM:SS.
  - Tempo restante exibido ao final ("Você venceu! Sobraram MM:SS" ou "Tempo esgotado!").
  - O tempo é a mecânica central: +1 s por triângulo, −5 s por impacto do inimigo, derrota quando zera.
- **Inimigo perseguidor com impacto:** o inimigo segue o jogador o tempo todo. O contato tira 5 s e ativa 2 s de invencibilidade com o personagem piscando.
- **UI:** cronômetro, contador de triângulos ("Triângulos: x/7"), barra de stamina e painel de fim de jogo com os botões "Reiniciar" e "Menu Inicial".
- **Áudio:** música de fundo em loop, som de moeda ao coletar um triângulo e som de lâmina ao ser atingido. A música para quando a partida termina.
- **Suporte a joystick:** movimento pelo analógico esquerdo, corrida no botão X e confirmação com A nos menus.

## Como rodar o projeto

1. Instale o **Unity 6000.3.24f1** pelo Unity Hub.
2. Clone o repositório:
   ```bash
   git clone https://github.com/EnzoPerego/MyTutorial.git
   ```
3. No Unity Hub, clique em **Add → Add project from disk** e selecione a pasta clonada.
4. Abra a cena `Assets/Scenes/MainMenu.unity` e aperte **Play**.

## Estrutura do projeto

```
Assets/
├── Scenes/
│   ├── MainMenu.unity      menu inicial
│   └── SampleScene.unity   fase do jogo
├── Settings/               configurações do URP e do Input System
├── Player.prefab           jogador
├── Triangle.prefab         triângulo coletável
├── *.cs                    scripts (abaixo)
└── *.mp3, *.ttf            áudios e fonte
```

| Script | O que faz |
|---|---|
| `GameController.cs` | Guarda o estado da partida: triângulos coletados, vitória e derrota. |
| `GameTimer.cs` | Cronômetro regressivo; soma ou subtrai segundos do tempo restante. |
| `PlayerMovement.cs` | Movimento, corrida com stamina, coleta de triângulos e dano do inimigo com invencibilidade. |
| `EnemyChaser.cs` | Faz o inimigo seguir o jogador. |
| `UIManager.cs` | Esconde o HUD e mostra o painel de fim de jogo com o tempo restante. |
| `ScoreUI.cs` | Atualiza o contador de triângulos. |
| `StaminaBar.cs` | Atualiza a barra de stamina. |
| `AudioManager.cs` | Toca o som de impacto e para a música no fim da partida. |
| `MenuAction.cs` | Ações dos botões: iniciar ou reiniciar a partida e voltar ao menu inicial. |

## Créditos

- Música "Bit Bound Run" (`bitbound-run.mp3`): gerada com ElevenLabs.
- Efeito sonoro de lâmina (`ElevenLabs_A_cheerful_voice_ringing_out__'Look_at_my_blade!'_followed_by_a_distinct_ringing_blade_sound.mp3`): gerado com ElevenLabs.
- Som de moeda (`chieuk-coin-257878.mp3`).
- Fonte AldotheApache (`AldotheApache.ttf`).

---

Projeto individual da disciplina Jogos Digitais (Insper, 2025.2), feito por Enzo Rios Perego.
