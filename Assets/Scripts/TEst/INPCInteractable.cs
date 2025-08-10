// INPCInteractable.cs
// Simple interface so other objects can be interactable (NPCs, items, etc.)
using UnityEngine;

public interface INPCInteractable
{
    string GetName();
    // Called when the player interacts (e.g., open UI)
    void OnInteract(PlayerInteractor interactor);
    // Called when interaction ends
    void OnLeave(PlayerInteractor interactor);
}
