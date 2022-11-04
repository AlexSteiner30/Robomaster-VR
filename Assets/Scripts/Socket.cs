using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;

public class Socket : MonoBehaviour
{
    [Header("TCP")]
    public string ip;
    public int port;

    TcpClient client;

    [Header("Oculus")]
    public XRNode movementSource;
    private Vector2 inputAxis;

    public XROrigin rig;

    [Header("Controllers")]
    public XRController rightHand;
    public XRController leftHand;

    [Header("Left Hand Buttons")]
    public InputHelpers.Button sprintButton;

    [Header("Right Hand Buttons")]
    public InputHelpers.Button shootButton;


    // Start is called before the first frame update
    void Start()
    {
        try
        {
            Debug.Log("Connecting.....");

            client = new TcpClient(ip, port);

            Debug.Log("Connected");
        }

        catch (Exception e)
        {
            Debug.Log("Error " + e.StackTrace);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Inputs();
        Move();

        if (Shoot())
            Send("shoot");
    }

    private void Inputs()
    {
        // Movement Axis
        InputDevice device = InputDevices.GetDeviceAtXRNode(movementSource);
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis);
    }

    private void Move()
    {
        Quaternion headYaw = Quaternion.Euler(0, rig.Camera.transform.eulerAngles.y, 0);
        Vector3 orientation = headYaw * new Vector3(inputAxis.x, 0, inputAxis.y);

        if (orientation.x != 0 || orientation.y != 0)
            Send("move " + orientation.x.ToString() + " " + orientation.z.ToString());
    }


    // Right Hand
    public bool Shoot()
    {
        bool shoot;
        rightHand.inputDevice.IsPressed(shootButton, out shoot);

        return shoot;
    }

    private void Send(string command)
    {
        string message = command;
        int byteCount = Encoding.ASCII.GetByteCount(message);

        byte[] sendData = new byte[byteCount];
        sendData = Encoding.ASCII.GetBytes(message);

        NetworkStream stream = client.GetStream();
        stream.Write(sendData, 0, sendData.Length);
    }
}
