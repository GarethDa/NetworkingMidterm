using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Lec4
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using TMPro;

public class Client : MonoBehaviour
{
    [SerializeField] TMP_Text validationText;

    public GameObject myCube;
    private static Socket clientSoc;

    private static byte[] bpos;
    private static float[] pos;

    Vector3 prevPos = new Vector3(0f, 0f, 0f);

    float currentTimer = 0f;

    IPAddress ip = IPAddress.Parse("0.0.0.0");

    public static void StartClient()
    {
        try
        {
            clientSoc = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            clientSoc.Connect(IPAddress.Parse("127.0.0.1"), 8888);
            Debug.Log("Connected to server");

        } catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        myCube = gameObject;
        StartClient();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(ip.ToString());
        currentTimer += Time.deltaTime;

        //If the cube has moved and we reached the requiured time for sending a packet
        if (prevPos != myCube.transform.position && currentTimer >= 0.4f)
        {
            currentTimer = 0f;

            pos = new float[] { myCube.transform.position.x, myCube.transform.position.y, myCube.transform.position.z };
            bpos = new byte[pos.Length * 4];
            Buffer.BlockCopy(pos, 0, bpos, 0, bpos.Length);

            Debug.Log("Sending coordinates to server -" +
                "   X: " + myCube.transform.position.x +
                "   Y: " + myCube.transform.position.y +
                "   Z: " + myCube.transform.position.z);

            clientSoc.Send(bpos);
        }

        prevPos = myCube.transform.position;

    }

    public void SetIP(string inIP)
    {
        Debug.Log("Here! IP = " + inIP);
        ip = IPAddress.Parse(inIP);

        try
        {
            clientSoc.Connect(IPAddress.Parse("127.0.0.1"), 8888);
        }

        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }

    }
}
