namespace Radium
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using Cloo;

    public class MandelbrotRenderer : GpuCompute<Bitmap>
    {
        public MandelbrotRenderer(IGpuProgram sourceProgram)
            : base(sourceProgram)
        {
        }

        protected override Bitmap Execute()
        {
            Bitmap image;
            var height = 720;
            var width = 1280;

            float realLeft = -2.0f;
            float realRight = 1.0f;
            float imaginaryBottom = -1.2f;
            float imaginaryTop = imaginaryBottom + (realRight - realLeft) * height / width;

            float realFactor = (realRight - realLeft) / (width - 1);
            float imaginaryFactor = (imaginaryTop - imaginaryBottom) / (height - 1);

            uint maxIterations = 30;

            ComputeBuffer<char> kernelOutput = new ComputeBuffer<char>(this.Context, ComputeMemoryFlags.WriteOnly, width * height * 4);

            this.Kernel.SetValueArgument(0, realFactor);
            this.Kernel.SetValueArgument(1, imaginaryFactor);
            this.Kernel.SetValueArgument(2, realLeft);
            this.Kernel.SetValueArgument(3, imaginaryBottom);
            this.Kernel.SetValueArgument(4, imaginaryTop);
            this.Kernel.SetValueArgument(5, maxIterations);
            this.Kernel.SetValueArgument(6, width);
            this.Kernel.SetMemoryArgument(7, kernelOutput);

            this.Commands.Execute(this.Kernel, null, new long[] { width, height }, null, this.Events);

            byte[] kernelResult = new byte[width * height * 4];
            GCHandle kernelResultHandle = GCHandle.Alloc(kernelResult, GCHandleType.Pinned);

            this.Commands.Read(kernelOutput, false, 0, width * height * 4, kernelResultHandle.AddrOfPinnedObject(), this.Events);
            this.Commands.Finish();

            kernelResultHandle.Free();

            unsafe
            {
                fixed (byte* kernelResultPointer = kernelResult)
                {
                    IntPtr intPtr = new IntPtr(kernelResultPointer);
                    image = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, intPtr);
                }
            }

            return image;
        }
    }
}