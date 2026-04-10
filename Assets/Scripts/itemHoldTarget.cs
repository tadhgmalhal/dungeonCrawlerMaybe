using UnityEngine;

public class ItemHoldTarget : MonoBehaviour
{
    public static ItemHoldTarget instance;

    void Awake()
    {
        instance = this;
    }
}