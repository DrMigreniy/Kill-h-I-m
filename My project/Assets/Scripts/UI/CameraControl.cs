using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float cameraDistanceZ = -10f;
    [SerializeField] private float cameraDistanceY = 0.3f;
    [SerializeField] private float smooth = 0.015f;
    private Vector3 pos;

    
    void Start()
    {
        if (!player)
            player = FindObjectOfType<Hero>().transform;
    }

    
    void Update()
    {
        pos = player.position;
        pos.z = cameraDistanceZ;
        pos.y -= cameraDistanceY;

        transform.position = Vector3.Lerp(transform.position, pos, smooth * Time.deltaTime);
    }
}
