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
    public class square : Volume
    {
        public square()
        {
            VertCount = 4;
            IndiceCount = 12;
            ColorDataCount = 4;
            TextureCoordsCount = 4;
        }
        public override Vector3[] GetVerts()
        {
            return new Vector3[] {new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f)
            };
        }
        public override Vector3[] GetNormals()
        {
            return new Vector3[] {new Vector3(0f, 0f, 1f),
                new Vector3(0f, 0f, 1f),
                new Vector3(0f, 0f, 1f),
                new Vector3(0f, 0f, 1f)
            };
        }

        public override Vector2[] GetTextureCoords()
        {
            return new Vector2[] {
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(-1.0f, 0.0f),
                new Vector2(-1.0f, 1.0f),
                };
        }
        public override int[] GetIndices(int offset = 0)
        {
            int[] inds = new int[] {
                0, 1, 2,
                0, 2, 3,
                1, 2, 3,
                3, 1, 0
            };

            if (offset != 0)
            {
                for (int i = 0; i < inds.Length; i++)
                {
                    inds[i] += offset;
                }
            }
            return inds;
        }
        public override Vector3[] GetColorData()
        {
            Vector3[] out_color = new Vector3[4];
            return out_color;
        }

        public override void CalculateModelMatrix()
        {
            ModelMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateTranslation(Position);
        }
    
    }
}
