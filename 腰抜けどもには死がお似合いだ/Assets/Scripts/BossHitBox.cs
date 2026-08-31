using UnityEngine;

public class BossHitBox : MonoBehaviour
{
    public Boss boss;

    public void TakeDamage(int damage)
    {
        if (boss == null)
            return;

        boss.TakeDamage(damage);
    }
}