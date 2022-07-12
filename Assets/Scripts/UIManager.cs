using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject startMenu;
    //public InputField usernameField;
    public Text connectStatusText;
    public Text receivedText;
    public Text textForSend;
    public Button sendButton;

    private bool connectStatus;
    private string receivedMsg; 

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

    }
    private void Start()
    {
        Client.instance.tcp.Connected += new Client.ConnectHandler(ConnectHandler);
        Client.instance.tcp.Disonnected += new Client.DisconnectHandler(DisconnectHandler);
        Client.instance.tcp.messageReceived += new Client.MessageReceivedHandler(MessageReceivedHandler);
        sendButton.onClick.AddListener(SendButtonClick);
    }

    private void SendButtonClick()
    {
        Client.instance.Send(textForSend.text);
    }

    private void MessageReceivedHandler(string str)
    {
        receivedMsg ="接收到消息："+str;
    }

    private void Update()
    {
        if ( connectStatus == true)
        {
            connectStatusText.text = "连接状态：" + true;
        }
        else
        {
            connectStatusText.text = "连接状态：" + false;
        }

        if (!string.IsNullOrEmpty(receivedMsg))
        {
            receivedText.text = receivedMsg;
        }
        
    }
    private void DisconnectHandler(TcpClient socket)
    {
        if (socket == null)
        {
            Debug.Log("连接状态：" + false);
            connectStatus = false;
        }
    }

    public void ConnectToServer()
    {
        Client.instance.ConnectToServer();
    }

    public void DisconnectedToServer()
    {
        Client.instance.DisconnectedToServer();
    }

    private void ConnectHandler(TcpClient socket)
    {
        Debug.Log("连接状态：" + socket.Connected);

        connectStatus = true;
    }
}
