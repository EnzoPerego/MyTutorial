using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuAction : MonoBehaviour
{
  public void IniciarJogo()
  {
    GameController.Init();
    SceneManager.LoadScene(1);
  }

  public void MenuPrincipal()
  {
    SceneManager.LoadScene(0);
  }
}
