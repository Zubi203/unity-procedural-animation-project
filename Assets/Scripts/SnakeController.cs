using UnityEngine;

public class SnakeController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 100f;

    [SerializeField] private float cameraSensitivity = 5f;
    [SerializeField] private Transform cameraAnchor;

    [SerializeField] private ProceduralSnake snakeAnim;

    void FixedUpdate()
    {
        if (snakeAnim == null) return;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        
        float rotation = horizontalInput * rotateSpeed * Time.deltaTime;
        transform.Rotate(0f, rotation, 0f);

        transform.position += transform.forward * verticalInput * moveSpeed * Time.deltaTime;

        if (Mathf.Abs(verticalInput) > 0.2f)
        {
            snakeAnim.Slither(true);
        }
        else
        {
            snakeAnim.Slither(false);
        }

    }

    void LateUpdate()
    {
        if (cameraAnchor == null) return;

        cameraAnchor.position = transform.position;

        float x = Input.GetAxis("Mouse X");
        cameraAnchor.transform.eulerAngles += Vector3.up * x * cameraSensitivity;
    }
}
