using UnityEngine;

public class Room : MonoBehaviour
{
    public roomConnection[] connectors;

    private void OnValidate()
    {
        connectors = GetComponentsInChildren<roomConnection>();
    }
}
