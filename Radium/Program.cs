namespace Radium
{
    using System;
    using Rendering;

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
            var rayTracer = new RayTrace(
                new GpuProgram("kernels/raytrace.kl"), 
                1920, 
                1080,
                new[]
                {
                    new PointLight(new Vector3(-1f, 1f, 1f)),
                    new PointLight(new Vector3(-1f, 1f, 1f))
                });

            int index = 0;
            for (float i = 0; i < 2f; i += 0.05f)
            {
                rayTracer.Lights = new[]
                {
                    new PointLight(new Vector3(i - 1f, 1f, 0.5f)),
                    new PointLight(new Vector3(1f - i, 1f, 0.5f))
                };

                var image = rayTracer.Run();
                image.Save($"raytrace/raytrace_{index}.png");

                index++;
            }
        }

        private static void AddMany(int amount)
        {
            var addManyComputation = new AddMany(new GpuProgram("kernels/addmany.kl"), amount);
            var manyResult = addManyComputation.Run();
            Console.WriteLine(string.Join(",", manyResult));
            Console.ReadKey();
        }

        private static void Addition(float a, float b)
        {
            var additionComputation = new Addition(new GpuProgram("kernels/addition.kl"), a, b);
            var result = additionComputation.Run();
            Console.WriteLine($"{a} + {b} = {result}");
            Console.ReadKey();
        }

        private static void Mandelbrot()
        {
            var mandelbrotRenderer = new MandelbrotRenderer(new GpuProgram("kernels/mandelbrot.kl"));
            var mandelbrotImage = mandelbrotRenderer.Run();
            mandelbrotImage?.Save("mandelbrot.png");
        }
    }
}
