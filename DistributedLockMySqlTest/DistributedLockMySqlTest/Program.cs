using Medallion.Threading;
using Medallion.Threading.MySql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace DistributedLockMySqlTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // DoMySqlLockTest();
            DoMySqlLockTestReuseKeys();
            Console.ReadLine();
        }

        private static void DoMySqlLockTest()
        {
            Console.WriteLine("Hello DistributedLock.Mysql!");
            var lockProvider = new MySqlDistributedSynchronizationProvider($"server=127.0.0.1;port=3306;user=root;password=root;database=benchmarks;SslMode=None");

            for (int i = 0; i < 5; i++)
            {
                ThreadPool.QueueUserWorkItem((obj) =>
                {
                    string lockString = string.Join("", Enumerable.Repeat(0, 10).Select(n => (char)new Random().Next(30, 110)));
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}\tThread: {obj}\tRequesting lock {lockString}");
                    Stopwatch sw = Stopwatch.StartNew();
                    using (var redLock = lockProvider.AcquireLock(lockString, TimeSpan.FromSeconds(1)))
                    {
                        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}\tThread: {obj}\tGot lock  {lockString} (waited {sw.ElapsedMilliseconds})");
                    }
                }, i);
            }
            Console.WriteLine();
        }


        private static void DoMySqlLockTestReuseKeys()
        {
            Console.WriteLine("Hello DistributedLock.Mysql with predefined keys");

            int max = 5;
            var lockProvider = new MySqlDistributedSynchronizationProvider($"server=127.0.0.1;port=3306;user=root;password=root;database=benchmarks;SslMode=None");
            List<string> keys = Enumerable.Repeat(0, max).Select(a => string.Join("", Enumerable.Repeat(0, 10).Select(n => (char)new Random().Next(30, 110)))).ToList();

            
            for (int i = 0; i < max; i++)
            {
                ThreadPool.QueueUserWorkItem((obj) =>
                {
                    var key = keys[(int)obj];

                    for (int q = 0; q < 3; q++)
                    {
                        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}\tThread: {obj} (q={q})\tRequesting lock {key}");
                        Stopwatch sw = Stopwatch.StartNew();
                        using (var redLock = lockProvider.AcquireLock(key, TimeSpan.FromSeconds(1)))
                        {
                            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}\tThread: {obj} (q={q})\tGot lock  {key} (waited {sw.ElapsedMilliseconds})");
                        }
                    }
                }, i);
            }
        }
    }
}
