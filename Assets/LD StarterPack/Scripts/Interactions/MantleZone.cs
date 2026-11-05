using UnityEngine;

public class MantleZone : MonoBehaviour
{
    [Tooltip("Куда поставить персонажа после залаза")]
    public Transform standPoint;

    [Tooltip("Минимальная высота для залаза")]
    public float minHeight = 1.2f;

    [Tooltip("Максимальная высота для залаза")]
    public float maxHeight = 2.1f;
    
    [Tooltip("Максимальный угол перед стенкой для залаза")]
    public float maxAngle = 45f;

    public bool CanMantle(Transform player)
    {
        float h = standPoint.position.y - player.position.y;
        float angle = Vector3.Angle(standPoint.forward, player.forward);
        return h >= minHeight && h <= maxHeight && angle <= maxAngle;
    }
}