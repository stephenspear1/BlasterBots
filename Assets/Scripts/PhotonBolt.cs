using UnityEngine;

public class PhotonBolt : MonoBehaviour
{
    public float lifeTime = 3f;
    public int damage = 10;

    void Start() => Destroy(gameObject, lifeTime);

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return;
        Destroy(gameObject);
    }
}
