using UnityEngine;

public class GoalZone2D : MonoBehaviour
{
    public HUD hud;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Goal trigger entered by: " + other.name);

        BallPlayerController2D player = other.GetComponent<BallPlayerController2D>();
        if (player == null)
        {
            player = other.GetComponentInParent<BallPlayerController2D>();
        }

        if (player == null)
        {
            Debug.Log("No BallPlayerController2D found on entering object.");
            return;
        }

        if (hud == null)
        {
            Debug.LogWarning("GoalZone2D has no HUD assigned.");
            return;
        }

        Debug.Log("Player reached goal.");
        hud.SetWinState(true);
    }
}