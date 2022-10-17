using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using System.Threading.Tasks;
using UnityEngine.UI;

public class Display : MonoBehaviour
{
    //Variable for handling Kinect
    Device kinect;
    //Width and Height of Depth image.
    int depthWidth;
    int depthHeight;
    int rgbWidth;
    int rgbHeight;
    //Number of all points of PointCloud 
    int numRgb;
    int numDepth;
    //Array of colors corresponding to each point in PointCloud
    Color32[] colorsRgb;
    Color32[] colorsDepth;
    //Raw Image to display
    RawImage rawImageRgb;
    RawImage rawImageDepth;
    //Color image to be attatched to mesh
    Texture2D textureRgb;
    Texture2D textureDepth;
    //Class for coordinate transformation(e.g.Color-to-depth, depth-to-xyz, etc.)
    Transformation transformation;

    //Stop Kinect as soon as this object disappear
    private void OnDestroy()
    {
        kinect.StopCameras();
    }

    void Start()
    {
        //The method to initialize Kinect
        InitKinect();
        //Initialization for colored mesh rendering
        InitTexture();
        //Loop to get data from Kinect and rendering
        Task t = KinectLoop(kinect);
    }


    //Initialization of Kinect
    void InitKinect()
    {
        //Connect with the 0th Kinect
        kinect = Device.Open(0);
        //Setting the Kinect operation mode and starting it
        kinect.StartCameras(new DeviceConfiguration
        {
            ColorFormat = ImageFormat.ColorBGRA32,
            ColorResolution = ColorResolution.R720p,
            DepthMode = DepthMode.NFOV_2x2Binned,
            SynchronizedImagesOnly = true,
            CameraFPS = FPS.FPS30,
        });
        //Access to coordinate transformation information
        transformation = kinect.GetCalibration().CreateTransformation();

    }

    //Prepare to draw colored mesh
    void InitTexture()
    {
        //Display Image
        rawImageRgb = GameObject.Find("Display rgb").GetComponent<RawImage>();
        rawImageDepth = GameObject.Find("Display depth").GetComponent<RawImage>();

        //Get the width and height of the Depth image and calculate the number of all points
        depthWidth = kinect.GetCalibration().DepthCameraCalibration.ResolutionWidth;
        depthHeight = kinect.GetCalibration().DepthCameraCalibration.ResolutionHeight;
        rgbWidth = kinect.GetCalibration().ColorCameraCalibration.ResolutionWidth;
        rgbHeight = kinect.GetCalibration().ColorCameraCalibration.ResolutionHeight;
        numRgb = rgbWidth * rgbHeight;
        numDepth = depthWidth * depthHeight;


        //Allocation of vertex and color storage space for the total number of pixels in the depth image
        colorsRgb = new Color32[numRgb];
        textureRgb = new Texture2D(rgbWidth, rgbHeight);

        colorsDepth = new Color32[numDepth];
        textureDepth = new Texture2D(depthWidth, depthHeight);

        //rawImage.uvRect = new Rect(0, 0, rgbWidth, rgbHeight);
        rawImageRgb.texture = textureRgb;
        rawImageDepth.texture = textureDepth;
    }

    public Microsoft.Azure.Kinect.Sensor.Image[] encode(Microsoft.Azure.Kinect.Sensor.Image modifiedColor)
    {
        Microsoft.Azure.Kinect.Sensor.Image DepthImage = new Microsoft.Azure.Kinect.Sensor.Image(ImageFormat.ColorBGRA32, depthWidth, depthHeight);
    }

    private async Task KinectLoop(Device device)
    {

        while (true)
        {
            using (Capture capture = await Task.Run(() => device.GetCapture()).ConfigureAwait(true))
            {
                //Getting color informatio
                Microsoft.Azure.Kinect.Sensor.Image modifiedColor = capture.Depth;
                modifiedColor = encode(modifiedColor);
                BGRA[] depthArray = encode(modifiedColor);

                //Getting the rgb image
                Microsoft.Azure.Kinect.Sensor.Image RgbImage = capture.Color;
                BGRA[] colorArray = RgbImage.GetPixels<BGRA>().ToArray();

               

                int pointIndex1 = 0;

                for (int y = 0; y < rgbHeight; y++)
                {
                    for (int x = 0; x < rgbWidth; x++)
                    {

                        colorsRgb[pointIndex1].a = 255;
                        colorsRgb[pointIndex1].b = colorArray[pointIndex1].B;
                        colorsRgb[pointIndex1].g = colorArray[pointIndex1].G;
                        colorsRgb[pointIndex1].r = colorArray[pointIndex1].R;

                        pointIndex1++;
                    }
                }

                int pointIndex2 = 0;

                for (int y = 0; y < depthHeight; y++)
                {
                    for (int x = 0; x < depthWidth; x++)
                    {

                        colorsDepth[pointIndex2].a = 255;
                        colorsDepth[pointIndex2].b = depthArray[pointIndex2].B;
                        colorsDepth[pointIndex2].g = depthArray[pointIndex2].G;
                        colorsDepth[pointIndex2].r = depthArray[pointIndex2].R;

                        pointIndex2++;
                    }
                }


                textureRgb.SetPixels32(colorsRgb);
                textureRgb.Apply();

                textureDepth.SetPixels32(colorsDepth);
                textureDepth.Apply();

            }
        }
    }

}
