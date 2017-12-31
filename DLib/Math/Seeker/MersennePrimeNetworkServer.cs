﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;

namespace DLib.Math.Seeker
{
    public class MPNetworkServer2 : IDisposable
    {
        Networking.ServerNew3 server;
        List<(uint exponent, DateTime explorationDate, TimeSpan totalTime, TimeSpan testTime, IPAddress ipAdresse)> mersennePrimes = new List<(uint, DateTime, TimeSpan, TimeSpan, IPAddress)>();
        ManualResetEventSlim manualResetEvent = new ManualResetEventSlim(true);
        Stopwatch totalTime = new Stopwatch();

        public bool Started => totalTime.ElapsedTicks > 0;
        public bool Running { get; private set; }
        public bool Paused => !manualResetEvent.Wait(0);
        public uint NextExponent { get; private set; }
        public (uint exponent, DateTime explorationDate, TimeSpan totalTime, TimeSpan testTime, IPAddress ipAdresse)[] MersennePrimes => mersennePrimes.ToArray();
        public uint[] MersennePrimeExponents => mersennePrimes.Select(i => i.exponent).ToArray();
        public TimeSpan ServerWorkTime => totalTime.Elapsed;

        public void Start(int port, uint startExponent)
        {
            if (!Running && !Started)
            {
                Running = true;
                server = new Networking.ServerNew3(port);
                mersennePrimes.Clear();
                manualResetEvent.Set();
                NextExponent = System.Math.Max(5, startExponent + ((startExponent + 1) & 1));
                totalTime.Restart();
                new Thread(() => {
                    while (Running)
                    {
                        manualResetEvent.Wait();
                        IPEndPoint member = null;
                        string message = server.Recieve(ref member);
                        if (message == "g")
                        {
                            const uint count = 1024;
                            server.Send(member, NextExponent, count);
                            NextExponent += count;
                        }
                        else
                        {
                            var array = message.Split('|');
                            InsertMersennPrime((uint.Parse(array[0]), DateTime.Parse(array[1]), ServerWorkTime, TimeSpan.Parse(array[2]), member.Address));

                            void InsertMersennPrime((uint, DateTime, TimeSpan, TimeSpan, IPAddress) mersennePrime)
                            {
                                Console.WriteLine("exponent: {0};\texploration date: {1};\ttotal time: {2};\ttest time: {3};\tclient ip: {4}", mersennePrime.Item1, mersennePrime.Item2, mersennePrime.Item3, mersennePrime.Item4, mersennePrime.Item5);
                                int i = mersennePrimes.Count;
                                for (; i > 0 && mersennePrimes[i - 1].exponent > mersennePrime.Item1; i--) ;
                                mersennePrimes.Insert(i, mersennePrime);
                            }
                        }
                    }
                }).Start();
            }
        }

        public void Stop()
        {
            if (Running)
            {
                manualResetEvent.Set();
                Running = false;
                totalTime.Stop();
            }
        }

        public void Dispose()
        {
            if (Running)
                Stop();
            manualResetEvent.Dispose();
            server.Dispose();
        }

        public void Pause()
        {
            if (Running && !Paused)
            {
                manualResetEvent.Reset();
                totalTime.Stop();
            }
        }

        public void Continue()
        {
            if (Running && Paused)
            {
                totalTime.Start();
                manualResetEvent.Set();
            }
        }
    }

    public class MPNetworkServer : IDisposable
    {
        Networking.ServerNew3 server;
        List<(uint exponent, TimeSpan time, IPAddress ipAdresse)> mersennePrimes = new List<(uint, TimeSpan, IPAddress)>();
        ManualResetEventSlim manualResetEvent = new ManualResetEventSlim(true);
        Stopwatch totalTime = new Stopwatch();

        public bool Running { get; private set; }
        public bool Disposed { get; private set; }
        public bool Paused => !manualResetEvent.Wait(0);
        public uint NextExponent { get; private set; }
        public (uint exponent, TimeSpan time, IPAddress ipAdresse)[] MersennePrimes => mersennePrimes.ToArray();
        public uint[] MersennePrimeExponents => mersennePrimes.Select(i => i.exponent).ToArray();
        public TimeSpan ServerWorkTime => totalTime.Elapsed;
        public int Port => server.Port;

        const uint packageSize = 1024;

        ~MPNetworkServer() => Dispose();

        public void Start(int port, uint startExponent)
        {
            if (!Running && !Disposed)
                new Thread(() => {
                    Running = true;
                    NextExponent = System.Math.Max(5, startExponent + ((startExponent + 1) & 1));
                    totalTime.Restart();
                    server = new Networking.ServerNew3(port);
                    while (Running)
                    {
                        manualResetEvent.Wait();
                        IPEndPoint member = null;
                        var message = server.Recieve(ref member);
                        if (message == "g")
                        {
                            server.Send(member, NextExponent, packageSize);
                            NextExponent += packageSize;
                        }
                        else
                            foreach (uint exponent in message.Split('|').Select(n => ulong.Parse(n)))
                            {
                                Console.WriteLine("exponent: {0};\ttime: {1};\tclient ip: {2}", exponent, ServerWorkTime, member.Address);
                                int i = mersennePrimes.Count;
                                for (; i > 0 && mersennePrimes[i - 1].exponent > exponent; i--) ;
                                mersennePrimes.Insert(i, (exponent, ServerWorkTime, member.Address));
                            }
                    }
                }).Start();
        }

        public void Stop()
        {
            if (Running)
            {
                mersennePrimes.Clear();
                manualResetEvent.Set();
                Running = false;
                totalTime.Stop();
            }
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                if (Running)
                    Stop();
                manualResetEvent.Dispose();
                server.Dispose();
                Disposed = true;
            }
        }

        public void Pause()
        {
            if (Running && !Paused)
            {
                manualResetEvent.Reset();
                totalTime.Stop();
            }
        }

        public void Continue()
        {
            if (Running && Paused)
            {
                totalTime.Start();
                manualResetEvent.Set();
            }
        }
    }
}
