using UnityEngine;

// [INFO]
// IGNORE THIS FILE!
// It's just a quick script to make the test Cube movable.


public class CubeMovement : MonoBehaviour
{
    [SerializeField] float speedFactor = 0.09f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            this.transform.Translate(0, 0, speedFactor);
        }
        else
        {
            if (Input.GetKey(KeyCode.DownArrow))
            {
                this.transform.Translate(0, 0, (-speedFactor));
            }
        }
    }
}