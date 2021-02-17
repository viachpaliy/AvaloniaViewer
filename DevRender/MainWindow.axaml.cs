using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Numerics;
using System;
using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;
using Avalonia.Media;
using Avalonia.Platform;

namespace DevRender
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
            Image image = new Image();
            image.Stretch = Stretch.Fill;
            image.Margin = new Thickness(0);
            image.Width = this.ClientSize.Width;
            image.Height = this.ClientSize.Height;
            int width = (int)image.Width;
            int height = (int)image.Height;
            
            //grdMain.Children.Add(image);
            this.FindControl<Grid>("grid").HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            this.FindControl<Grid>("grid").VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
            this.FindControl<Grid>("grid").Children.Add(image);
            var s = Marshal.SizeOf<Vertex>();
            var env = new Enviroment(1000);
            var camera = new Camera(new Vector3(0, 100, -400), 1, 1.57f, 640, 480);
            var camera2 = new Camera(new Vector3(0, 100, -400), 1, 1.57f, 1920, 1080);
            CreateSylvanasScene(env, camera2);
            
            Title = Title + " " + Convert.ToString(width) + " x " + Convert.ToString(height);
            var render = new DevRender(env, width, height, camera, camera2);
            KeyDown += (args, e) => render.Controller.KeyDown(e);
            KeyUp += (args, e) => render.Controller.KeyUp(e);
            image.Source = render.wbitmap;
            render.Start();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void CreateSylvanasScene(Enviroment enviroment, Camera bindingCamera)
        {
            var model = ObjParser.FromObjFile(@"D:\VSCode\AvaloniaViewer\DevRender\Models\sylvanas_obj.obj", null);
            var smShader = new ShadowMappingShader(enviroment, new Rasterizer(bindingCamera), 50000f);
            var pShader = new PhongModelShader(new Light(bindingCamera.Pivot.Center, 2f));
            model.Shaders = new IShader[] { smShader, pShader };
            model.Scale(10f);
            model.Rotate(3.14f + 0.5f, Axis.Y);
            model.Move(new Vector3(0, -2200, 0));
            enviroment.AddPrimitive(model);
        }
    }
}
