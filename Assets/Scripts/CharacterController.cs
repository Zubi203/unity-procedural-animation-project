using UnityEngine;

public class SpiderController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 100f;

    [SerializeField] private float cameraSensitivity = 5f;
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private Transform spiderBody;




    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        
        float rotation = horizontalInput * rotateSpeed * Time.deltaTime;
        transform.Rotate(0f, rotation, 0f);

        if (spiderBody != null)
        {
            spiderBody.Rotate(0f, rotation, 0f);
        }

        transform.position += transform.forward * verticalInput * moveSpeed * Time.deltaTime;


    }

    void LateUpdate()
    {
        if (cameraAnchor == null) return;

        cameraAnchor.position = spiderBody.position;

        float x = Input.GetAxis("Mouse X");
        cameraAnchor.transform.eulerAngles += Vector3.up * x * cameraSensitivity;
    }
}
