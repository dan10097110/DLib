﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DLib.Collection
{
    public static class Primes2
    {
        static List<int> primes = new List<int>() { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 91, 97 };
        static int nextCand = primes.Last() + 2;
        static ThreadQueue threadQueue = new ThreadQueue();

        public static int GetIth(int i)
        {
            threadQueue.Wait();
            if (i >= primes.Count)
                CalcUntilIthPrime(i + 1);
            int p = primes[i];
            threadQueue.Finished();
            return p;
        }

        public static bool IsPrime(int n)
        {
            threadQueue.Wait();
            bool b = true;
            if (n < nextCand)
                b = Contain(n);
            else if (n < nextCand * nextCand)
            {
                CalcUntilI(n);
                threadQueue.Finished();
                b = primes.Last() == n;
            }
            else
            {
                CalcUntilI((int)System.Math.Sqrt(n));
                for (int i = 0; i < primes.Count; i++)
                    if (n % primes[i] == 0)
                    {
                        b = false;
                        break;
                    }
            }
            threadQueue.Finished();
            return b;
        }

        public static bool IsProbPrime(int n)
        {
            threadQueue.Wait();
            bool b = true;
            if (n < nextCand)
                b = Contain(n);
            else
            {
                CalcUntilI((int)System.Math.Sqrt(System.Math.Sqrt(n)));
                for (int i = 0; i < primes.Count; i++)
                    if (n % primes[i] == 0)
                    {
                        b = false;
                        break;
                    }
            }
            threadQueue.Finished();
            return b;
        }

        static void CalcUntilIthPrime(int exclusiveI)
        {
            while (primes.Count < exclusiveI)
                TestNextCand();
        }

        static void CalcUntilI(int inclusiveI)
        {
            if (inclusiveI < (nextCand << 1))
                while (primes.Last() < inclusiveI)
                    TestNextCand();
            else
                Sieve(inclusiveI);
        }

        static bool Contain(int n)
        {
            int u = 0;
            for (int o = primes.Count - 1; u != o;)
            {
                int m = (u + o) >> 1;
                if (primes[m] < n)
                    u = m + 1;
                else
                    o = m;
            }
            return primes[u] == n;
        }

        static void TestNextCand()
        {
            for (int i = 1; primes[i] * primes[i] <= nextCand; i++)
                if (nextCand % primes[i] == 0)
                {
                    nextCand += 2;
                    return;
                }
            primes.Add(nextCand);
            nextCand += 2;
        }

        static void Sieve(int exclusive)
        {
            var sieve = new BitArray(exclusive + 1, true);
            for (int i = 1; i < primes.Count; i++)
                for (int j = primes[i] * primes[i]; j < sieve.Count; sieve[j] = false, j += primes[i]) ;
            for (int i = nextCand; i * i <= sieve.Count; i += 2)
                if (sieve[i])
                {
                    primes.Add(i);
                    for (int j = i * i; j < sieve.Count; sieve[j] = false, j += i) ;
                }
            int fsqrt = (int)System.Math.Sqrt(sieve.Count);
            for (int i = System.Math.Max(primes.Last() + 2, fsqrt + 1 + (fsqrt & 1)); i < sieve.Count; i += 2)
                if (sieve[i])
                    primes.Add(i);
            nextCand = sieve.Count + ((sieve.Count + 1) & 1);
        }
    }

    public static class Primes
    {
        static List<int> primes = new List<int>() { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 91, 97 };
        static int nextCand = primes.Last() + 2;
        static ThreadQueue threadQueue = new ThreadQueue();

        public static int GetIth(int i)
        {
            threadQueue.Wait();
            if (i >= primes.Count)
                CalcUntilIthPrime(i + 1);
            return primes[i];
        }

        public static bool IsPrime(int n)
        {
            threadQueue.Wait();
            if (n < nextCand)
                return Contain(n);
            else if (n < nextCand * nextCand)
            {
                CalcUntilI(n);
                return primes.Last() == n;
            }
            else
            {
                CalcUntilI((int)System.Math.Sqrt(n));
                for (int i = 0; i < primes.Count; i++)
                    if (n % primes[i] == 0)
                        return false;
                return true;
            }
        }

        public static bool IsProbPrime(int n)
        {
            threadQueue.Wait();
            if (n <= nextCand)
                return Contain(n);
            else
            {
                CalcUntilI((int)System.Math.Sqrt(System.Math.Sqrt(n)));
                for (int i = 0; i < primes.Count; i++)
                    if (n % primes[i] == 0)
                        return false;
                return true;
            }
        }

        public static void CalcUntilIthPrime(int exclusiveI)
        {
            threadQueue.Wait();
            while (primes.Count < exclusiveI)
                TestNextCand();
            threadQueue.Finished();
        }

        public static void CalcUntilI(int inclusiveI)
        {
            threadQueue.Wait();
            if (inclusiveI < (nextCand << 1))
                while (primes.Last() < inclusiveI)
                    TestNextCand();
            else
                Sieve(inclusiveI);
            threadQueue.Finished();
        }

        static bool Contain(int n)
        {
            int u = 0;
            for (int o = primes.Count - 1; u != o;)
            {
                int m = (u + o) >> 1;
                if (primes[m] < n)
                    u = m + 1;
                else
                    o = m;
            }
            return primes[u] == n;
        }

        static void TestNextCand()
        {
            for (int i = 1; primes[i] * primes[i] <= nextCand; i++)
                if (nextCand % primes[i] == 0)
                {
                    nextCand += 2;
                    return;
                }
            primes.Add(nextCand);
            nextCand += 2;
        }

        static void Sieve(int exclusive)
        {
            var sieve = new BitArray(exclusive + 1, true);
            for (int i = 1; i < primes.Count; i++)
                for (int j = primes[i] * primes[i]; j < sieve.Count; sieve[j] = false, j += primes[i]) ;
            for (int i = nextCand; i * i <= sieve.Count; i += 2)
                if (sieve[i])
                {
                    primes.Add(i);
                    for (int j = i * i; j < sieve.Count; sieve[j] = false, j += i) ;
                }
            int fsqrt = (int)System.Math.Sqrt(sieve.Count);
            for (int i = System.Math.Max(primes.Last() + 2, fsqrt + 1 + (fsqrt & 1)); i < sieve.Count; i += 2)
                if (sieve[i])
                    primes.Add(i);
            nextCand = sieve.Count + ((sieve.Count + 1) & 1);
        }
    }
}