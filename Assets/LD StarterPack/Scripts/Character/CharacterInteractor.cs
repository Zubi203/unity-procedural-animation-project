using UnityEngine;

public class CharacterInteractor : MonoBehaviour
{
    public float interactDistance = 2f;
    public LayerMask interactMask;
    public KeyCode interactKey = KeyCode.E;

    public bool TryInteract()
    {
        if (!Input.GetKeyDown(interactKey))
            return false;

        if (Physics.Raycast(transform.position, transform.forward, out var hit, interactDistance, interactMask))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
                return true;
            }
        }

        return false;
    }

    public bool TryEnterLadder(out Ladder ladder)
    {
        ladder = null;

        if (Physics.Raycast(transform.position, transform.forward, out var hit, 0.5f))
        {
            ladder = hit.collider.GetComponent<Ladder>();
            return ladder != null;
        }

        return false;
    }

    
}