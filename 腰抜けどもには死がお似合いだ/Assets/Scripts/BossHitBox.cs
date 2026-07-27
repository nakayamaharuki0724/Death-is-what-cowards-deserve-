using UnityEngine;

public class BossHitBox : MonoBehaviour
{
    public Boss boss;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PlayerAttack"))
            return;

        boss.TakeDamage(10);
    }
}