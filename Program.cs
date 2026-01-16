using System;

Console.WriteLine("Hello from SimpleDotnetApp!");
Console.WriteLine($"Framework: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");

if (args.Length > 0)
{
    Console.WriteLine("Arguments:");
    foreach (var a in args) Console.WriteLine($" - {a}");
}
else
{
    Console.WriteLine("No command-line arguments provided. Run with: dotnet run -- <args>");
}
