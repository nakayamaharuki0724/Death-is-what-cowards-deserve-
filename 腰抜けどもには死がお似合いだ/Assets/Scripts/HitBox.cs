using UnityEngine;

public class HitBox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (gameObject.name.Contains("Claw"))
        {
            Debug.Log("爪攻撃ヒット");
        }
        else if (gameObject.name.Contains("Basic"))
        {
            Debug.Log("噛みつきヒット");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (gameObject.name.Contains("Fire"))
        {
            Debug.Log("炎ダメージ");
        }
    }
}