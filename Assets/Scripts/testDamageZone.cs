using UnityEngine;

public class testDamageZone : MonoBehaviour
{
    public float damage = 10f;
    public float tickRate = 0.5f;
    private float tickTimer = 0f;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tickTimer += Time.deltaTime;
            if (tickTimer >= tickRate)
            {
                tickTimer = 0f;
                other.GetComponent<playerHP>().takeDamage(damage);
            }
        }
    }
}