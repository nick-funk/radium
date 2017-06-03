namespace Radium
{
    using System;
    using Cloo;

    public abstract class GpuCompute<T>
    {
        private readonly IGpuProgram sourceProgram;

        private ComputePlatform platform;

        private ComputeProgram program;

        private bool programIsLoaded;

        private ComputeContextPropertyList properties;

        protected GpuCompute(IGpuProgram sourceProgram)
        {
            this.sourceProgram = sourceProgram;
            this.programIsLoaded = false;
        }

        protected ComputeCommandQueue Commands { get; private set; }

        protected ComputeContext Context { get; private set; }

        protected ComputeEventList Events { get; private set; }

        protected ComputeKernel Kernel { get; private set; }

        public T Run()
        {
            if (!this.programIsLoaded)
            {
                this.LoadProgram();
                this.programIsLoaded = true;
            }

            return this.Execute();
        }

        protected abstract T Execute();

        private void LoadProgram()
        {
            this.platform = ComputePlatform.Platforms[0];
            this.properties = new ComputeContextPropertyList(this.platform);
            this.Context = new ComputeContext(this.platform.Devices, this.properties, null, IntPtr.Zero);

            this.program = new ComputeProgram(this.Context, new[] { this.sourceProgram.Source });
            this.program.Build(null, "-cl-mad-enable", null, IntPtr.Zero);
            this.Kernel = this.program.CreateKernel(this.sourceProgram.Name);

            this.Commands = new ComputeCommandQueue(this.Context, this.Context.Devices[0], ComputeCommandQueueFlags.None);
            this.Events = new ComputeEventList();
        }
    }
}
