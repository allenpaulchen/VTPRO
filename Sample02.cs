using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class Sample02 : MonoBehaviour
{
    public GameObject humanoid, mesh, camera;
    public bool mirror = true;
    public bool move = true;
    public Material humanoid_material;
    int clothes_number = 0;
    public float correction;

    NtUnity.Kinect nt;
    NtUnity.HumanoidSkeleton hs;

    void Start()
    {
        nt = new NtUnity.Kinect();
        hs = new NtUnity.HumanoidSkeleton(humanoid);
        humanoid.SetActive(false);
    }

    void Update()
    {
        nt.setRGB();
        nt.setSkeleton();
        nt.setFace();
        // nt.imshowBlack();
        int n = nt.getSkeleton();
        //humanoid.transform.Rotate(0, hs.joint[NtUnity.Kinect.JointType_ShoulderLeft].z - hs.joint[NtUnity.Kinect.JointType_ShoulderRight].z, 0, Space.Self);
        if (n > 0)
        {
            humanoid.SetActive(true);
            hs.set(nt, 0, correction ,  mirror, move);
            //humanoid.transform.rotation = Quaternion.Euler(0, PointRotation(hs.joint[NtUnity.Kinect.JointType_HipRight], hs.joint[NtUnity.Kinect.JointType_HipLeft]), 0);
            camera.transform.position = new Vector3(0, hs.joint[NtUnity.Kinect.JointType_Head].y, 0);
        }
        else
            humanoid.SetActive(false);

        if (hs.joint[NtUnity.Kinect.JointType_ElbowLeft].y > hs.joint[NtUnity.Kinect.JointType_Head].y &&
            hs.joint[NtUnity.Kinect.JointType_ElbowRight].y > hs.joint[NtUnity.Kinect.JointType_Head].y)
        {
            Thread.Sleep(500);//想一下要怎麼辦比較好
            clothes_number++;

            switch (clothes_number)
            {
                case 1:
                    humanoid_material = Resources.Load("Material/sample/circus1", typeof(Material)) as Material;
                    break;
                case 2:
                    humanoid_material = Resources.Load("Material/sample/circus2", typeof(Material)) as Material;
                    break;
                case 3:
                    humanoid_material = Resources.Load("Material/sample/underocean", typeof(Material)) as Material;
                    break;
                //insert new here
                default:
                    clothes_number = 0;
                    humanoid_material = Resources.Load("Material/sample/mine", typeof(Material)) as Material;
                    break;
            }
            mesh.GetComponent<Renderer>().material = humanoid_material;
        }
    }


    void OnApplicationQuit()
    {
        nt.stopKinect();
    }
}