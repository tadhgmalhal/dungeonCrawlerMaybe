using UnityEngine;

public class roomConnection : MonoBehaviour
{
    public bool isConnected = false;

    private void OnDrawGizmos()
    {
        Gizmos.color = isConnected ? Color.green : Color.red;
        Gizmos.DrawSphere(transform.position, 0.2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 0.1f);
    }
}