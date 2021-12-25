using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Look : MonoBehaviour
{
    [SerializeField]
    private Transform lookObject;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 position = lookObject.position;
        position.y = transform.position.y;
        Vector3 direction = position - transform.position;
        transform.forward = direction.normalized;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
