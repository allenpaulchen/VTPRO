// Kinect for Windows SDK Header
// Standard Library
#define _CRT_SECURE_NO_WARNINGS
#include <WinSock2.h>
#include <iostream>
#include <Kinect.h>
#include <opencv2/core.hpp>
#include <opencv2/highgui.hpp>
#include"socketClient.h"

using namespace std;

// output operator for CameraSpacePoint
ostream& operator<<(ostream& rOS, const CameraSpacePoint& rPos)
{
	rOS << "(" << rPos.X << "/" << rPos.Y << "/" << rPos.Z << ")";
	return rOS;
}

// output operator for Vector4
ostream& operator<<(ostream& rOS, const Vector4& rVec)
{
	rOS << "(" << rVec.x << "/" << rVec.y << "/" << rVec.z << "/" << rVec.w << ")";
	return rOS;
}

char keyboard = ' ';
int main(int argc, char** argv)
{
	/////////socket連線
	ConnectToUnity();
	// 1a. Get default Sensor
	cout << "Try to get default sensor" << endl;
	IKinectSensor* pSensor = nullptr;
	if (GetDefaultKinectSensor(&pSensor) != S_OK)
	{
		cerr << "Get Sensor failed" << endl;
		return -1;
	}

	// 1b. Open sensor
	cout << "Try to open sensor" << endl;
	if (pSensor->Open() != S_OK)
	{
		cerr << "Can't open sensor" << endl;
		return -1;
	}

	// 2a. Get frame source
	cout << "Try to get body source" << endl;
	IBodyFrameSource* pFrameSource = nullptr;
	if (pSensor->get_BodyFrameSource(&pFrameSource) != S_OK)
	{
		cerr << "Can't get body frame source" << endl;
		return -1;
	}
	//F---------------------------------------------------
	// Get Show source
	IBodyIndexFrameSource* pFrameSourceForShow = nullptr;
	if (pSensor->get_BodyIndexFrameSource(&pFrameSourceForShow) != S_OK)
	{
		cerr << "抓不到" << endl;
		return -1;
	}

	// 2b. Get frame description
	int        iWidth = 0;
	int        iHeight = 0;
	IFrameDescription* pFrameDescription = nullptr;
	pFrameSourceForShow->get_FrameDescription(&pFrameDescription);
	pFrameDescription->get_Width(&iWidth);
	pFrameDescription->get_Height(&iHeight);
	pFrameDescription->Release();
	pFrameDescription = nullptr;

	// 3a. get frame reader
	IBodyIndexFrameReader* pFrameReader_Show = nullptr;
	pFrameSourceForShow->OpenReader(&pFrameReader_Show);

	// 2c. release Frame source
	cout << "Release frame source" << endl;
	pFrameSourceForShow->Release();
	pFrameSourceForShow = nullptr;

	// Prepare OpenCV data
	cv::Mat mImg(iHeight, iWidth, CV_8UC3);
	cv::namedWindow("Body Index Image");
	//---------------------------------------------------

	// 2b. Get the number of body
	INT32 iBodyCount = 0;
	if (pFrameSource->get_BodyCount(&iBodyCount) != S_OK)
	{
		cerr << "Can't get body count" << endl;
		return -1;
	}
	cout << " > Can trace " << iBodyCount << " bodies" << endl;
	IBody** aBody = new IBody*[iBodyCount];
	for (int i = 0; i < iBodyCount; ++i)
		aBody[i] = nullptr;

	// 3a. get frame reader
	cout << "Try to get body frame reader" << endl;
	IBodyFrameReader* pFrameReader = nullptr;
	if (pFrameSource->OpenReader(&pFrameReader) != S_OK)
	{
		cerr << "Can't get body frame reader" << endl;
		return -1;
	}
	// 2b. release Frame source
	cout << "Release frame source" << endl;
	pFrameSource->Release();
	pFrameSource = nullptr;


	char *headP;
	float smOrz;
	float smx;	float smy;	float smz;
	float srx;	float sry;	float srz;
	float slx;	float sly;	float slz;
	// Enter main loop
	while (keyboard != 'q')
	{
		//F---------------------------------------------------
		IBodyIndexFrame* pFrame_Show = nullptr;
		if (pFrameReader_Show->AcquireLatestFrame(&pFrame_Show) == S_OK)
		{
			// 4c. Fill OpenCV image
			UINT uSize = 0;
			BYTE* pBuffer = nullptr;
			pFrame_Show->AccessUnderlyingBuffer(&uSize, &pBuffer);
			for (int y = 0; y < iHeight; ++y)
			{
				for (int x = 0; x < iWidth; ++x)
				{
					int uBodyIdx = pBuffer[x + y * iWidth];
					if (uBodyIdx < 6){
						//cout << "有人在"<< endl;
						mImg.at<cv::Vec3b>(y, x) = cv::Vec3b(255, 0, 0);
					}
					else
						mImg.at<cv::Vec3b>(y, x) = cv::Vec3b(0, 0, 0);
				}
			}
			cv::imshow("Body Index Image", mImg);

			// 4e. release frame
			pFrame_Show->Release();
		}
		//---------------------------------------------------

		// 4a. Get last frame
		IBodyFrame* pFrame = nullptr;
		if (pFrameReader->AcquireLatestFrame(&pFrame) == S_OK)
		{
			// 4b. get Body data
			if (pFrame->GetAndRefreshBodyData(iBodyCount, aBody) == S_OK)
			{
				int iTrackedBodyCount = 0;

				// 4c. for each body
				for (int i = 0; i < iBodyCount; ++i)
				{
					IBody* pBody = aBody[i];

					// check if is tracked
					BOOLEAN bTracked = false;
					if ((pBody->get_IsTracked(&bTracked) == S_OK) && bTracked)
					{
						++iTrackedBodyCount;
						cout << "User " << i << " is under tracking" << endl;

						// get joint 位置 position
						Joint aJoints[JointType::JointType_Count];
						if (pBody->GetJoints(JointType::JointType_Count, aJoints) != S_OK)
						{
							cerr << "Get joints fail" << endl;
						}

						// get joint 方向 orientation 
						JointOrientation aOrientations[JointType::JointType_Count];
						if (pBody->GetJointOrientations(JointType::JointType_Count, aOrientations) != S_OK)
						{
							cerr << "Get joints fail" << endl;
						}

						// output information
						JointType slJointType = JointType::JointType_ShoulderLeft;  //ShoulderLeft
						const Joint& slJointPos = aJoints[slJointType];
						const JointOrientation& slJointOri = aOrientations[slJointType];

						JointType srJointType = JointType::JointType_ShoulderRight;  //ShoulderRight
						const Joint& srJointPos = aJoints[srJointType];
						const JointOrientation& srJointOri = aOrientations[srJointType];

						JointType smJointType = JointType::JointType_SpineMid;  //SpineMid
						const Joint& smJointPos = aJoints[smJointType];
						const JointOrientation& smJointOri = aOrientations[smJointType];

						//SpineMid XYZ
						cout << " > SpineMid is ";
						if (smJointPos.TrackingState == TrackingState_NotTracked)
						{
							cout << "not tracked" << endl;
						}
						else
						{
							if (smJointPos.TrackingState == TrackingState_Inferred)
							{
								cout << "inferred ";
							}
							else if (smJointPos.TrackingState == TrackingState_Tracked)
							{
								cout << "tracked ";
							}

							smx = smJointPos.Position.X;	smy = smJointPos.Position.Y;	smz = smJointPos.Position.Z;//修正測試過
							smOrz = smJointOri.Orientation.z;
							cout << "at " << smJointPos.Position << ",\n\t orientation: " << smJointOri.Orientation << endl;
						}

						//Right Shoulder XYZ
						cout << " > Right Shoulder is ";
						if (srJointPos.TrackingState == TrackingState_NotTracked)
						{
							cout << "not tracked" << endl;
						}
						else
						{
							if (srJointPos.TrackingState == TrackingState_Inferred)
							{
								cout << "inferred ";
							}
							else if (srJointPos.TrackingState == TrackingState_Tracked)
							{
								cout << "tracked ";
							}
							srx = srJointPos.Position.X;	sry = srJointPos.Position.Y;	srz = srJointPos.Position.Z;
							cout << "at " << srJointPos.Position << ",\n\t orientation: " << srJointOri.Orientation << endl;
						}

						//Left Shoulder XYZ
						cout << " > Left Shoulder is ";
						if (slJointPos.TrackingState == TrackingState_NotTracked)
						{
							cout << "not tracked" << endl;
						}
						else
						{
							if (slJointPos.TrackingState == TrackingState_Inferred)
							{
								cout << "inferred ";
							}
							else if (slJointPos.TrackingState == TrackingState_Tracked)
							{
								cout << "tracked ";
							}
							slx = slJointPos.Position.X;	sly = slJointPos.Position.Y;	slz = slJointPos.Position.Z;
							cout << "at " << slJointPos.Position << ",\n\t orientation: " << slJointOri.Orientation << endl;
						}

						//pass value to Unity
						stringstream ss;
						string p;
						//smOrz is edit from srx

						ss << smx << 'a' << smy << 'b' << smz << 'c' << smOrz << 'd' << sry << 'e' << srz << 'z' << slx << 'g' << sly << 'h' << slz << 'i';
						ss >> p;
						cout << p << endl;
						if (slJointPos.TrackingState != TrackingState_NotTracked){
							headP = new char[p.length() + 1];
							strcpy(headP, p.c_str());
							PassToUnity(headP);
						}

					}
				}

				if (iTrackedBodyCount > 0)
					cout << "Total " << iTrackedBodyCount << " bodies in this time\n" << endl;
			}
			else
			{
				cerr << "Can't read body data" << endl;
			}

			// 4e. release frame
			pFrame->Release();
		}
	}

	// delete body data array
	delete[] aBody;

	// 3b. release frame reader
	cout << "Release frame reader" << endl;
	pFrameReader->Release();
	pFrameReader = nullptr;
	pFrameReader_Show->Release();
	pFrameReader_Show = nullptr;

	// 1c. Close Sensor
	cout << "close sensor" << endl;
	pSensor->Close();

	// 1d. Release Sensor
	cout << "Release sensor" << endl;
	pSensor->Release();
	pSensor = nullptr;

	return 0;
}