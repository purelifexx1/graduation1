using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

namespace LAN_project
{
    public partial class Form1 : Form
    {
        #region declare
        public Int32 Abs(Int32 x) => (Int32)Math.Abs(x);
        public float PI => (float)Math.PI;
        MotoComUDP MotoComUDP;
        Camera cam = new Camera();
        float[] theta = new float[7];
        bool enter = false;
        int ibo_elements;
        int[] indicedata;
        Vector2 lastMousePos = new Vector2();
        Vector3[] vertdata;
        Vector3[] normdata;
        //Vector3[] coldata;
        //Vector2[] texcoorddata;
        List<Volume> objects = new List<Volume>();
        Robot motoman_mini = new Robot(new string[]{"base.obj", "link1.obj", "link2.obj", "link3.obj", "link4.obj", "link5.obj"});
        create_world World = new create_world(new int[2] { 0, 0 }, 100);
        Dictionary<string, ShaderProgram> shaders = new Dictionary<string, ShaderProgram>();
        Dictionary<String, Material> materials = new Dictionary<string, Material>();
        string activeShader = "default";
        Light activeLight = new Light(new Vector3(), new Vector3(0.55f, 0.55f, 0.55f));
        Matrix4[] DH_link = new Matrix4[6];
        Matrix4 Perspective = new Matrix4();
        byte pointer = 0;
        Int32[] pulse_des = new Int32[6];
        Int32[] pulse_current = new Int32[6];
        Int32[] pulse_error = new Int32[6];
        float a1, a2, a3, a4, a5, a6;
        bool inc_status = false, dec_status = false;
        List<float[]> list_point_cartesian = new List<float[]>();
        List<Int32[]> list_point_pulse = new List<Int32[]>();
        #endregion
        private void initProgram()
        {
            activeShader = "lit";
            shaders.Add(activeShader, new ShaderProgram("shader/vs_lit.glsl", "shader/fs_lit.glsl", true));

            loadMaterials("material/personal_mtl.mtl");

            GL.UseProgram(shaders[activeShader].ProgramID);
            GL.GenBuffers(1, out ibo_elements);
            lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
            Robot.inverse(new Vector3(15.88f, 10.47f, 15.54f), new Vector3(PI, -PI / 4, 0f));
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button2.Enabled = true;
            tab1.Enabled = false;
            Perspective = Matrix4.CreatePerspectiveFieldOfView(1.3f, glControl1.Width / glControl1.Height, 1.0f, 200.0f);
            //MotoComUDP = new MotoComUDP(IPAddress.Parse(IP.Text), int.Parse(port.Text), int.Parse(local_port.Text));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (button1.Text == "connect")
                {
                    MotoComUDP = new MotoComUDP(IPAddress.Parse(IP.Text), int.Parse(port.Text), int.Parse(local_port.Text));
                    if (MotoComUDP.ConnectMotoman())
                    {
                        button1.Text = "disconnect";
                        button2.Enabled = true;
                        err.Text = "IP is valid";
                    }
                }
                else if (button1.Text == "disconnect")
                {
                    MotoComUDP.CloseMotoman();
                    button1.Text = "connect";
                    button2.Enabled = false;
                }
            }
            catch
            {
                err.Text = "IP is invalid";                    
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (button2.Text == "On Servo")
                {
                    if (MotoComUDP.TurnOnServo())
                    {
                        MotoComUDP.ReceiveDataThread();
                        receive.Text += MotoComUDP.ReceivedData + "\r\n";
                        String[] tempper = MotoComUDP.Get_feedback_staus();
                        status_code.Text = tempper[0];
                        added_statuscode.Text = tempper[1];
                        button2.Text = "Off Servo";
                        tab1.Enabled = true;
                    }
                }
                else if(button2.Text == "Off Servo")
                {
                    if (MotoComUDP.TurnOffServo())
                    {
                        MotoComUDP.ReceiveDataThread();
                        receive.Text += MotoComUDP.ReceivedData + "\r\n";
                        String[] tempper = MotoComUDP.Get_feedback_staus();
                        status_code.Text = tempper[0];
                        added_statuscode.Text = tempper[1];
                        button2.Text = "On Servo";
                        tab1.Enabled = false;
                    }
                }
            }
            catch
            {
                
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MotoComUDP.GetPosition();
            MotoComUDP.ReceiveDataThread();
            receive.Text += MotoComUDP.ReceivedData + "\r\n";
            String[] tempper = MotoComUDP.Get_feedback_staus();
            status_code.Text = tempper[0];
            added_statuscode.Text = tempper[1];
            byte[] data = MotoComUDP.ReceiveBytes;
            X.Text = Convert.ToString(Convert.ToDouble(MotoComUDP.ByteArrayToInt32(MotoComUDP.SubArray(data, 52, 4))) / 1000);
            Y.Text = Convert.ToString(Convert.ToDouble(MotoComUDP.ByteArrayToInt32(MotoComUDP.SubArray(data, 56, 4))) / 1000);
            Z.Text = Convert.ToString(Convert.ToDouble(MotoComUDP.ByteArrayToInt32(MotoComUDP.SubArray(data, 60, 4))) / 1000);

            Rx.Text = Convert.ToString(Convert.ToDouble(MotoComUDP.ByteArrayToInt32(MotoComUDP.SubArray(data, 64, 4))) / 10000);
            Ry.Text = Convert.ToString(Convert.ToDouble(MotoComUDP.ByteArrayToInt32(MotoComUDP.SubArray(data, 68, 4))) / 10000);
            Rz.Text = Convert.ToString(Convert.ToDouble(MotoComUDP.ByteArrayToInt32(MotoComUDP.SubArray(data, 72, 4))) / 10000);

        }
        #region GRAPHICS
        private void glControl1_Load(object sender, EventArgs e)
        {
            initProgram();
            World.Material = materials["red"];
            objects.Add(World);
            objects.AddRange(motoman_mini.objects);
            
            GL.ClearColor(0.894f, 0.925f, 0.949f, 0.8f);
            GL.PointSize(5f);
            buffer_process();
        }

        private void buffer_process()
        {
            List<Vector3> verts = new List<Vector3>();
            List<int> inds = new List<int>();
            List<Vector3> colors = new List<Vector3>();
            List<Vector2> texcoords = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();

            int vertcount = 0;
            foreach (Volume v in objects)
            {
                verts.AddRange(v.GetVerts().ToList());
                inds.AddRange(v.GetIndices(vertcount).ToList());
                //colors.AddRange(v.GetColorData().ToList());
                //texcoords.AddRange(v.GetTextureCoords());
                normals.AddRange(v.GetNormals().ToList());
                vertcount += v.VertCount;   
            }

            vertdata = verts.ToArray();
            indicedata = inds.ToArray();
            //coldata = colors.ToArray();
            //texcoorddata = texcoords.ToArray();
            normdata = normals.ToArray();


            GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vPosition"));
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StaticDraw);

            /*if (shaders[activeShader].GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vColor"));
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }*/

            /*if (shaders[activeShader].GetAttribute("texcoord") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("texcoord"));
                GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(texcoorddata.Length * Vector2.SizeInBytes), texcoorddata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("texcoord"), 2, VertexAttribPointerType.Float, true, 0, 0);
            }*/

            if (shaders[activeShader].GetAttribute("vNormal") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vNormal"));
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(normdata.Length * Vector3.SizeInBytes), normdata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vNormal"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }
        }

        private void glControl1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                enter = true;
            }
            else if (e.Button == MouseButtons.Right)
            {
                enter = false;
            }
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            OnUpdateFrame();
            //GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            shaders[activeShader].EnableVertexAttribArrays();
            int indiceat = 0;
            Matrix4 view = cam.GetViewMatrix();
            foreach (Volume v in objects)
            {
                //GL.BindTexture(TextureTarget.Texture2D, v.TextureID);

                GL.UniformMatrix4(shaders[activeShader].GetUniform("modelview"), false, ref v.ModelViewProjectionMatrix);

                /*if (shaders[activeShader].GetAttribute("maintexture") != -1)
                {
                    GL.Uniform1(shaders[activeShader].GetAttribute("maintexture"), v.TextureID);
                }*/

                if (shaders[activeShader].GetUniform("view") != -1)
                {
                    GL.UniformMatrix4(shaders[activeShader].GetUniform("view"), false, ref view);
                }

                if (shaders[activeShader].GetUniform("model") != -1)
                {
                    GL.UniformMatrix4(shaders[activeShader].GetUniform("model"), false, ref v.ModelMatrix);
                }

                if (shaders[activeShader].GetUniform("material_ambient") != -1)
                {
                    GL.Uniform3(shaders[activeShader].GetUniform("material_ambient"), ref v.Material.AmbientColor);
                }

                if (shaders[activeShader].GetUniform("material_diffuse") != -1)
                {
                    GL.Uniform3(shaders[activeShader].GetUniform("material_diffuse"), ref v.Material.DiffuseColor);
                }

                if (shaders[activeShader].GetUniform("material_specular") != -1)
                {
                    GL.Uniform3(shaders[activeShader].GetUniform("material_specular"), ref v.Material.SpecularColor);
                }

                if (shaders[activeShader].GetUniform("material_specExponent") != -1)
                {
                    GL.Uniform1(shaders[activeShader].GetUniform("material_specExponent"), v.Material.SpecularExponent);
                }

                if (shaders[activeShader].GetUniform("light_position") != -1)
                {
                    GL.Uniform3(shaders[activeShader].GetUniform("light_position"), ref cam.Position);
                }

                if (shaders[activeShader].GetUniform("light_color") != -1)
                {
                    GL.Uniform3(shaders[activeShader].GetUniform("light_color"), ref activeLight.Color);
                }

                if (shaders[activeShader].GetUniform("light_diffuseIntensity") != -1)
                {
                    GL.Uniform1(shaders[activeShader].GetUniform("light_diffuseIntensity"), activeLight.DiffuseIntensity);
                }

                if (shaders[activeShader].GetUniform("light_ambientIntensity") != -1)
                {
                    GL.Uniform1(shaders[activeShader].GetUniform("light_ambientIntensity"), activeLight.AmbientIntensity);
                }

                GL.DrawElements(BeginMode.Triangles, v.IndiceCount, DrawElementsType.UnsignedInt, indiceat * sizeof(uint));
                indiceat += v.IndiceCount;
            }
            shaders[activeShader].DisableVertexAttribArrays();

            GL.Flush();
            glControl1.SwapBuffers();
            lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
        }
        #endregion
        private void OnUpdateFrame()
        {           
            ProcessInput();
            Matrix4 tempp = Matrix4.Identity;
            if (mode_enable.Checked == false)
            {               
                theta[1] = a1;
                theta[2] = -a2 + PI / 2;
                theta[3] = a3;
                theta[4] = a4;
                theta[5] = -PI / 2 + a5;
                theta[6] = a6;
            }else if(mode_enable.Checked == true)
            {
                if(inc_status == true)
                {
                    theta[axis_select] += (float)manual_sp.Value / 100f;
                }else if(dec_status == true)
                {
                    theta[axis_select] -= (float)manual_sp.Value / 100f;
                }
            }
            
            for(byte inc = 0; inc <= 5; inc++)
            {
                DH_link[inc] = Matrix4.CreateScale(100f, 100f, 100f) * Matrix4.CreateRotationZ(theta[inc]) * tempp;
                motoman_mini.dh_parameter[inc].X = theta[inc];
                tempp = dh(motoman_mini.dh_parameter[inc]) * tempp;
            }
            tempp = dh(motoman_mini.dh_parameter[6]) * tempp;
            Matrix4 ViewProjectionMatrix = cam.GetViewMatrix() * Perspective;
            foreach (Volume v in objects)
            {
                if (v.attribure != 10) v.ModelMatrix = DH_link[v.attribure];
                else v.CalculateModelMatrix();
                v.ModelViewProjectionMatrix = v.ModelMatrix * ViewProjectionMatrix;
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            
        }

        private void ProcessInput()
        {
            if (Keyboard.GetState().IsKeyDown(Key.W))
            {
                //Vector3 m_direc = Vector3.Multiply(cam.lookat, 0.2f);
                //cam.Move((float)Math.Abs(m_direc.X), 0f, m_direc.Z);
                cam.Move(0.1f, 0f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.S))
            {
                //Vector3 m_direc =  Vector3.Multiply(cam.lookat, -0.2f);
                //cam.Move(-(float)Math.Abs(m_direc.X), 0f, m_direc.Z);
                cam.Move(-0.1f, 0f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.A))
            {
                cam.Move(0f, -0.1f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.D))
            {
                cam.Move(0f, 0.1f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.Q))
            {
                cam.Move(0f, 0f, 0.1f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.E))
            {
                cam.Move(0f, 0f, -0.1f);
            }
            if (enter)
            {
                Vector2 delta = lastMousePos - new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
                lastMousePos += delta;
                cam.AddRotation(delta.X, delta.Y);
                lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
            }
        }

        private Matrix4 dh(Vector4 dh_index)
        {
            return Matrix4.CreateRotationX(dh_index.W) * Matrix4.CreateTranslation(dh_index.Z, 0f, dh_index.Y) * Matrix4.CreateRotationZ(dh_index.X);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            glControl1.Refresh();
            glControl1.Invalidate();
        }
        #region LOAD_IMAGE_TEXTURE(NOT USED)
        /*private int loadImage(Bitmap image)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }
        private int loadImage(string filename)
        {
            try
            {
                Bitmap file = new Bitmap(filename);
                return loadImage(file);
            }
            catch (FileNotFoundException e)
            {
                return -1;
            }
        }*/
        #endregion
        private void button7_Click(object sender, EventArgs e)
        {
            timer2.Enabled = false;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            /*xm.Text = "184.877";
            ym.Text = "-202.567";
            zm.Text = "1.136";
            rxm.Text = "161.0248";
            rym.Text = "-8.7115";
            rzm.Text = "13.5610";*/
            t1_pulse.Text = t1.Text;
            t2_pulse.Text = t2.Text;
            t3_pulse.Text = t3.Text;
            t4_pulse.Text = t4.Text;
            t5_pulse.Text = t5.Text;
            t6_pulse.Text = t6.Text;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            MotoComUDP.Write_Read_Register(Convert.ToInt16(register.Text), Convert.ToInt32(re_value.Text));
            MotoComUDP.ReceiveDataThread();
            receive.Text += MotoComUDP.ReceivedData + "\r\n";
            String[] tempper = MotoComUDP.Get_feedback_staus();
            status_code.Text = tempper[0];
            added_statuscode.Text = tempper[1];
        }

        private void button12_Click(object sender, EventArgs e)
        {
            MotoComUDP.Write_Read_IO(Convert.ToInt16(IO.Text), Convert.ToInt32(IO_value.Text));
            MotoComUDP.ReceiveDataThread();
            receive.Text += MotoComUDP.ReceivedData + "\r\n";
            String[] tempper = MotoComUDP.Get_feedback_staus();
            status_code.Text = tempper[0];
            added_statuscode.Text = tempper[1];
        }

        private void button11_Click(object sender, EventArgs e)
        {
            MotoComUDP.Write_Read_Register(Convert.ToInt16(register.Text));
            MotoComUDP.ReceiveDataThread();
            receive.Text += MotoComUDP.ReceivedData + "\r\n";
            String[] tempper = MotoComUDP.Get_feedback_staus();
            status_code.Text = tempper[0];
            added_statuscode.Text = tempper[1];
            re_value.Text = BitConverter.ToInt32(MotoComUDP.ReceiveBytes, 32).ToString();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            MotoComUDP.Write_Read_IO(Convert.ToInt16(register.Text));
            MotoComUDP.ReceiveDataThread();
            receive.Text += MotoComUDP.ReceivedData + "\r\n";
            String[] tempper = MotoComUDP.Get_feedback_staus();
            status_code.Text = tempper[0];
            added_statuscode.Text = tempper[1];
            IO_value.Text = BitConverter.ToInt32(MotoComUDP.ReceiveBytes, 32).ToString();
        }

        private void button14_Click(object sender, EventArgs e)
        {

        }

        private void button15_Click(object sender, EventArgs e)
        {

        }
        #region manual_button
        private void inc_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            inc_status = false;
        }

        private void dec_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            dec_status = false;
        }

        private void dec_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            dec_status = true;
        }
        int axis_select = 1;
        private void axis_change(object sender, EventArgs e)
        {
            if (s_axis.Checked == true)
            {
                axis_select = 1;
            }
            else if (l_axis.Checked == true)
            {
                axis_select = 2;
            }
            else if (u_axis.Checked == true)
            {
                axis_select = 3;
            }
            else if (r_axis.Checked == true)
            {
                axis_select = 4;
            }
            else if (b_axis.Checked == true)
            {
                axis_select = 5;
            }
            else if (t_axis.Checked == true)
            {
                axis_select = 6;
            }
        }

        private void home_Click(object sender, EventArgs e)
        {
            theta[1] = 0;
            theta[2] = PI / 2;
            theta[3] = 0;
            theta[4] = 0;
            theta[5] = -PI / 2;
            theta[6] = 0;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (op_cartesian.Checked == true)
            {
                list.Items.Add("cartesian cordinate");
                list_point_cartesian.Add(new float[6] { Convert.ToSingle(X.Text), Convert.ToSingle(Y.Text), Convert.ToSingle(Z.Text), Convert.ToSingle(Rx.Text), Convert.ToSingle(Ry.Text), Convert.ToSingle(Rz.Text) });
            }else if(op_pulse.Checked == true)
            {
                list.Items.Add("pulse cordinate");
                list_point_pulse.Add(new Int32[6] { Convert.ToInt32(t1.Text), Convert.ToInt32(t2.Text), Convert.ToInt32(t3.Text), Convert.ToInt32(t4.Text), Convert.ToInt32(t5.Text), Convert.ToInt32(t6.Text) });
            }
        }

        private void list_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(list.SelectedItem.ToString() == "cartesian cordinate")
            {
                u1.Text = "X(mm)";       un1.Text = list_point_cartesian[list.SelectedIndex][0].ToString();
                u2.Text = "Y(mm)";       un2.Text = list_point_cartesian[list.SelectedIndex][1].ToString();
                u3.Text = "Z(mm)";       un3.Text = list_point_cartesian[list.SelectedIndex][2].ToString();
                u4.Text = "Rx(deg)";     un4.Text = list_point_cartesian[list.SelectedIndex][3].ToString();
                u5.Text = "Ry(deg)";     un5.Text = list_point_cartesian[list.SelectedIndex][4].ToString();
                u6.Text = "Rz(deg)";     un6.Text = list_point_cartesian[list.SelectedIndex][5].ToString();
            }
            else if(list.SelectedItem.ToString() == "pulse cordinate")
            {
                u1.Text = "theta1(pulse)"; un1.Text = list_point_pulse[list.SelectedIndex][0].ToString();
                u2.Text = "theta2(pulse)"; un2.Text = list_point_pulse[list.SelectedIndex][1].ToString();
                u3.Text = "theta3(pulse)"; un3.Text = list_point_pulse[list.SelectedIndex][2].ToString();
                u4.Text = "theta4(pulse)"; un4.Text = list_point_pulse[list.SelectedIndex][3].ToString();
                u5.Text = "theta5(pulse)"; un5.Text = list_point_pulse[list.SelectedIndex][4].ToString();
                u6.Text = "theta6(pulse)"; un6.Text = list_point_pulse[list.SelectedIndex][5].ToString();
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if(list.SelectedIndex != -1) 
                list.Items.RemoveAt(list.SelectedIndex);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            MotoComUDP.Execute_job(job_name.Text);
            MotoComUDP.ReceiveDataThread();
            receive.Text += MotoComUDP.ReceivedData + "\r\n";
            String[] tempper = MotoComUDP.Get_feedback_staus();
            status_code.Text = tempper[0];
            added_statuscode.Text = tempper[1];
        }

        private void inc_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            inc_status = true;
        }
        #endregion
        private void loadMaterials(String filename)
        {
            foreach (var mat in Material.LoadFromFile(filename))
            {
                if (!materials.ContainsKey(mat.Key))
                {
                    materials.Add(mat.Key, mat.Value);
                }
            }
            #region texture
            // Load textures
            /*foreach (Material mat in materials.Values)
            {
                if (File.Exists(mat.AmbientMap) && !textures.ContainsKey(mat.AmbientMap))
                {
                    textures.Add(mat.AmbientMap, loadImage(mat.AmbientMap));
                }

                if (File.Exists(mat.DiffuseMap) && !textures.ContainsKey(mat.DiffuseMap))
                {
                    textures.Add(mat.DiffuseMap, loadImage(mat.DiffuseMap));
                }

                if (File.Exists(mat.SpecularMap) && !textures.ContainsKey(mat.SpecularMap))
                {
                    textures.Add(mat.SpecularMap, loadImage(mat.SpecularMap));
                }

                if (File.Exists(mat.NormalMap) && !textures.ContainsKey(mat.NormalMap))
                {
                    textures.Add(mat.NormalMap, loadImage(mat.NormalMap));
                }

                if (File.Exists(mat.OpacityMap) && !textures.ContainsKey(mat.OpacityMap))
                {
                    textures.Add(mat.OpacityMap, loadImage(mat.OpacityMap));
                }
            }*/
            #endregion
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Int32 motionSpeed = Convert.ToInt32(Convert.ToDouble(speed.Text) * 100);

            Int32 X_cordVal = Convert.ToInt32(Convert.ToDouble(xm.Text) * 1000);
            Int32 Y_cordVal = Convert.ToInt32(Convert.ToDouble(ym.Text) * 1000);
            Int32 Z_cordVal = Convert.ToInt32(Convert.ToDouble(zm.Text) * 1000);

            Int32 Rx_angle = Convert.ToInt32(Convert.ToDouble(rxm.Text) * 10000);
            Int32 Ry_angle = Convert.ToInt32(Convert.ToDouble(rym.Text) * 10000);
            Int32 Rz_angle = Convert.ToInt32(Convert.ToDouble(rzm.Text) * 10000);
            MotoComUDP.MoveJoint(motionSpeed, X_cordVal, Y_cordVal, Z_cordVal, Rx_angle, Ry_angle, Rz_angle);
            MotoComUDP.ReceiveDataThread();
            receive.Text += MotoComUDP.ReceivedData + "\r\n";
            String[] tempper = MotoComUDP.Get_feedback_staus();
            status_code.Text = tempper[0];
            added_statuscode.Text = tempper[1];
            if (check.Checked == true)
            {
                test1.Text = "run";
                timer2.Enabled = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Int32 motionSpeed = Convert.ToInt32(Convert.ToDouble(speed.Text) * 100);
            Int32 first_pulse = Convert.ToInt32(t1_pulse.Text);
            Int32 second_pulse = Convert.ToInt32(t2_pulse.Text);
            Int32 third_pulse = Convert.ToInt32(t3_pulse.Text);

            Int32 fourth_pulse = Convert.ToInt32(t4_pulse.Text);
            Int32 fifth_pulse = Convert.ToInt32(t5_pulse.Text);
            Int32 sixth_pulse = Convert.ToInt32(t6_pulse.Text);
            pulse_des[0] = first_pulse;
            pulse_des[1] = second_pulse;
            pulse_des[2] = third_pulse;
            pulse_des[3] = fourth_pulse;
            pulse_des[4] = fifth_pulse;
            pulse_des[5] = sixth_pulse;
            MotoComUDP.MovePulse(motionSpeed, first_pulse, second_pulse, third_pulse, fourth_pulse, fifth_pulse, sixth_pulse);
            MotoComUDP.ReceiveDataThread();
            receive.Text += MotoComUDP.ReceivedData + "\r\n";
            String[] tempper = MotoComUDP.Get_feedback_staus();
            status_code.Text = tempper[0];
            added_statuscode.Text = tempper[1];
            if (check.Checked == true)
            {
                test1.Text = "run";
                timer2.Enabled = true;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            MotoComUDP.GetPosition_pulse();
            MotoComUDP.ReceiveDataThread();
            receive.Text += MotoComUDP.ReceivedData + "\r\n";
            String[] tempper = MotoComUDP.Get_feedback_staus();
            status_code.Text = tempper[0];
            added_statuscode.Text = tempper[1];
            byte[] data = MotoComUDP.ReceiveBytes;
            t1.Text = Convert.ToString(MotoComUDP.ByteArrayToInt32(MotoComUDP.SubArray(data, 52, 4)));
            t2.Text = Convert.ToString(MotoComUDP.ByteArrayToInt32(MotoComUDP.SubArray(data, 56, 4)));
            t3.Text = Convert.ToString(MotoComUDP.ByteArrayToInt32(MotoComUDP.SubArray(data, 60, 4)));
            t4.Text = Convert.ToString(MotoComUDP.ByteArrayToInt32(MotoComUDP.SubArray(data, 64, 4)));
            t5.Text = Convert.ToString(MotoComUDP.ByteArrayToInt32(MotoComUDP.SubArray(data, 68, 4)));
            t6.Text = Convert.ToString(MotoComUDP.ByteArrayToInt32(MotoComUDP.SubArray(data, 72, 4)));
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                if (check.Checked == true)
                {
                    MotoComUDP.SendCommand(MotoComUDP.temper);
                    MotoComUDP.ReceiveDataThread_direct();
                    pointer = 0;
                    byte[] data = MotoComUDP.ReceiveBytes;
                    for (byte counter = 52; counter <= 72; counter += 4)
                    {
                        pulse_current[pointer++] = MotoComUDP.ByteArrayToInt32(MotoComUDP.SubArray(data, counter, 4));
                        pulse_error[pointer - 1] = Abs(pulse_des[pointer - 1] - pulse_current[pointer - 1]);
                    }
                    a1 = (float)pulse_current[0] * 1.503900192e-5f;
                    a2 = (float)pulse_current[1] * 1.53397394e-5f + 7.012452296e-6f;
                    a3 = (float)pulse_current[2] * 4.090615434e-5f;
                    a4 = (float)pulse_current[3] * 5.114196812e-5f;
                    a5 = (float)pulse_current[4] * 5.113269293e-5f;
                    a6 = (float)pulse_current[5] * 5.113269293e-5f;
                    if (pulse_error[0] < 8 && pulse_error[1] < 8 && pulse_error[2] < 8 && pulse_error[3] < 8 && pulse_error[4] < 8 && pulse_error[5] < 8)
                    {
                        test1.Text = "stop";
                        timer2.Enabled = false;
                    }
                }
            }
            catch
            {
                test1.Text = "stop error";
                timer2.Enabled = false;
            }
        }
        //OpenTK.Graphics.GraphicsMode Mode = new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(32), 24, 8, 16, new OpenTK.Graphics.ColorFormat(32));

    }
}
