using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VanishPlatforme : MonoBehaviour
{
    private static readonly List<VanishPlatforme> _instances = new List<VanishPlatforme>();

    [Tooltip("Delai avant disparition en secondes")]
    [SerializeField] private float _delay = 0.5f;

    private void OnEnable()
    {
        _instances.Add(this);
    }

    private void OnDisable()
    {
        _instances.Remove(this);
    }

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
    }

    public static void ResetAll()
    {
        for (int i = _instances.Count - 1; i >= 0; i--)
            _instances[i].gameObject.SetActive(true);
    }
}
