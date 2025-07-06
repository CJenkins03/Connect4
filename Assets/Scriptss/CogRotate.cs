using UnityEngine;

public class CogRotate : MonoBehaviour
{

    [SerializeField] private float speed;
    [SerializeField] private bool reverse;

    // Update is called once per frame
    void Update()
    {
        if(reverse) transform.Rotate(new Vector3(0, 0, -5) * speed * Time.deltaTime);
        else transform.Rotate(new Vector3(0, 0, 5) * speed * Time.deltaTime);
    }
}
