using UnityEngine;

public class HumanController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 100f;
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private float cameraSensitivity = 2f;
    [SerializeField] private Transform model;
    [SerializeField] private Animator anim;

    Rigidbody rb;
    Vector3 facingDir;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Move();
    }

    void LateUpdate()
    {
        if (cameraAnchor == null) return;

        float x = Input.GetAxis("Mouse X");
        cameraAnchor.transform.eulerAngles += Vector3.up * x * cameraSensitivity;
    }

    void Move()
    {
        if (rb == null) return;

        float dirX = Input.GetAxis("Horizontal");
        float dirZ = Input.GetAxis("Vertical");

        Vector3 dir = (cameraAnchor.right * dirX + cameraAnchor.forward * dirZ).normalized;

        if (dir.magnitude > 0.0)
        {
            facingDir = dir;
        }

        Vector3 moveVector = dir * moveSpeed;

        rb.linearVelocity = new Vector3(moveVector.x, rb.linearVelocity.y, moveVector.z);

        if (model == null) return;

        Quaternion targetRotation = Quaternion.LookRotation(facingDir);
        model.rotation = Quaternion.RotateTowards(model.rotation, targetRotation, Time.deltaTime * 1000f);

        if (anim == null) return;

        bool moving = (dir.magnitude > 0.0) ? true : false;
        anim.SetBool("IsMoving", moving);
    }
}
