using System;
using System.Collections.Generic;
using System.Numerics;

namespace Syntheticdata
{
    class Program
    {
        //maybe not used icl
        static double xpos;
        static double ypos;
        static double zpos;

        static void GenerateData()
        {
            Random rand = new Random();
            xpos = rand.Next(0, 100);
            ypos = rand.Next(0, 100);
            zpos = rand.Next(0, 100);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("--- GOLF LAUNCH MONITOR SIMULATION ---");

            // fake da cams like 15cm apart
            Camera camLeft = new Camera("Left", -75, 1000);
            Camera camRight = new Camera("Right", 75, 1000);

            // fake the ball and its data
            Vector3 ballPos = new Vector3(0, 0, 500); // 500 from the camera
            Vector3 velocity = new Vector3(2000, 10000, 50000); // 2k right / 10k up / 50k forward
            double gravity = -9800; // mm/s squard
            double timeStep = 0.01; // 10ms per frame (sum like <100fps)

            for (int frame = 0; frame < 20; frame++)
            {
                // move the ball
                ballPos.X += (float)(velocity.X * timeStep);
                ballPos.Y += (float)(velocity.Y * timeStep);
                ballPos.Z += (float)(velocity.Z * timeStep);
                velocity.Y += (float)(gravity * timeStep);

                // what the cam sees..... maybe
                var leftView = camLeft.ProjectPoint(ballPos);
                var rightView = camRight.ProjectPoint(ballPos);

                Console.WriteLine($"Frame {frame}:");
                Console.WriteLine($"   REAL 3D Pos:   X={ballPos.X:0.0}, Y={ballPos.Y:0.0}, Z={ballPos.Z:0.0}");
                Console.WriteLine($"   CAM LEFT sees:  Pixel ({leftView.u}, {leftView.v})");
                Console.WriteLine($"   CAM RIGHT sees: Pixel ({rightView.u}, {rightView.v})");
                Console.WriteLine($"   DISPARITY:      {Math.Abs(leftView.u - rightView.u)} pixels");
                Console.WriteLine("---------------------------------------------");
            }

            Console.WriteLine("Simulation Complete. Press Enter to exit.");
            Console.ReadLine();
        }
    }

    public class Camera
    {
        public string Cam;
        public Vector3 Position;
        public double FocalLength;

        public Camera(string camName, double x, double focal)
        {
            Cam = camName;
            // Vector3 uses floats, so cast the double x
            Position = new Vector3((float)x, 0, 0);
            this.FocalLength = focal;
        }

        // turn 3d pos to 2d
        public (int u, int v) ProjectPoint(Vector3 ballPos)
        {
            // ball from cam 
            double rposX = ballPos.X - Position.X;
            double rposY = ballPos.Y - Position.Y;
            double rposZ = ballPos.Z - Position.Z;

            // Prevent divide by zero if ball is behind cam
            if (rposZ <= 0) return (-1, -1);

            // center the cam on a screen like 1280x720
            int u = (int)((FocalLength * (rposX / rposZ)) + 640);
            int v = (int)(360 - (FocalLength * (rposY / rposZ))); // invert y for the screen i think

            return (u, v);
        }
    }
}