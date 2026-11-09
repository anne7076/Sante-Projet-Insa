using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using hapticDriver;
using UnityEditor;
using UnityEngine;

public class VibrationManager_minimal: MonoBehaviour
{
    public string devicePort = "COM4"; // Check which port is used on your system !
    private Timer callbackTimer;
    private Driver driver;
    
    
    // Examples of values that can be edited in the inspector
    [Range(0,255)]
    public byte motor_1 = 100;
    
    [Range(0,255)]
    public byte motor_2 = 0;

    [Range(0,255)]
    public byte motor_3 = 0;

    [Range(0,255)]
    public byte motor_4 = 0;

    [Range(0,10000)]
    public int duration = 1000;
    
    [System.Serializable]
    public class Motor{
        int motorId;
        byte intensity;
        int duration;
    }
    
    public List<Motor> motors = new List<Motor>(); // I want to make this list editable in the inspector,how ? 

    public bool useMock = false;


    public void InitDriver()
    {
        Disconnect();
        // create a new driver instance
        driver = new Driver(devicePort, useMock);
        // create a new timer that will call the emitterCallback function every 40ms
        callbackTimer = new Timer(EmitterCallback, null, 0, 40);
    }

    public void Disconnect()
    {
        if (driver != null)
        {
            driver.Close();
            driver = null;
            callbackTimer.Dispose();
            callbackTimer = null;
        }
    }

    
    // the Start function is called when a script is enabled
    public void Start()
    {
        InitDriver();
    }
    
    // this function is called every 40ms
    private void EmitterCallback(object state)
    {
        //driver.SetMessage(getDefaultMessage());
        driver.SendMessage();
    }
    
    // default message is a message with all motors at 0 + the end marker
    private byte[] getDefaultMessage()
    {
        return new byte[5] { 0, 0, 0, 0, Driver.EndMarker };
    }

    // methode for sinusoidal vibration : 
    private async void SinusoidalVibration()
    {
        int frequence = 1;
        int amplitude = 200;
        int phi = 0;
        int duration = 10000;
        DateTime startTime = DateTime.Now;
        DateTime t = startTime;
        // while the duration is not reached, we will send a message to the driver every 40ms using sinusoidal function to generate the vibration
        while ((t - startTime).TotalMilliseconds < duration)
        {
            // calculate the intensity of the vibration using a sinusoidal function
            motor_1 = (byte)(amplitude * (Math.Sin(2 * Math.PI * frequence * (t - startTime).TotalMilliseconds / 1000 + phi) + 1) / 2);
            // create a new message with the calculated intensities
            byte[] message = new byte[5] { motor_1, motor_2, motor_3, motor_4, Driver.EndMarker };
            // send the message to the driver
            driver.SetMessage(message);
            await Task.Delay(40);
            t = DateTime.Now;
        }
        driver.SetMessage(getDefaultMessage());
    }
    
    // method for direction vibration : right, left, front, back.
    private void DirectionalVibration()
    {
        byte amplitude = 200;
        int duration = 1000;
        Action<byte> left = async (byte amplitude) =>{
            byte[] message = new byte[5] {amplitude, 0, 0, 0, Driver.EndMarker };
            driver.SetMessage(message);
            await Task.Delay(duration);
        };
        left(amplitude);
        driver.SetMessage(getDefaultMessage());
    }

    public void playOne()
    {
        // sinusoidal vibration example
        // SinusoidalVibration();
        // directional vibration example
        DirectionalVibration();
    }
}


// this is the editor script that will be used to display buttons in the inspector
[CustomEditor(typeof(VibrationManager_minimal))]
public class VibrationManagerEditor : Editor
{
    // instance is the object that is being edited/displayed
    private VibrationManager_minimal instance;
    private void OnEnable()
    {
        instance = (VibrationManager_minimal)target;
    }

    // this function is called when the inspector is drawn
    // this is where we can add buttons
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // draw the default inspector

        if (GUILayout.Button("Initialize Driver")) instance.InitDriver();
        
        if (GUILayout.Button("Vibrer !")) instance.playOne();

        if (GUILayout.Button("Disconnect")) instance.Disconnect();
    }
}