using UnityEngine;

/// <summary>
/// Checkpoint trigger that updates the player's respawn position.
/// Attach to an invisible GameObject with a Collider set to "Is Trigger".
/// </summary>
[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null)
            return;

        player.SpawnPosition = transform.position;
        Debug.Log($"Checkpoint activated: respawn set to {transform.position}");
    }
}
