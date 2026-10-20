using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    public Image fill;
    public Color fullColor = new Color(0.18f, 0.77f, 0.71f);
    public Color lowColor = new Color(1f, 0.62f, 0.11f);

    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void Update()
    {
        slider.value = PlayerMovement.stamina;
        if (fill != null) fill.color = Color.Lerp(lowColor, fullColor, PlayerMovement.stamina / PlayerMovement.maxStamina);
    }
}
