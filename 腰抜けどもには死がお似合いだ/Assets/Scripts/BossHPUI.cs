using UnityEngine;
using UnityEngine.UI;

public class BossHPUI : MonoBehaviour
{
    public Boss boss;

    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
        slider.maxValue = boss.GetMaxHP();
    }

    void Update()
    {
        slider.value = boss.GetCurrentHP();
    }
}