namespace Radium
{
    using System.IO;

    public class GpuProgram : IGpuProgram
    {
        private readonly string fileName;

        private string source;

        public GpuProgram(string fileName)
        {
            this.fileName = fileName;
            this.Name = Path.GetFileNameWithoutExtension(fileName);
        }

        public string Name { get; }

        public string Source => this.source ?? this.ReloadSource();

        public string ReloadSource()
        {
            return this.source = File.ReadAllText(this.fileName);
        }
    }
}
