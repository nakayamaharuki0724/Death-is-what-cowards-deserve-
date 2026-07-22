using UnityEngine;

public class HitBox : MonoBehaviour
{
    public int biteDamage = 20;
    public int clawDamage = 15;
    public int fireDamage = 5;

    public float fireDamageInterval = 0.5f; // 0.5ïbÇ≤Ç∆

    private float fireTimer = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Player player = other.GetComponent<Player>();
        if (player == null) return;

        if (gameObject.name.Contains("Claw"))
        {
            Debug.Log("í‹çUåÇÉqÉbÉg");
            player.TakeDamage(clawDamage);
        }
        else if (gameObject.name.Contains("Basic"))
        {
            Debug.Log("äöÇ›Ç¬Ç´ÉqÉbÉg");
            player.TakeDamage(biteDamage);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!gameObject.name.Contains("Fire")) return;

        fireTimer += Time.deltaTime;

        if (fireTimer >= fireDamageInterval)
        {
            fireTimer = 0f;

            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                Debug.Log("âäÉ_ÉÅÅ[ÉW");
                player.TakeDamage(fireDamage);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            fireTimer = 0f;
        }
    }
}