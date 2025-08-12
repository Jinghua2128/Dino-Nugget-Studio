using UnityEngine;

public class TriggerDoor : MonoBehaviour
{
    private Animator entranceDoorAnimator;
    private int playerCount = 0; // Tracks how many players are inside

    void Start()
    {
        entranceDoorAnimator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerCount++;

            if (playerCount == 1) // First player entering
            {
                entranceDoorAnimator.SetTrigger("Open");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerCount--;

            if (playerCount <= 0) // No players left
            {
                playerCount = 0;
                entranceDoorAnimator.SetTrigger("Close");
            }
        }
    }
}
