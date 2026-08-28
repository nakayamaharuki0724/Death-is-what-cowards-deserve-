using UnityEngine;

public class PlayerAttackHitBox : MonoBehaviour
{
    public int damage = 10;

    private void OnTriggerEnter(Collider other)
    {
        Boss boss = other.GetComponentInParent<Boss>();

        if (boss == null) return;

        Debug.Log("プレイヤーの攻撃がドラゴンにヒット！");

        boss.TakeDamage(damage);
    }
}