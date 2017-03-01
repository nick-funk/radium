namespace Radium
{
    using Cloo;

    public class Addition : GpuCompute
    {
        private readonly float a;
        private readonly float b;

        public Addition(IGpuProgram sourceProgram, float a, float b)
            : base(sourceProgram)
        {
            this.a = a;
            this.b = b;
        }

        protected override object Execute()
        {
            ComputeBuffer<float> kernelOutput = new ComputeBuffer<float>(this.Context, ComputeMemoryFlags.WriteOnly, 1);

            this.Kernel.SetValueArgument(0, this.a);
            this.Kernel.SetValueArgument(1, this.b);
            this.Kernel.SetMemoryArgument(2, kernelOutput);

            this.Commands.Execute(this.Kernel, null, new long[] { 1 }, null, this.Events);

            var result = new float[1];
            this.Commands.ReadFromBuffer(kernelOutput, ref result, false, this.Events);
            this.Commands.Finish();

            return result[0];
        }
    }
}