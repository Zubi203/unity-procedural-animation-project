using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 100f;


    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        
        Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles + Vector3.up * horizontalInput);
        
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);

        transform.position += -transform.up * verticalInput * moveSpeed * Time.deltaTime;


    }
}
