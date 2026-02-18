using System.Collections;
using UnityEngine;

public class VanishPlatforme : MonoBehaviour
{
    [Tooltip("Delai avant disparition en secondes")]
    [SerializeField] private float _delay = 0.5f;

    [Tooltip("Delai avant reapparition en secondes (0 = ne reapparait pas)")]
    [SerializeField] private float _respawnDelay = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>() == null)
            return;

        StartCoroutine(VanishSequence());
    }

    private IEnumerator VanishSequence()
    {
        yield return new WaitForSeconds(_delay);

        gameObject.SetActive(false);

        if (_respawnDelay > 0)
        {
            yield return new WaitForSeconds(_respawnDelay);
            gameObject.SetActive(true);
        }
    }
}
