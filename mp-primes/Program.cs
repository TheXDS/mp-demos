//
// Program.cs
//
// Author:
//       César Andrés Morgan <xds_xps_ivx@hotmail.com>
//
// Copyright (c) 2019 César Andrés Morgan
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TheXDS.MCART.Math;
using System.Linq;
using System.Threading.Tasks;

namespace TheXDS.Experiments.Mp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Configurar prioridad del proceso.
            var thisProcess = System.Diagnostics.Process.GetCurrentProcess();
            try
            {
                thisProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine($"No fue posible cambiar la prioridad de esta aplicación. Continuando con prioridad {thisProcess.PriorityClass}");
            }

            // Inicializar un arreglo de datos gigantesco
            var c = new int[262144];
            var u = c.GetUpperBound(0);
            var rnd = new Random();
            for (var j = 0; j < u; j++) c[j] = rnd.Next(1, 262144);


            Console.Write("Conteo en bloque foreach... ");
            var t = new System.Diagnostics.Stopwatch();
            var tot = 0;

            t.Start();
            foreach (var j in c) { if (j.IsPrime()) tot++; }
            t.Stop();

            Console.WriteLine($"{tot}, Tiempo: {t.ElapsedMilliseconds} ms");


            Console.Write("Conteo utilizando lambdas... ");
            t.Reset();
            tot = 0;

            t.Start();
            tot = c.Count(p => p.IsPrime());
            t.Stop();

            Console.WriteLine($"{tot}, Tiempo: {t.ElapsedMilliseconds} ms");

            var times = new Dictionary<int, long>();
            for (var threads = 2; threads <= Environment.ProcessorCount; threads++)
            {
                Console.Write($"Conteo multihilo ({threads} hilos)... ");
                t.Reset();
                tot = 0;

                t.Start();
                var part = Partitioner.Create(c);
                ConcurrentBag<int> primes = new ConcurrentBag<int>();


                Parallel.ForEach(part, new ParallelOptions { MaxDegreeOfParallelism = threads }, j =>
                {
                    if (j.IsPrime())
                        primes.Add(j);
                });
                tot = primes.Count;
                t.Stop();

                times.Add(threads, t.ElapsedMilliseconds);

                Console.WriteLine($"{tot}, Tiempo: {t.ElapsedMilliseconds} ms");
            }

            var best = times.OrderBy(p => p.Value).First();

            Console.WriteLine($"Mejor tiempo: {best.Key} hilos ({best.Value} ms)");
        }
    }
}