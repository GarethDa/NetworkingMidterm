using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cube : MonoBehaviour
{
    bool moveable = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (moveable)
            transform.Translate(Input.GetAxis("Horizontal") * Time.deltaTime * 2f, 
                0, Input.GetAxis("Vertical") * Time.deltaTime *2f);   
    }

    public void SetMoveable(bool canMove)
    {
        moveable = canMove;
    }

    public bool GetMoveable()
    {
        return moveable;
    }
}
