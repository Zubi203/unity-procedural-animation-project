using UnityEngine;

[CreateAssetMenu(menuName = "LDToolkit/Character Stats")]
public class CharacterStats : ScriptableObject
{
    [Header("Movement Speeds")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2.5f;
    public float climbSpeed = 5f;
    
    [Header("Jump")]
    public float jumpHeight = 1.5f;
    public float gravity = -20f;
    public float airControl = 0.2f;

    [Header("Crouch")]
    public float standHeight = 1.8f;
    public float crouchHeight = 1.0f;
    public float crouchTransitionSpeed = 8f;
    
    [Header("Slope")]
    public float maxSlopeAngle = 45f;
    public float slideGravity = 30f;
    
    [Header("Mantle")]
    public float mantleMinHeight = 1.2f;
    public float mantleMaxHeight = 2.1f;
    public float mantleForwardCheck = 0.7f;
    public float mantleTopCheck = 1.2f;
    public float mantleMoveSpeed = 6f;

}