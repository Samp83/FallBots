using System.Collections;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null || player.State.IsDead)
            return;

        player.State.IsDead = true;
        player.State.HorizontalVelocity = Vector2.zero;
        player.State.VerticalVelocity = 0f;
        StartCoroutine(DeathSequence(player));
    }

    private IEnumerator DeathSequence(Player player)
    {
        Animator animator = player.GetComponentInChildren<Animator>();
        if (animator)
            animator.SetTrigger("trigger_die");

        yield return new WaitForSeconds(2f);

        player.Respawn();
    }
}
