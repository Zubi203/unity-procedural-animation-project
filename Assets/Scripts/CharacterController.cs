using UnityEngine;

public class SpiderController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 100f;

    [SerializeField] private float cameraSensitivity = 5f;
    [SerializeField] private Transform cameraAnchor;




    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        
        Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles + Vector3.up * horizontalInput);
        
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);

        transform.position += transform.forward * verticalInput * moveSpeed * Time.deltaTime;


    }

    void LateUpdate()
    {
        if (cameraAnchor == null) return;

        cameraAnchor.position = transform.position;

        float x = Input.GetAxis("Mouse X");
        cameraAnchor.transform.eulerAngles += Vector3.up * x * cameraSensitivity;
    }
}
