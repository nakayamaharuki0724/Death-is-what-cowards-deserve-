using UnityEngine;
using UnityEngine.UI;

public class PlayerHPUI : MonoBehaviour
{
    public Player player;

    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
        slider.maxValue = player.GetMaxHP();
    }

    void Update()
    {
        slider.value = player.GetCurrentHP();
    }
}