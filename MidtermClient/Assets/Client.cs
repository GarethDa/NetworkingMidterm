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
    [SerializeField] GameObject p2Cube;

    ChatBoxBehaviour chatBehaviour;

    public GameObject myCube;
    private Socket socTcp;
    private Socket socUdp;

    private byte[] bpos;
    private float[] pos;

    private byte[] outBufferTcp = new byte[512];
    private byte[] inBufferTcp = new byte[512];

    private byte[] outBufferUdp = new byte[512];
    private byte[] inBufferUdp = new byte[512];

    private IPEndPoint remoteEP;

    Vector3 prevPos = new Vector3(0f, 0f, 0f);

    float currentTimer = 0f;

    private IPAddress ip;

    int playerNum;

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

            //Debug.Log("UDP Endpoint: " + socUdp.LocalEndPoint.ToString());

            socTcp.BeginReceive(inBufferTcp, 0, inBufferTcp.Length, 0, new AsyncCallback(ReceiveCallback), socTcp);
            socUdp.BeginReceive(inBufferUdp, 0, inBufferUdp.Length, 0, new AsyncCallback(ReceiveCallbackUdp), socUdp);

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

        chatBehaviour = chatCanvas.GetComponentInChildren<ChatBoxBehaviour>();
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

            pos = new float[] { playerNum, myCube.transform.position.x, myCube.transform.position.y, myCube.transform.position.z };
            bpos = new byte[pos.Length * 4];
            Buffer.BlockCopy(pos, 0, bpos, 0, bpos.Length);

            Debug.Log("Sending coordinates to server -" +
                "   PlayerNum: " + playerNum +
                "   X: " + myCube.transform.position.x +
                "   Y: " + myCube.transform.position.y +
                "   Z: " + myCube.transform.position.z);

            socUdp.SendTo(bpos, remoteEP);
        }

        prevPos = myCube.transform.position;

    }

    private void ReceiveCallback(IAsyncResult result)
    {
        Socket socket = (Socket)result.AsyncState;

        int rec = socket.EndReceive(result);

        byte[] data = new byte[rec];

        Array.Copy(inBufferTcp, data, rec);

        string msg = Encoding.ASCII.GetString(data);

        if (msg.Substring(0, 4).Equals("PN: "))
        {
            playerNum = int.Parse(msg.Substring(4));

            Debug.Log("I am player: " + playerNum);
        }

        else if (msg.Substring(0, 5).Equals("msg: "))
        {
            Debug.Log("Message received: " + msg.Substring(5));

            chatBehaviour.QueueMessage(msg.Substring(5));

            socTcp.BeginReceive(inBufferTcp, 0, inBufferTcp.Length, 0, new AsyncCallback(ReceiveCallback), socTcp);
        }
        
        else
        {
            //Logic for player quitting
        }
    }
    private void ReceiveCallbackUdp(IAsyncResult result)
    {
        Socket socket = (Socket)result.AsyncState;

        int rec = socket.EndReceive(result);

        pos = new float[rec / 4];
        Buffer.BlockCopy(inBufferUdp, 0, pos, 0, rec);

        Debug.Log("Recv (playerNum, x, y, z): (" + pos[0] + ", " + pos[1] + ", " + pos[2] + ", " + pos[3] + ")");

        
        p2Cube.transform.position = new Vector3(pos[1], pos[2], pos[3]);

        socUdp.BeginReceive(inBufferUdp, 0, inBufferUdp.Length, 0, new AsyncCallback(ReceiveCallback), socUdp);
    }

    public void SendMsg(string msg)
    {
        outBufferTcp = Encoding.ASCII.GetBytes(msg);

        socTcp.Send(outBufferTcp);
    }

    public void SetIP(string inIP)
    {
        Debug.Log("Here! IP = " + inIP);
        ip = IPAddress.Parse(inIP);

        StartClient();
    }

    void OnApplicationQuit()
    {
        if (socTcp.Connected)
            SendMsg("quit");
    }

    void OnDestroy()
    {
        if (socTcp.Connected)
            SendMsg("quit");
    }
}
