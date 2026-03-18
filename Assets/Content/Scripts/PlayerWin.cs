using UnityEngine;
using System.Collections;

public class PlayerWin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null || player.State.IsWinner)
            return;

        player.State.IsWinner = true;
        player.State.HorizontalVelocity = Vector2.zero;
        player.State.VerticalVelocity = 0f;
        StartCoroutine(WinSequence(player));
    }

    private IEnumerator WinSequence(Player player)
    {
        Animator animator = player.GetComponentInChildren<Animator>();
        if (animator)
            animator.SetTrigger("trigger_win");

        yield return new WaitForSeconds(2f);
        
        player.State.IsWinner = false;
        player.Respawn();
    }
}
