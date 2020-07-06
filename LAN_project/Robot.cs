using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace LAN_project
{
    class Robot
    {
        #region Math
        public static float cos(float x) => (float)Math.Cos(x);
        public static float sin(float x) => (float)Math.Sin(x);
        public static float PI => (float)Math.PI;
        public static float sqrt(float x) => (float)Math.Sqrt(x);
        public static float sqr(float x) => (float)Math.Pow(x, 2);
        public static float atan2(float y,float x) => (float)Math.Atan2(y, x);
        public static float atan(float x) => (float)Math.Atan(x);
        public static float asin(float x) => (float)Math.Asin(x);
        #endregion
        private List<ObjVolume> Base;
        private List<ObjVolume> Link1;
        private List<ObjVolume> Link2;
        private List<ObjVolume> Link3;
        private List<ObjVolume> Link4;
        private List<ObjVolume> Link5;
        public List<Volume> objects = new List<Volume>();
        public Vector4[] dh_parameter = new Vector4[7];
        public Robot(String[] input_obj)
        {
            dh_parameter[0] = new Vector4(0f, 0f, 0f, 0f);
            dh_parameter[1] = new Vector4(0f, 16.81f, 0f, PI / 2);
            dh_parameter[2] = new Vector4(PI / 2, 0f, 16.5f, 0f);
            dh_parameter[3] = new Vector4(0f, 0f, 0f, PI / 2);
            dh_parameter[4] = new Vector4(0f, 16.5f, 0f, -PI / 2);
            dh_parameter[5] = new Vector4(0f, 0f, 0f, PI / 2);
            dh_parameter[6] = new Vector4(0f, 39.5f, 0f, 0f);
            Base = ObjVolume.LoadFromFile_mul_mtl(input_obj[0]);
            Link1 = ObjVolume.LoadFromFile_mul_mtl(input_obj[1]);
            Link2 = ObjVolume.LoadFromFile_mul_mtl(input_obj[2]);
            Link3 = ObjVolume.LoadFromFile_mul_mtl(input_obj[3]);
            Link4 = ObjVolume.LoadFromFile_mul_mtl(input_obj[4]);
            Link5 = ObjVolume.LoadFromFile_mul_mtl(input_obj[5]);
            #region import
            foreach (ObjVolume obj in Base)
            {
                obj.attribure = 0;
                objects.Add(obj);
            }
            foreach (ObjVolume obj in Link1)
            {
                obj.attribure = 1;
                objects.Add(obj);
            }
            foreach (ObjVolume obj in Link2)
            {
                obj.attribure = 2;
                objects.Add(obj);
            }
            foreach (ObjVolume obj in Link3)
            {
                obj.attribure = 3;
                objects.Add(obj);
            }
            foreach (ObjVolume obj in Link4)
            {
                obj.attribure = 4;
                objects.Add(obj);
            }
            foreach (ObjVolume obj in Link5)
            {
                obj.attribure = 5;
                objects.Add(obj);
            }
            #endregion
        }
        public static float[] inverse(Vector3 p, Vector3 rotation)
        {
            float[] output = new float[6];
            float r11, r12, r13, r21, r22, r23, r31, r32, r33, Pxy, P, K, phi, cphi1, d6, d1, si, ci;
            float A = 1f, B, C;
            Matrix3 r = Matrix3.CreateRotationX(rotation.X) * Matrix3.CreateRotationY(rotation.Y) * Matrix3.CreateRotationZ(rotation.Z);
            r11 = r.Column0.X; r12 = r.Column0.Y; r13 = r.Column0.Z;
            r21 = r.Column1.X; r22 = r.Column1.Y; r23 = r.Column1.Z;
            r31 = r.Column2.X; r32 = r.Column2.Y; r33 = r.Column2.Z;
            Pxy = sqrt(sqr(p.X) + sqr(p.Y));
            P = sqrt(sqr(p.X) + sqr(p.Y) + sqr(p.Z));
            phi = atan2(p.Y, p.X);
            d6 = 3.95f; d1 = 16.81f;
            K = (sqr(d1) - 2f * p.Z * d1 - sqr(d6) + sqr(P)) / 33 + 16.5f;
            output[0] = atan2(p.Y - d6 * r23, p.X - d6 * r13);
            cphi1 = cos(phi - output[0]);
            B = -(4 * cphi1 * Pxy) / 33;
            C = 4 * sqr(d6) * sqr(r33) / 1089 + 4 * K / 33 + 8 * d1 * p.Z / 1089 - 4 * sqr(d1) / 1089 - 4 * sqr(p.Z) / 1089 - 2;
            si = (p.Z - d6 * r33 - d1) / 16.5f;
            ci = (-B - sqrt(sqr(B) - 4 * A * C)) / (2 * A);
            output[2] = asin((sqr(si) + sqr(ci) - 2) / 2);
            A = 2 * ci; B = 2 * si; C = sqr(ci) + sqr(si);
            output[1] = atan2(A, -B) - atan2(C, sqrt(sqr(A) + sqr(B) - sqr(C)));
            output[3] = atan((r13 * sin(output[0]) - r23 * cos(output[0])) / (r13 * cos(output[1] + output[2]) * cos(output[0]) + r23 * cos(output[1] + output[2]) * sin(output[0]) + r33 * sin(output[1] + output[2])));
            output[4] = atan2(r13 * (sin(output[0]) * sin(output[3]) + cos(output[0]) * cos(output[3]) * cos(output[1] + output[2])) + r23 * (sin(output[0]) * cos(output[3]) * cos(output[1] + output[2]) - cos(output[0]) * sin(output[3])) + r33 * sin(output[1] + output[2]) * cos(output[3]),
                              r13 * sin(output[1] + output[2]) * cos(output[0]) + r23 * sin(output[1] + output[2]) * sin(output[0]) - r33 * cos(output[1] + output[2]));
            output[5] = atan2(r11 * (sin(output[0]) * cos(output[3]) - cos(output[0]) * sin(output[3]) * cos(output[1] + output[2])) - r21 * (sin(output[0]) * sin(output[3]) * cos(output[1] + output[2]) + cos(output[0]) * cos(output[3])) - r31 * sin(output[1] + output[2]) * sin(output[3]),
                              r12 * (sin(output[0]) * cos(output[3]) - cos(output[0]) * sin(output[3]) * cos(output[1] + output[2])) - r22 * (sin(output[0]) * sin(output[3]) * cos(output[1] + output[2]) + cos(output[0]) * cos(output[3])) - r32 * sin(output[1] + output[2]) * sin(output[3]));
            return output;
        }
 
        private Matrix4 dh(Vector4 dh_index)
        {
            return Matrix4.CreateRotationX(dh_index.W) * Matrix4.CreateTranslation(dh_index.Z, 0f, dh_index.Y) * Matrix4.CreateRotationZ(dh_index.X);
        }
    }
}
