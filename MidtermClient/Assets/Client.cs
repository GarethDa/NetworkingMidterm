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
    [SerializeField] GameObject joiningCanvas;
    [SerializeField] GameObject chatCanvas;

    public GameObject myCube;
    private Socket socTcp;
    private Socket socUdp;

    private byte[] bpos;
    private float[] pos;

    private byte[] outBuffer = new byte[512];

    private IPEndPoint remoteEP;

    Vector3 prevPos = new Vector3(0f, 0f, 0f);

    float currentTimer = 0f;

    private IPAddress ip = IPAddress.Parse("0.0.0.0");

    public void StartClient()
    {
        try
        {
            socTcp = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            validationText.text = "Valid IP! Connecting...";

            socTcp.Connect(ip, 8888);

            myCube.GetComponent<cube>().SetMoveable(true);
            joiningCanvas.SetActive(false);
            chatCanvas.SetActive(true);

            remoteEP = new IPEndPoint(ip, 8889);

            socUdp = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp);

        } catch (Exception e)
        {
            Debug.Log(e.ToString());
            validationText.text = "Invalid IP!";
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        //myCube = gameObject;
        //StartClient();
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

            socTcp.Send(bpos);
        }

        prevPos = myCube.transform.position;

    }

    public void SendMsg(string msg)
    {
        outBuffer = Encoding.ASCII.GetBytes(msg);

        socUdp.SendTo(outBuffer, remoteEP);
    }

    public void SetIP(string inIP)
    {
        Debug.Log("Here! IP = " + inIP);
        ip = IPAddress.Parse(inIP);

        StartClient();
    }
}
