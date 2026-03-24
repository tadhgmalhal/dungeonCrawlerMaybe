using UnityEngine;
using UnityEngine.InputSystem;

public class descendChute : MonoBehaviour
{
    public float interactRange = 3f;
    private static Camera mainCam;

    private void Start()
    {
        if (mainCam == null)
            mainCam = Camera.main;
    }

    private void Update()
    {
        if (mainCam == null) return;

        Ray ray = new Ray(mainCam.transform.position, mainCam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            if (hit.collider.gameObject == gameObject)
            {
                Debug.Log("Press E to descend");

                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    descend();
                }
            }
        }
    }

    private void descend()
    {
        gameSceneManager.Instance.descendToNextFloor();
    }
}