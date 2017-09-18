namespace Radium.Rendering
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Cloo;

    public class RayTrace : GpuCompute<Bitmap>
    {
        private readonly int _width;
        private readonly int _height;

        public RayTrace(IGpuProgram sourceProgram, int width, int height, ICollection<PointLight> lights)
            : base(sourceProgram)
        {
            Lights = lights;
            _width = width;
            _height = height;
        }

        public ICollection<PointLight> Lights { get; set; }

        protected override Bitmap Execute()
        {
            Bitmap image;

            ComputeBuffer<char> kernelOutput = new ComputeBuffer<char>(Context, ComputeMemoryFlags.WriteOnly,
                _width * _height * 4);

            var spheres = new[]
            {
                0, 0, 0, 0.25f, 255, 0, 0,
                -0.35f, 0.5f, 0, 0.25f, 255, 255, 0,
                0.2f, 0.25f, 0.75f, 0.1f, 0, 255, 255,
                0.5f, 0, 0.5f, 0.3f, 0, 255, 0,
                -0.5f, 0, 0.5f, 0.3f, 0, 25, 180
            };

            ComputeBuffer<float> sphereData = new ComputeBuffer<float>(
                Context,
                ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer,
                spheres);

            var camera = new[] { 0, 0.15f, 3, 0, 0, -1 };
            ComputeBuffer<float> cameraData = new ComputeBuffer<float>(
                Context,
                ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer,
                camera);

            var lights = Lights.SelectMany(l => l.Position.ToArray()).ToArray();
            ComputeBuffer<float> lightData = new ComputeBuffer<float>(
                Context,
                ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer,
                lights.ToArray());

            Kernel.SetValueArgument(0, _width);
            Kernel.SetValueArgument(1, _height);
            Kernel.SetValueArgument(2, spheres.Length / 7);
            Kernel.SetMemoryArgument(3, kernelOutput);
            Kernel.SetMemoryArgument(4, sphereData);
            Kernel.SetMemoryArgument(5, cameraData);
            Kernel.SetValueArgument(6, Lights.Count);
            Kernel.SetMemoryArgument(7, lightData);

            Commands.Execute(Kernel, null, new long[] {_width, _height }, null, Events);

            byte[] kernelResult = new byte[_width * _height * 4];
            GCHandle kernelResultHandle = GCHandle.Alloc(kernelResult, GCHandleType.Pinned);

            Commands.Read(kernelOutput, false, 0, _width * _height * 4,
                kernelResultHandle.AddrOfPinnedObject(), Events);
            Commands.Finish();

            kernelResultHandle.Free();

            unsafe
            {
                fixed (byte* kernelResultPointer = kernelResult)
                {
                    IntPtr intPtr = new IntPtr(kernelResultPointer);
                    image = new Bitmap(_width, _height, _width * 4, PixelFormat.Format32bppArgb, intPtr);
                }
            }

            return image;
        }
    }
}