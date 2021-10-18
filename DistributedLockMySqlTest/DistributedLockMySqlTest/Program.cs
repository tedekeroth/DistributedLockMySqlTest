using Medallion.Threading;
using Medallion.Threading.MySql;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace DistributedLockMySqlTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello DistributedLock.Mysql!");
            DoMySqlLockTest();
            Console.ReadLine();
        }

        private static void DoMySqlLockTest()
        {
            var lockProvider = new MySqlDistributedSynchronizationProvider($"server=127.0.0.1;port=3306;user=root;password=root;database=benchmarks;SslMode=None");

            for (int i = 0; i < 2; i++)
            {
                ThreadPool.QueueUserWorkItem((obj) =>
                {
                    var wait = new Random().Next(10, 50);
                    Thread.Sleep(wait);
                    string lockString = string.Join("", Enumerable.Repeat(0, 10).Select(n => (char)new Random().Next(30, 110)));
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}\tThread: {obj}\tRequesting lock {lockString} after sleeping for {wait} ms");
                    Stopwatch sw = Stopwatch.StartNew();
                    using (var redLock = lockProvider.AcquireLock(lockString, TimeSpan.FromSeconds(1)))
                    {
                        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}\tThread: {obj}\tGot lock  {lockString} (waited {sw.ElapsedMilliseconds})");
                    }
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}\tThread: {obj}\tReleased lock");
                }, i);
            }
        }
    }
}
