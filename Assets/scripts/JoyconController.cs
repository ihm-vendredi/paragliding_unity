using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoyconController : MonoBehaviour
{

    // Calibrer les valeures de la magnitude pour detecter le mouvement
    public float borne_sup_magnitude = (float)1.018;

    public float coeffAcceleration = (float)0.07;

    // Calibrer les valeur de monté et descente pour detecter la monté et descente
    public float borne_monte = (float)-1.02;
    public float borne_descente = (float)-0.08;

    public JoyconManager.JoyconType joyconType;
    private float leftOrientationX;
    private float rightOrientationX;
    //public float[] stick;

    public Vector3 gyro = Vector3.zero;
    public float gyroMagnitude;

    public Vector3 accel = Vector3.zero;
    public float accelMagnitude;
    public Quaternion orientation;
    public Vector3 rotation;
    //public Joycon joycon;
    Vector3 rotationOffset = new Vector3(0, 180, 0);

    private enum Direction { Droite, Gauche, Arret }

    private Direction rightJoyconDirectionState;
    private Direction leftJoyconDirectionState;

    private Vector3 position;

    JoyconManager joyconManager;
    List<Joycon> joycons;

    void Start()
    {
        joyconManager = JoyconManager.Instance;
        //joycon = joyconManager.GetJoycon(joyconType);

        // get the public Joycon array attached to the JoyconManager in scene
        joycons = JoyconManager.Instance.j;
    }

    private void SetJoyconState()
    {
        foreach (Joycon joycon in joycons)
        {
            float magnitude = joycon.GetAccel().magnitude;
            Vector3 accelero = joycon.GetAccel();
            Vector3 lastPosition = new Vector3(0, (float)-1.0);

            // Bouton shoulder appuye
            if (joycon.GetButton(Joycon.Button.SHOULDER_1))
            {
                //Debug.Log("orientationX " + (rightOrientationX - rotation.x));
                float borne_inf_magnitude = 1 - (borne_sup_magnitude - 1);

                // Assez de magnitude sur l'accelerometre pour considerer un mouvement
                if (magnitude < borne_inf_magnitude || magnitude > borne_sup_magnitude)
                {
                    // joycon droit
                    if (accelero.y > borne_monte || accelero.y < borne_descente)
                    {
                        //Debug.Log("Monte : " + accelero);
                        //gameObject.transform.position = new Vector3(position.x, position.y - magnitude / 2, position.z);
                        //gameObject.transform.position = newGameObjectPosition;
                        //gameObject.transform.rotation = orientation;

                        if (rotation.x >= 0 && rotation.x <= 90)
                        {
                            // Gauche pour Joy droite & inverssement
                            //Debug.Log("Gauche");

                            if (!joycon.isLeft)
                            {
                                rightJoyconDirectionState = Direction.Gauche;
                            }
                            else
                            {
                                leftJoyconDirectionState = Direction.Droite;
                            }
                        }
                        else if (rotation.x <= 360 && rotation.x >= 270)
                        {
                            //Debug.Log("Droite");

                            if (!joycon.isLeft)
                            {
                                rightJoyconDirectionState = Direction.Droite;
                            }
                            else
                            {
                                leftJoyconDirectionState = Direction.Gauche;
                            }
                        }
                        else
                        { leftJoyconDirectionState = Direction.Arret; rightJoyconDirectionState = Direction.Arret; }


                    }
                    else
                    { leftJoyconDirectionState = Direction.Arret; rightJoyconDirectionState = Direction.Arret; }
                }
                else { leftJoyconDirectionState = Direction.Arret; rightJoyconDirectionState = Direction.Arret; }
            }
            else { leftJoyconDirectionState = Direction.Arret; rightJoyconDirectionState = Direction.Arret; }
        }
    }

    // Update is called once per frame
    void Update()
    {

        foreach (Joycon joycon in joycons)
        {
            // make sure the Joycon only gets checked if attached
            if (joycon != null)
            {
                // GetButtonDown checks if a button has been pressed (not held)
                if (joycon.GetButtonDown(Joycon.Button.SHOULDER_1))
                {
                    // Joycon has no magnetometer, so it cannot accurately determine its yaw value. Joycon.Recenter allows the user to reset the yaw value.
                    joycon.Recenter();

                    if (joycon.isLeft)
                    {
                        // Joycon gauche
                        leftOrientationX = rotation.x;
                        //gameObject.transform.position = new Vector3((float)2.421276, (float)-0.1984076, (float)-1.163957);   
                    }
                    else
                    {
                        // Joycon droit
                        rightOrientationX = rotation.x;
                        //gameObject.transform.position = new Vector3((float)0.84, (float)-0.1984076, (float)-0.09);
                    }
                }

                //stick = joycon.GetStick();

                // Gyro values: x, y, z axis values (in radians per second)
                gyro = joycon.GetGyro();
                gyroMagnitude = gyro.magnitude;

                // Accel values:  x, y, z axis values (in Gs)
                accel = joycon.GetAccel();
                accelMagnitude = accel.magnitude;

                // fix rotation
                orientation = joycon.GetVector();
                orientation = new Quaternion(orientation.x, orientation.z, orientation.y, orientation.w);
                Quaternion quat = Quaternion.Inverse(orientation);
                Vector3 rot = quat.eulerAngles;
                rot += rotationOffset;
                orientation = Quaternion.Euler(rot);
                rotation = orientation.eulerAngles;

                //Debug.LogWarning("Gyro : " + gyro.ToString());
                //Debug.LogWarning("Magnitude G : " + gyroMagnitude.ToString());
                //Debug.LogWarning("Accel : " + accel.ToString());
                //Debug.LogWarning("Magnitude A: " + accelMagnitude.ToString());


                // Doit être placé sous la position
                SetJoyconState();

                position = gameObject.transform.position;
                Vector3 newGameObjectPosition = new Vector3(position.x, position.y, position.z);

                if (rightJoyconDirectionState == Direction.Droite)
                {
                    newGameObjectPosition = new Vector3(position.x + coeffAcceleration, position.y, position.z);
                }
                else if (rightJoyconDirectionState == Direction.Gauche)
                {
                    newGameObjectPosition = new Vector3(position.x - coeffAcceleration, position.y, position.z);
                }

                gameObject.transform.position = newGameObjectPosition;
                //gameObject.transform.rotation = orientation;

            }
        }
    }
}