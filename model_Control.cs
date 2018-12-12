using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.UI;

static class ConstantsForObject
{
    public const float Original_humanoid_dist = 0.3721694f;
    public const float Original_humanoid_up_dist = 0.5541338f;
    public const float Original_humanoid_arms_dist = 0.4464892f;
    public const float Original_humanoid_legs_dist = 0.8394225f;
}

public class model_Control : MonoBehaviour
{
    public GameObject[] humanoid = new GameObject[] { null, null, null, null, null, null };
    //for change material
    public GameObject[] mesh = new GameObject[] { null, null, null, null, null, null };
    public GameObject camera;
    public bool[] mirror = new bool[] { true, true, true, true, true, true };
    public bool[] move = new bool[] { true, true, true, true, true, true };
    public int[] clothes_number = new int[] { 0, 0, 0, 0, 0, 0 };
    //correction for doing x asix correction
    //mouse_scroll for x scale correction
    public float correction, mouse_scroll;
    //model距離差
    public float humanoid_dist, humanoid_up_dist, humanoid_arms_dist, humanoid_legs_dist;
    //humanoid_size for setting object size , in my model is about 0.25f
    public float DefaultBase_all, DefaultBase_up, DefaultBase_arms, DefaultBase_legs;
    //new humanoid_size for correction object size when using the object
    public float[] curBase_all = new float[] { 0, 0, 0, 0, 0, 0 };
    public float[] curBase_up = new float[] { 0, 0, 0, 0, 0, 0 };
    public float[] curBase_arms = new float[] { 0, 0, 0, 0, 0, 0 };
    public float[] curBase_legs = new float[] { 0, 0, 0, 0, 0, 0 };

    //for test
    public static float sinceLastAction = 0, actionTimeout;
    public static float[] lastRota;
    public static bool[] correctScale, isback, bow, stopcorr;
    public static Vector3 postVector;
    public static float correctXfirst = 0.8f, correctXsecond = 0.6f;

    public Material humanoid_material;

    public Shader shader1;
    public Shader shader2;
    public float width = 0.002f;

    KinectUnity.Kinect nt;
    KinectUnity.HumanoidSkeleton[] hs = new KinectUnity.HumanoidSkeleton[] { null, null, null, null, null, null };

    void Start()
    {
        nt = new KinectUnity.Kinect();

        mouse_scroll = 0;
        sinceLastAction = 0;
        actionTimeout = 0.3f;
        lastRota = new float[] { 0, 0, 0, 0, 0, 0 };
        correctScale = new bool[] { true };
        stopcorr = new bool[] { false };
        isback = new bool[] { false };
        bow = new bool[] { false };
        postVector = new Vector3(1.0f, 0, 0);

        shader1 = Shader.Find("Toon/Basic");
        shader2 = Shader.Find("Toon/Basic Outline");

        for (int i = 0; i < humanoid.Length; i++)
        {
            if (humanoid[i] != null)
            {
                hs[i] = new KinectUnity.HumanoidSkeleton(humanoid[i]);
                mesh[i] = humanoid[i].gameObject.transform.GetChild(1).gameObject;
                calBase(i);
            }
            humanoid[i].transform.position = new Vector3(0, 0, -5);
        }
        // gameObject.transform.Find(humanoid[0].gameObject.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(1).gameObject.name);
        // GameObject.Find("Your_Name_Here").transform.position;
        //初始
        //hs[0].humanoid.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).gameObject.transform.rotation = new Quaternion(-0.09666204f, -0.003195104f, -0.03288203f,-0.9947689f);

    }

    void Update()
    {
        nt.setRGB();
        nt.setSkeleton();
        nt.setFace();
        nt.imshowBlack();
        //Debug.Log(humanoid[0].gameObject.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(1).gameObject.name);
        // object x scale correction
        mouse_scroll = Input.GetAxis("Mouse ScrollWheel");
        if (mouse_scroll > 0)
            correctXsecond = correctXsecond + 0.01f;

        else if (mouse_scroll < 0)
            correctXsecond = correctXsecond - 0.01f;

        // object x axis correction 
        if (Input.GetKey(KeyCode.P))
            curBase_all[0] = curBase_all[0] + 0.01f;
        else if (Input.GetKey(KeyCode.M))
            curBase_all[0] = curBase_all[0] - 0.01f;

        // object transform 
        for (int i = 0; i < nt.skeleton.Count; i++)
        {

            //setting camera high
            camera.transform.position = new Vector3(0, hs[0].joint[KinectUnity.Kinect.JointType_Head].y, 0);

            //Debug.Log(lastRota[0]);
            //set model move and rota
            if (hs[i] != null)
            {
                hs[i].set(nt, i, correction, mirror[i], move[i]);
                //let the model be straight
                hs[i].humanoid.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).gameObject.transform.Rotate(31.143f, 0, 0);
                //hs[i].humanoid.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).gameObject.transform.Rotate (-0.09666204f, -0.003195104f, -0.03288203f);
                //Debug.Log(Time.deltaTime + ", " + correctScale + ", back:" + isback);
                if (correctScale[i] && !stopcorr[i])
                    corrScale(i);
            }
            else
            {
                humanoid[i].transform.position = new Vector3(0, 0, -5);
                resetValue(i);
            }
        }
    }

    void resetValue(int i)
    {
        lastRota[i] = 0;
        correctScale[i] = true;
        stopcorr[i] = false;
        isback[i] = false;
        bow[i] = false;
    }

    //correction object size
    void calBase(int i)
    {
        //all
        humanoid_dist = GameObject.Find(humanoid[i].gameObject.name + "/Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r").transform.position.x
                      - GameObject.Find(humanoid[i].gameObject.name + "/Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l").transform.position.x;
        curBase_all[i] = DefaultBase_all * humanoid_dist / ConstantsForObject.Original_humanoid_dist;
        //up
        humanoid_up_dist = GameObject.Find(humanoid[i].gameObject.name + "/Game_engine/Root/pelvis/spine_01/spine_02/spine_03/neck_01").transform.position.y
                         - GameObject.Find(humanoid[i].gameObject.name + "/Game_engine/Root/pelvis/thigh_r").transform.position.y;
        curBase_up[i] = DefaultBase_up * humanoid_up_dist / ConstantsForObject.Original_humanoid_up_dist;
        //arm
        humanoid_arms_dist = DistOfxy(GameObject.Find(humanoid[i].gameObject.name + "/Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r").transform.position
                                    , GameObject.Find(humanoid[i].gameObject.name + "/Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r").transform.position);
        curBase_arms[i] = DefaultBase_arms * humanoid_arms_dist / ConstantsForObject.Original_humanoid_arms_dist;
        //leg
        humanoid_legs_dist = GameObject.Find(humanoid[i].gameObject.name + "/Game_engine/Root/pelvis/thigh_r").transform.position.y
                           - GameObject.Find(humanoid[i].gameObject.name + "/Game_engine/Root/pelvis/thigh_r/calf_r/foot_r").transform.position.y;
        curBase_legs[i] = DefaultBase_legs * humanoid_legs_dist / ConstantsForObject.Original_humanoid_legs_dist;
    }

    //correction scale
    void corrScale(int i)
    {
        // humanoid scale
        //should correct the problem of reota body
        humanoid[i].transform.localScale = new Vector3((hs[i].joint[KinectUnity.Kinect.JointType_ShoulderRight].x
                                                        - hs[i].joint[KinectUnity.Kinect.JointType_ShoulderLeft].x) / curBase_all[i], 1, 1.5f);// z is set by my mund
        // up y scale
        humanoid[i].gameObject.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).gameObject.transform.localScale
                                         = new Vector3(1, (hs[i].joint[KinectUnity.Kinect.JointType_Neck].y
                                                         - hs[i].joint[KinectUnity.Kinect.JointType_HipLeft].y) / curBase_up[i], 1);
        // left arm y scale
        humanoid[i].gameObject.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject.transform.localScale
              = new Vector3(1, DistOfxy(hs[i].joint[KinectUnity.Kinect.JointType_ShoulderLeft], hs[i].joint[KinectUnity.Kinect.JointType_WristLeft]) / curBase_arms[i]
            / humanoid[i].gameObject.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).gameObject.transform.localScale.y, 1);
        // right arms y scale
        humanoid[i].gameObject.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(0).gameObject.transform.localScale
              = new Vector3(1, (DistOfxy(hs[i].joint[KinectUnity.Kinect.JointType_ShoulderRight], hs[i].joint[KinectUnity.Kinect.JointType_WristRight])) / curBase_arms[i]
            / humanoid[i].gameObject.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).gameObject.transform.localScale.y, 1);
        // left legs y scale
        humanoid[i].gameObject.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(1).gameObject.transform.localScale
                                        = new Vector3(1, (hs[i].joint[KinectUnity.Kinect.JointType_HipLeft].y
                                        - hs[i].joint[KinectUnity.Kinect.JointType_AnkleLeft].y) / curBase_legs[i], 1);
        // right legs y scale 
        humanoid[i].gameObject.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(2).gameObject.transform.localScale
                                        = new Vector3(1, (hs[i].joint[KinectUnity.Kinect.JointType_HipRight].y
                                        - hs[i].joint[KinectUnity.Kinect.JointType_AnkleRight].y) / curBase_legs[i], 1);
    }


    float DistOfxy(Vector3 first, Vector3 second)
    {
        double doublex = System.Math.Pow(System.Convert.ToDouble(first.x - second.x), 2);
        double doubley = System.Math.Pow(System.Convert.ToDouble(first.y - second.y), 2);
        double doublez = System.Math.Pow(System.Convert.ToDouble(first.z - second.z), 2);

        float result = (float)System.Math.Sqrt(doublex + doubley + doublez);
        if (float.IsPositiveInfinity(result))
        {
            result = float.MaxValue;
        }
        else if (float.IsNegativeInfinity(result))
        {
            result = float.MinValue;
        }
        return result;
    }

    void OnGUI()
    {
        // check the value of humanoid.loaclScale.x
        GUI.Label(new Rect(0, 0, 100, 100), correctXsecond.ToString());
        GUI.Label(new Rect(0, 100, 100, 100), humanoid[0].gameObject.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).gameObject.transform.rotation.x.ToString());
        GUI.Label(new Rect(0, 200, 100, 100), humanoid[0].gameObject.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).gameObject.transform.rotation.y.ToString());
        GUI.Label(new Rect(0, 300, 100, 100), humanoid[0].gameObject.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).gameObject.transform.rotation.z.ToString());
        GUI.Label(new Rect(100, 300, 100, 100), humanoid[0].gameObject.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).gameObject.transform.rotation.w.ToString());
    }

    // change material
    void FixedUpdate()
    {

        for (int i = 0; i < nt.skeleton.Count; i++)
        {
            if (hs[i].joint[KinectUnity.Kinect.JointType_ElbowLeft].y > hs[i].joint[KinectUnity.Kinect.JointType_Head].y &&
            hs[i].joint[KinectUnity.Kinect.JointType_ElbowRight].y > hs[i].joint[KinectUnity.Kinect.JointType_Head].y)
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
            mesh[i].GetComponent<Renderer>().material.shader = shader2;
            if (width > 0.03f) width = 0.002f;
            mesh[i].GetComponent<Renderer>().material.SetFloat("_Outline", width + 0.01f);
            width = width + 0.01f;
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("finished"); ;
        nt.stopKinect();
    }
}