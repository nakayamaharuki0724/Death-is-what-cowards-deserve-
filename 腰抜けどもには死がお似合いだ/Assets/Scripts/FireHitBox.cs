using UnityEngine;

using UnityEngine;

public class FireHitBox : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("‰Šƒ_ƒ[ƒW");
        }
    }
}