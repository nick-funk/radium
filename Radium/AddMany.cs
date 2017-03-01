namespace Radium
{
    using System;
    using Cloo;

    public class AddMany : GpuCompute
    {
        private readonly int count;

        public AddMany(IGpuProgram sourceProgram, int count)
            : base(sourceProgram)
        {
            this.count = count;
        }

        protected override object Execute()
        {
            var arrayA = new float[this.count];
            var arrayB = new float[this.count];
            var result = new float[this.count];

            Random rand = new Random();
            for (int i = 0; i < this.count; i++)
            {
                arrayA[i] = (float)(rand.NextDouble() * 100);
                arrayB[i] = (float)(rand.NextDouble() * 100);
            }

            ComputeBuffer<float> a = new ComputeBuffer<float>(
                this.Context,
                ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer,
                arrayA);
            ComputeBuffer<float> b = new ComputeBuffer<float>(
                this.Context,
                ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer,
                arrayB);
            ComputeBuffer<float> resultBuffer = new ComputeBuffer<float>(this.Context, ComputeMemoryFlags.WriteOnly, result.Length);

            this.Kernel.SetMemoryArgument(0, a);
            this.Kernel.SetMemoryArgument(1, b);
            this.Kernel.SetMemoryArgument(2, resultBuffer);

            this.Commands.Execute(this.Kernel, null, new long[] { this.count }, null, this.Events);

            this.Commands.ReadFromBuffer(resultBuffer, ref result, false, this.Events);

            this.Commands.Finish();

            return result;
        }
    }
}