# Project Description

Passing data to and from the GPU for computation historically sucks.

Radium aims to make GPU computation as easy as implementing one class.

# Overview

GPU computation is made up of two things.

1. The code you run on your GPU (we sometimes call this a kernel)
2. The code you use to talk to your GPU

In Radium, the first is written in a C-style syntax that is essentially shader code.
The second is written in C#.

Radium makes this easier by giving you a clear template for how this is all done.

# Quick Example

Let's say you want to add two floating point numbers together on the GPU. 

In Radium, all you need to do is implement the GpuCompute class and write a kernel.

Let's start with the compute class to do addition. This is how your program will talk to the GPU.

```csharp
public class Addition : GpuCompute<float> // Return type for two floats added together is a float
{
  private readonly float a;
  private readonly float b;
  
  // You can take in as many parameters as you want
  // through the constructor, this works well with
  // dependency injection
  public Addition(IGpuProgram program, float a, float b)
    : base(program)
  {
      this.a = a;
      this.b = b;
  }
  
  protected override float Execute()
  {
      // We want to get back a float from the GPU
      ComputeBuffer<float> kernelOutput = new ComputeBuffer<float>(this.Context, ComputeMemoryFlags.WriteOnly, 1);

      // Set "a" and "b" as arguments and configure the return value
      this.Kernel.SetValueArgument(0, this.a);
      this.Kernel.SetValueArgument(1, this.b);
      this.Kernel.SetMemoryArgument(2, kernelOutput);

      // Tell Cloo to execute
      this.Commands.Execute(this.Kernel, null, new long[] { 1 }, null, this.Events);

      // Read out the result from the GPU buffer into our result array
      var result = new float[1];
      this.Commands.ReadFromBuffer(kernelOutput, ref result, false, this.Events);
      this.Commands.Finish();

      // Return the result of a + b
      return result[0];
  }
}
```

Now, let's write the code that will be run on the GPU to add two float values together.
This is a separate text file that is stored in your project.

I usually create a folder called ```kernels``` and would save this file as ```addition.kl``` (the kl stands for kernel).

Make sure that you set each kernel file's "Copy to Output Directory" property in Visual Studio to "Copy if Newer" so that it is available to your application when it goes to compile the kernel file for the GPU.

```c
kernel void addition(
	const float a, 
	const float b,
	global float* buffer)
{
	int index = 0;

	buffer[index] = a + b;
}
```

Here we have ```a``` and ```b``` coming into the GPU and a result buffer.
We write the result of ```a + b``` to the first location in the result array.

Later, we can use this array to do multithreaded operations on all of the GPU cores/streams but that's beyond this example.

Lastly, let's compile the kernel and execute our newly implemented Addition class.

```csharp
static void Main(string[] args)
{
  var a = 1.0f;
  var b = 2.0f;

  var computation = new Addition(new GpuProgram("kernels/addition.kl"), a, b);
  var result = computation.Run();
  Console.WriteLine($"{a} + {b} = {result}");
  Console.ReadKey();
}
```

# What can it do?

GPU computation is very useful for any parallelizable task. This is why they were developed for graphics as many rendering algorithms fall into the category of "ridiculously parallelizable" problems.

Some examples that are included in the source are:

* Large scale array transforms (multiplying, adding)
  * I intend to do a Fast Fourier example soon
* Generating fractals
* In-depth rendering (formal ray-casting and photon mapping for highly realistic but computationally intensive renders)

There are more, but this is just some examples. The advantage is this library is how easily this can fit into your current application. You can hand pick specific sections that need a boost, convert the method to a short bit of kernel code, run it on the GPU and return the results back to your application code with minimal fuss.

To stop talking, and show you the results, here is the results of a ray tracer kernel I wrote to dump out an animation of a light moving around some spheres.

[![GPU Raytracing using Radium](https://img.youtube.com/vi/Hu1327IZGQg/0.jpg)](https://www.youtube.com/watch?v=Hu1327IZGQg)

This was an 80 frame animation that was computed in under 800 milliseconds on a single nVidia GTX 660 graphics card.
