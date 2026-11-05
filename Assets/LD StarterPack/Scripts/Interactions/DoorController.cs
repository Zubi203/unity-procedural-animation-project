using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    
   private Transform rotatingLeaf;
   
   [SerializeField] private DoorState state = DoorState.Close;
   [SerializeField] private float duration = 1.0f;
   [Range(-180, 180)] public float openAngle = 90.0f;
   
   private Coroutine rotateCoroutine;

   private void Awake()
   {
       AssignLeaf();
       
   }

   public void Toggle()
   {
       var currentAngle = GetCurrentAngle();
       if (GetDoorState(currentAngle) == DoorState.Close)
           Open();
       else if (GetDoorState(currentAngle) == DoorState.Open)
           Close();
   }

   public void Open()
   {
       var currentAngle = GetCurrentAngle();

       if (GetDoorState(currentAngle) == DoorState.Open)
           return;

       if (rotateCoroutine != null)
           StopCoroutine(rotateCoroutine);
       
       rotateCoroutine = StartCoroutine(Rotate(currentAngle, openAngle));
   }

   public void Close()
   {
       var currentAngle = GetCurrentAngle();

       if (GetDoorState(currentAngle) == DoorState.Close)
           return;

       if (rotateCoroutine != null)
           StopCoroutine(rotateCoroutine);
       
       rotateCoroutine = StartCoroutine(Rotate(currentAngle, 0));
   }

   private void OnValidate()
   {
       AssignLeaf();

       switch (state)
       {
           case DoorState.Open:
               rotatingLeaf.transform.rotation = Quaternion.Euler(0, openAngle, 0);
               break;
           case DoorState.Close:
               rotatingLeaf.transform.rotation = Quaternion.identity;
               break;
       }
   }

   private IEnumerator Rotate(float start, float end)
   {
       for (float i = 0; i < 1; i += Time.deltaTime / duration)
       {
           rotatingLeaf.transform.rotation = Quaternion.Lerp(
               Quaternion.Euler(0, start, 0),
               Quaternion.Euler(0, end, 0),
               i);

           yield return null;
       }

       rotatingLeaf.transform.rotation = Quaternion.Euler(0, end, 0);
       rotateCoroutine = null;
   }

   private float GetCurrentAngle()
   {
       float currentAngle = Quaternion.Angle(Quaternion.identity, rotatingLeaf.transform.rotation);
       currentAngle *= openAngle > 0 ? 1 : -1;
       return currentAngle;
   }

   private void AssignLeaf()
   {
       if (!rotatingLeaf)
           rotatingLeaf = transform;
   }


   private DoorState GetDoorState(float currentAngle)
   {
       if (Mathf.Approximately(0, currentAngle))
           return DoorState.Close;

       if (Mathf.Approximately(openAngle, currentAngle))
           return DoorState.Open;

       return DoorState.Undefined;
   }

   private enum DoorState
   {
       Undefined,
       Open,
       Close,
   }
}
