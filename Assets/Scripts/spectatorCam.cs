using UnityEngine;
using UnityEngine.InputSystem;

public class spectatorCam : MonoBehaviour
{
    private Transform[] targets;
    private int currentTargetIndex = 0;
    private bool isSpectating = false;
    private Transform originalParent;
    private Vector3 spectatorOffset = new Vector3(0, 2f, -3f);

    public void startSpectating()
    {
        isSpectating = true;
        originalParent = transform.parent;
        transform.SetParent(null);
        refreshTargets();

        if (targets.Length > 0)
            attachToTarget(0);
    }

    private void refreshTargets()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        targets = new Transform[players.Length];
        for (int i = 0; i < players.Length; i++)
            targets[i] = players[i].transform;
    }

    private void attachToTarget(int index)
    {
        if (targets.Length == 0) return;
        currentTargetIndex = index;
        transform.SetParent(targets[currentTargetIndex]);
        transform.localPosition = spectatorOffset;
        transform.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        if (!isSpectating) return;

        if (Keyboard.current.aKey.wasPressedThisFrame)
            cycleTarget(-1);

        if (Keyboard.current.dKey.wasPressedThisFrame)
            cycleTarget(1);
    }

    private void cycleTarget(int direction)
    {
        if (targets.Length == 0) return;
        currentTargetIndex = (currentTargetIndex + direction + targets.Length) % targets.Length;
        attachToTarget(currentTargetIndex);
    }

    public void stopSpectating()
    {
        isSpectating = false;
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}