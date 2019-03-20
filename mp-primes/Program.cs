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