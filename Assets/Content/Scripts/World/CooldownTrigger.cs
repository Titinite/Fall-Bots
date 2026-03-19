using UnityEngine;
using System.Collections;

public class CooldownTrigger : MonoBehaviour
{
    [Header("Objet à désactiver")]
    public GameObject targetObject;

    [Header("Durée du cooldown (secondes)")]
    public float cooldown = 10f;

    private bool isOnCooldown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isOnCooldown) return;
        StartCoroutine(DisableTemporarily());
    }

    private IEnumerator DisableTemporarily()
    {
        isOnCooldown = true;

        if (targetObject != null)
            targetObject.SetActive(false);

        yield return new WaitForSeconds(cooldown);

        if (targetObject != null)
            targetObject.SetActive(true);

        isOnCooldown = false;
    }
}