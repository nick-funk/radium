namespace Radium
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using Cloo;

    public class RayTrace : GpuCompute<Bitmap>
    {
        private readonly int width;
        private readonly int height;

        public RayTrace(IGpuProgram sourceProgram, int width, int height)
            : base(sourceProgram)
        {
            this.width = width;
            this.height = height;
        }

        protected override Bitmap Execute()
        {
            Bitmap image;

            ComputeBuffer<char> kernelOutput = new ComputeBuffer<char>(this.Context, ComputeMemoryFlags.WriteOnly,
                this.width * this.height * 4);

            var spheres = new[]
            {
                0, 0, 0, 0.25f,
                0.5f, 0, 0, 0.3f
            };

            ComputeBuffer<float> sphereData = new ComputeBuffer<float>(
                this.Context,
                ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer,
                spheres);

            var camera = new[] { 0, 0.15f, 3, 0, 0, -1 };
            ComputeBuffer<float> cameraData = new ComputeBuffer<float>(
                this.Context,
                ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer,
                camera);

            this.Kernel.SetValueArgument(0, this.width);
            this.Kernel.SetValueArgument(1, this.height);
            this.Kernel.SetMemoryArgument(2, kernelOutput);
            this.Kernel.SetMemoryArgument(3, sphereData);
            this.Kernel.SetMemoryArgument(4, cameraData);

            this.Commands.Execute(this.Kernel, null, new long[] {this.width, this.height }, null, this.Events);

            byte[] kernelResult = new byte[this.width * this.height * 4];
            GCHandle kernelResultHandle = GCHandle.Alloc(kernelResult, GCHandleType.Pinned);

            this.Commands.Read(kernelOutput, false, 0, this.width * this.height * 4,
                kernelResultHandle.AddrOfPinnedObject(), this.Events);
            this.Commands.Finish();

            kernelResultHandle.Free();

            unsafe
            {
                fixed (byte* kernelResultPointer = kernelResult)
                {
                    IntPtr intPtr = new IntPtr(kernelResultPointer);
                    image = new Bitmap(this.width, this.height, this.width * 4, PixelFormat.Format32bppArgb, intPtr);
                }
            }

            return image;
        }
    }
}