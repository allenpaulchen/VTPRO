using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class model_Control : MonoBehaviour
{
    public GameObject[] humanoid = new GameObject[] { null, null, null, null, null, null };
    //for change material
    public GameObject[] mesh = new GameObject[] { null, null, null, null, null, null };
    public GameObject camera;
    public bool[] mirror = new bool[] { true, true, true, true, true, true };
    public bool[] move = new bool[] { true, true, true, true, true, true };
    public int[] clothes_number = new int[] { 0, 0, 0, 0, 0, 0 };
    public Material humanoid_material;
    public float correction;

    NtUnity.Kinect nt;
    NtUnity.HumanoidSkeleton[] hs = new NtUnity.HumanoidSkeleton[] { null, null, null, null, null, null };

    void Start()
    {
        nt = new NtUnity.Kinect();
        for (int i = 0; i < humanoid.Length; i++)
        {
            if (humanoid[i] != null)
            {
                hs[i] = new NtUnity.HumanoidSkeleton(humanoid[i]);
                mesh[i] = humanoid[i].gameObject.transform.GetChild(1).gameObject;
            }
            humanoid[i].transform.position = new Vector3(0, 0, -5);

            //humanoid[i].transform.parent = humanoid_parent[i].transform;
            //Debug.Log(humanoid[i].name +" is : "+ humanoid[i].transform.parent.name);


        }
        //*****for change object size
        //humanoid[0].transform.localScale += new Vector3(2, 0, 0);
    }

    void Update()
    {
        nt.setRGB();
        nt.setSkeleton();
        nt.setFace();
        nt.imshowBlack();

        if (Input.GetKey(KeyCode.P))
            correction = correction + 0.01f;
        else if (Input.GetKey(KeyCode.M))
            correction = correction - 0.01f;
        for (int i = 0; i < nt.skeleton.Count; i++)
        {
            camera.transform.position = new Vector3(0, hs[0].joint[NtUnity.Kinect.JointType_ShoulderRight].y, 0);
            if (hs[i] != null)
            {
                hs[i].set(nt, i, correction, mirror[i], move[i]);
                //float rota = hs[i].PointRotation(hs[i].joint[NtUnity.Kinect.JointType_HipRight], hs[i].joint[NtUnity.Kinect.JointType_HipLeft]);
                // humanoid[i].transform.rotation = Quaternion.Euler(0, rota, 0);
                //humanoid[i].transform.Rotate(0, (-rota + 180) * 1.3f, 0, Space.World);
            }
            else
                humanoid[i].transform.position = new Vector3(0, 0, -5);
        }
    }

    void FixedUpdate()
    {
        for (int i = 0; i < nt.skeleton.Count; i++)
        {
            if (hs[i].joint[NtUnity.Kinect.JointType_ElbowLeft].y > hs[i].joint[NtUnity.Kinect.JointType_Head].y &&
            hs[i].joint[NtUnity.Kinect.JointType_ElbowRight].y > hs[i].joint[NtUnity.Kinect.JointType_Head].y)
            {
                clothes_number[i]++;
                //for circus
                switch (clothes_number[i])
                {
                    case 1:
                        Debug.Log("circus");
                        humanoid_material = Resources.Load("Material/sample/circus", typeof(Material)) as Material;
                        break;
                    case 2:
                        Debug.Log("desert");
                        humanoid_material = Resources.Load("Material/sample/desert", typeof(Material)) as Material;
                        break;
                    case 3:
                        Debug.Log("seabed");
                        humanoid_material = Resources.Load("Material/sample/seabed", typeof(Material)) as Material;
                        break;
                    case 4:
                        Debug.Log("outerspace");
                        humanoid_material = Resources.Load("Material/sample/outerspace", typeof(Material)) as Material;
                        break;
                    //insert new here
                    default:
                        clothes_number[i] = 0;
                        humanoid_material = Resources.Load("Material/sample/mine", typeof(Material)) as Material;
                        break;
                }
                mesh[i].GetComponent<Renderer>().material = humanoid_material;
            }
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("finished"); ;
        nt.stopKinect();
    }
}