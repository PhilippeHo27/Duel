using UnityEngine;

public class CubeRotator : MonoBehaviour
{
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 90, 0);
    
    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}