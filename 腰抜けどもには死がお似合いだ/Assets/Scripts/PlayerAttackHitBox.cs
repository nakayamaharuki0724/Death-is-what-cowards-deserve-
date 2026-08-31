using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public int damage = 30;

    public AudioSource audioSource;
    public AudioClip hitSE;

    private void OnTriggerEnter(Collider other)
    {
        BossHitBox bossHitBox = other.GetComponent<BossHitBox>();

        if (bossHitBox == null)
            return;

        bossHitBox.TakeDamage(damage);

        if (audioSource != null && hitSE != null)
        {
            audioSource.PlayOneShot(hitSE);
        }

        Debug.Log("ÉhÉâÉSÉìÇ…çUåÇÅI");
    }
}