namespace Radium
{
    using System;
    using System.Drawing;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                return;
            }

            string choice = args[0].ToLower();

            if (choice == "addition")
            {
                Addition(4.3f, 1.2f);
            }
            if (choice == "addmany")
            {
                AddMany(10);
            }
            if (choice == "fractal")
            {
                Mandelbrot();
            }
            if (choice == "raytrace")
            {
                RayTrace();
            }
        }

        private static void RayTrace()
        {
            var rayTracer = new RayTrace(new GpuProgram("kernels/raytrace.kl"), 1280, 720);
            var image = rayTracer.Run() as Image;
            image?.Save("raytrace.png");
        }

        private static void AddMany(int amount)
        {
            var addManyComputation = new AddMany(new GpuProgram("kernels/addmany.kl"), amount);
            var manyResult = (float[]) addManyComputation.Run();
            Console.WriteLine(string.Join(",", manyResult));
            Console.ReadKey();
        }

        private static void Addition(float a, float b)
        {
            var additionComputation = new Addition(new GpuProgram("kernels/addition.kl"), a, b);
            var result = (float) additionComputation.Run();
            Console.WriteLine($"{a} + {b} = {result}");
            Console.ReadKey();
        }

        private static void Mandelbrot()
        {
            var mandelbrotRenderer = new MandelbrotRenderer(new GpuProgram("kernels/mandelbrot.kl"));
            var mandelbrotImage = mandelbrotRenderer.Run() as Image;
            mandelbrotImage?.Save("mandelbrot.png");
        }
    }
}
