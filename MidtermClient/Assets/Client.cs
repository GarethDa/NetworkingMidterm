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
        //try
        {
            socTcp = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            validationText.text = "Valid IP! Connecting...";

            //Connect through TCP
            socTcp.Connect(ip, 8888);

            //Stop showing the IP entry canvas, start showing chat, let the player move
            myCube.GetComponent<cube>().SetMoveable(true);
            joiningCanvas.SetActive(false);
            chatCanvas.SetActive(true);

            //Set up the remote endpoint
            remoteEP = new IPEndPoint(ip, 8889);

            socUdp = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp);

            //Connect through UDP
            socUdp.Connect(remoteEP);

            //Send the endpoint data to the server, so they can send UDP messages back
            SendMsg("UDP: " + socUdp.LocalEndPoint.ToString());

            //Begin to receive on both sockets
            socTcp.BeginReceive(inBufferTcp, 0, inBufferTcp.Length, 0, new AsyncCallback(ReceiveCallback), socTcp);
            socUdp.BeginReceive(inBufferUdp, 0, inBufferUdp.Length, 0, new AsyncCallback(ReceiveCallbackUdp), socUdp);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
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

            //Create the buffer
            pos = new float[] { playerNum, myCube.transform.position.x, myCube.transform.position.y, myCube.transform.position.z };
            bpos = new byte[pos.Length * 4];
            Buffer.BlockCopy(pos, 0, bpos, 0, bpos.Length);

            Debug.Log("Sending coordinates to server -" +
                "   PlayerNum: " + playerNum +
                "   X: " + myCube.transform.position.x +
                "   Y: " + myCube.transform.position.y +
                "   Z: " + myCube.transform.position.z);

            //Send the buffer through UDP
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

        //If we are receiving the player number
        if (msg.Length >= 5 && msg.Substring(0, 4).Equals("PN: "))
        {
            //Set the player number
            playerNum = int.Parse(msg.Substring(4));

            Debug.Log("I am player: " + playerNum);

            //Begin receiving again
            socTcp.BeginReceive(inBufferTcp, 0, inBufferTcp.Length, 0, new AsyncCallback(ReceiveCallback), socTcp);
        }

        //If we are receiving a chat message
        else if (msg.Length >= 5 && msg.Substring(0, 5).Equals("msg: "))
        {
            Debug.Log("Message received: " + msg.Substring(5, msg.Length - 6));

            //Queue up the message to be displayed
            //Note: messages are received in the format "{message}{player number of client which sent message}"
            //So we create the message to be displayed in the chat box as "From Player {playerNumber}: {message}" 
            chatBehaviour.QueueMessage("From Player " + msg[msg.Length - 1] + ": " + msg.Substring(5, msg.Length - 6));

            //Begin receiving again
            socTcp.BeginReceive(inBufferTcp, 0, inBufferTcp.Length, 0, new AsyncCallback(ReceiveCallback), socTcp);
        }

        //If a player has quit
        else if (msg.Length >= 6 && msg.Substring(0, 6).Equals("quit: "))
        {
            //Send a chat message saying the player has quit
            chatBehaviour.QueueMessage("***Player " + msg.Substring(6) + " has quit***");
        }

        else
            Debug.Log("Invalid info received!!!!");


    }
    private void ReceiveCallbackUdp(IAsyncResult result)
    {
        Socket socket = (Socket)result.AsyncState;

        int rec = socket.EndReceive(result);

        pos = new float[rec / 4];
        Buffer.BlockCopy(inBufferUdp, 0, pos, 0, rec);

        Debug.Log("Recv (playerNum, x, y, z): (" + pos[0] + ", " + pos[1] + ", " + pos[2] + ", " + pos[3] + ")");

        //If the position data we have received isn't our own, then set the other player's cube to the position received
        if (pos[0] != playerNum)
            p2Cube.transform.position = new Vector3(pos[1], pos[2], pos[3]);

        //Begin receiving again
        socUdp.BeginReceive(inBufferUdp, 0, inBufferUdp.Length, 0, new AsyncCallback(ReceiveCallbackUdp), socUdp);
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
        //Send a message saying we have quit if the app has quit
        if (socTcp.Connected)
            SendMsg("quit: " + playerNum);
    }

    void OnDestroy()
    {
        //Send a message saying we have quit if the client has been destroyed
        if (socTcp.Connected)
            SendMsg("quit: " + playerNum);
    }

    public int GetPlayerNum()
    {
        return playerNum;
    }
}
