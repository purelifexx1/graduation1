using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
namespace LAN_project
{
    class create_world: Volume
    {
        List<Volume> objects = new List<Volume>();
        Dictionary<String, Material> materials = new Dictionary<string, Material>();
        public List<Vector3> Normals = new List<Vector3>();
        public List<Vector3> Verts = new List<Vector3>();
        public int vert_length;
        public int[] Inds;
        
        private List<Tuple<FaceVertex, FaceVertex, FaceVertex>> faces = new List<Tuple<FaceVertex, FaceVertex, FaceVertex>>();
        public override int VertCount { get { return vert_length; } }
        public override int IndiceCount { get { return vert_length * 3; } }
        public override int ColorDataCount { get { return 1; } }
        public override Vector3[] GetVerts()
        {                      
            return Verts.ToArray();
        }
        public override int[] GetIndices(int offset = 0)
        {
            if (offset != 0)
            {
                for (int i = 0; i < Inds.Length; i++)
                {
                    Inds[i] += offset;
                }
            }
            return Inds;
        }
        public override Vector3[] GetNormals()
        {         
            return Normals.ToArray();
        }
        public override Vector3[] GetColorData()
        {
            return new Vector3[ColorDataCount];
        }

        public override Vector2[] GetTextureCoords()
        {
            List<Vector2> coords = new List<Vector2>();

            foreach (var face in faces)
            {
                coords.Add(face.Item1.TextureCoord);
                coords.Add(face.Item2.TextureCoord);
                coords.Add(face.Item3.TextureCoord);
            }

            return coords.ToArray();
        }
        public override void CalculateModelMatrix()
        {
            ModelMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateTranslation(Position);
        }

        public create_world(int[] center, int Length)
        {
            byte num = 0;
            for(; num <= 20; num++)
            {
                square base_line = new square();
                objects.Add(base_line);
                objects[num].Position = new Vector3(center[0], (float)((center[1] - Length / 2) + num * (Length / 20)), 0f);
                objects[num].Scale = new Vector3((float)Length, (float)Length * 0.005f, 0.1f);                             
            }
            for (; num <= 41; num++)
            {
                square base_line = new square();
                objects.Add(base_line);
                objects[num].Position = new Vector3((float)((center[0] - Length / 2) + (num - 21) * (Length / 20)), center[1], 0f);
                objects[num].Scale = new Vector3((float)Length * 0.005f, (float)Length, 0.1f);
            }

            List<Vector3> verts = new List<Vector3>();
            List<int> inds = new List<int>();
            List<Vector3> normals = new List<Vector3>();
            int vertcount = 0;
            foreach (Volume v in objects)
            {
                v.CalculateModelMatrix();
                Matrix4 calculate = new Matrix4(new Vector4(v.GetVerts()[0], 1f), new Vector4(v.GetVerts()[1], 1f), new Vector4(v.GetVerts()[2], 1f), new Vector4(v.GetVerts()[3], 1f));
                Matrix4 final = Matrix4.Mult(calculate, v.ModelMatrix);
                verts.Add(final.Row0.Xyz);
                verts.Add(final.Row1.Xyz);
                verts.Add(final.Row2.Xyz);
                verts.Add(final.Row3.Xyz);
                inds.AddRange(v.GetIndices(vertcount).ToList());
                normals.AddRange(v.GetNormals().ToList());
                vertcount += v.VertCount;
            }
            vert_length = vertcount;
            Verts = verts;
            Inds = inds.ToArray();
            Normals = normals;
        }
    }
}
