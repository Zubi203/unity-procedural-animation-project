using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    
    public DoorController[] doors;
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            foreach (DoorController door in doors)
            {
                if (door)
                    door.Open();
            }
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            foreach (DoorController door in doors)
            {
                if (door)
                    door.Close();
            }
        }
    }
}

