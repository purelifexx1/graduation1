using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace LAN_project
{
    class Camera
    {
        public Vector3 Position = new Vector3(50f, -50f, 45f);
        public Vector3 Orientation = new Vector3(3f * (float)Math.PI / 4f, 0f, 0f);
        public float MoveSpeed = 0.6f;
        public float MouseSensitivity = 0.0055f;
        public Vector3 lookat = new Vector3();
        public Matrix4 GetViewMatrix()
        {

            lookat.Y = (float)(Math.Sin((float)Orientation.X) * Math.Cos((float)Orientation.Z));
            lookat.Z = (float)Math.Sin((float)Orientation.Z);
            lookat.X = (float)(Math.Cos((float)Orientation.X) * Math.Cos((float)Orientation.Z));

            return Matrix4.LookAt(Position, Position + lookat, Vector3.UnitZ);
        }

        public void Move(float x, float y, float z)
        {
            Vector3 offset = new Vector3();

            Vector3 forward = new Vector3((float)Math.Cos((float)Orientation.X), (float)Math.Sin((float)Orientation.X), 0f);
            Vector3 right = new Vector3(forward.Y, -forward.X, 0f);

            offset += x * forward;
            offset += y * right;
            offset.Z += z;

            offset.NormalizeFast();
            offset = Vector3.Multiply(offset, MoveSpeed);

            Position += offset;
        }

        public void AddRotation(float x, float y)
        {
            x = x * MouseSensitivity;
            y = y * MouseSensitivity;

            Orientation.X = (Orientation.X + x) % ((float)Math.PI * 2.0f);
            Orientation.Z = Math.Max(Math.Min(Orientation.Z + y, (float)Math.PI / 2.0f - 0.05f), (float)-Math.PI / 2.0f + 0.05f);
        }
    }
}
