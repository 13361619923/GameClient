using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId = 0;
    public TCP tcp;

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
        tcp = new TCP();
    }

    public void ConnectToServer()
    {
        tcp.Connect();
    }

    public void DisconnectedToServer()
    {
        tcp.Disconnect();
    }
    public void Send(string str)
    {
        tcp.Send(str);
    }

    private void OnDisable()
    {
        tcp?.Disconnect();
    }


    public delegate void ConnectHandler(TcpClient socket);
    public delegate void DisconnectHandler(TcpClient socket);
    public delegate void MessageReceivedHandler(string str);

    public class TCP
    {
        public TcpClient socket = new TcpClient
        {
            ReceiveBufferSize = dataBufferSize,
            SendBufferSize = dataBufferSize
        };

        private NetworkStream stream;
        private byte[] receiveBuffer;

        public event ConnectHandler Connected;
        public event DisconnectHandler Disonnected;
        public event MessageReceivedHandler messageReceived;

        public void Connect()
        {
            receiveBuffer = new byte[dataBufferSize];

            if (socket == null || socket.Connected == false)
            {
                socket = new TcpClient
                {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize
                };
                socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
            }
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);

            if (!socket.Connected)
            {
                return;
            }

            Connected?.Invoke(this.socket);

            stream = socket.GetStream();

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    Disonnected?.Invoke(this.socket);
                    // TODO: disconnect
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                string s = Encoding.UTF8.GetString(_data);

                Debug.Log("接收到数据：" + s);
                messageReceived?.Invoke(s);
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                Disonnected?.Invoke(this.socket);
                // TODO: disconnect
            }
        }

        // 客户端向服务器发送数据
        public void Send(string msg)
        {
            if (socket != null && socket.Connected)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(msg);

                stream = socket.GetStream();

                stream.Write(buffer, 0, buffer.Length);

                Debug.Log("发出消息：" + msg);

            }
        }

        public void Disconnect()
        {
            socket?.Close();
            socket?.Dispose();
            socket = null;
            Disonnected?.Invoke(socket);
        }

    }


}
