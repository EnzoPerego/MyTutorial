using UnityEngine;

// Componentes no mesmo objeto de um Hazard podem reagir quando ele acerta o surfista.
public interface IHazardListener
{
    void OnHazardHit(PlayerMovement player);
}

// Qualquer coisa que tira tempo ao encostar no surfista: navio pirata, bala de canhão, tubarão.
public class Hazard : MonoBehaviour
{
    public float timePenalty = 3f;

    public void NotifyHit(PlayerMovement player)
    {
        foreach (IHazardListener listener in GetComponents<IHazardListener>())
            listener.OnHazardHit(player);
    }
}
