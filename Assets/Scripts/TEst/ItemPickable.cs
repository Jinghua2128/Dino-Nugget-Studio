// ItemPickable.cs
// Represents an item that can be pushed down by distractor and picked up by the player.
using UnityEngine;

public class ItemPickable : MonoBehaviour
{
    public bool isPushed = false;
    public float pickupPoints = 5f;
    public AudioClip pushSound;
    public AudioClip pickupSound;

    Rigidbody rb;
    Collider col;
    AudioSource audioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        audioSource = gameObject.AddComponent<AudioSource>();
        if (rb) rb.isKinematic = true; // by default on shelf
    }

    public void PushDown()
    {
        if (isPushed) return;
        isPushed = true;
        // make physics active and let it fall
        if (rb)
        {
            rb.isKinematic = false;
            rb.AddForce(Vector3.forward * Random.Range(0.5f, 1.8f) + Vector3.up * 1f, ForceMode.Impulse);
        }
        if (pushSound) audioSource.PlayOneShot(pushSound);
        // optionally change layer/tag
        gameObject.tag = "DroppedItem";
    }

    public void OnPicked(PlayerInteractor player)
    {
        if (!isPushed) return; // cannot pick items that weren't pushed (optional)
        // award points & destroy
        GameManager.Instance.AddPoints(pickupPoints);
        if (pickupSound) audioSource.PlayOneShot(pickupSound);
        Destroy(gameObject);
    }
}
