﻿using DLib.Collection;
using System;
using System.Collections.Generic;

namespace DLib.Math.Number.Big
{
    public class Natural : ParallelExecutionObject, INumber
    {
        public Bits bits;


        public static Natural Zero => new Natural() { bits = new Bits() };

        public static Natural One => new Natural() { bits = new Bits(true) };

        public static Natural Two => new Natural() { bits = new Bits(false, true) };

        public static Natural Three => new Natural() { bits = new Bits(true, true) };


        public Natural() { }

        public Natural(ulong u)
        {
            int length = 0;
            for (ulong u1 = u; u1 > 0; u1 >>= 1, length++) ;
            bits = new Bits(length);
            for (int i = 0; i < length; u >>= 1, i++)
                bits[i] = (u & 1) == 1 ? true : false;
        }

        public Natural(Bits b) => bits = b.Clone();

        public Natural(Natural a) => bits = a.bits.Clone();


        public static bool operator ==(Natural a, Natural b) => Compare(a, b) == 0;

        public static bool operator !=(Natural a, Natural b) => Compare(a, b) != 0;

        public static bool operator <(Natural a, Natural b) => Compare(a, b) == -1;

        public static bool operator >(Natural a, Natural b) => Compare(a, b) == 1;

        public static bool operator <=(Natural a, Natural b) => Compare(a, b) != 1;

        public static bool operator >=(Natural a, Natural b) => Compare(a, b) != -1;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>a < b: - 1; a == b: 0; a > b: == 1</b></returns>
        public static int Compare(Natural a, Natural b)
        {
            for (int i = System.Math.Max(a.bits.Length, b.bits.Length); i >= 0; i--)
                if (a.bits[i] != b.bits[i])
                    return a.bits[i] ? 1 : -1;
            return 0;
        }


        public static Natural operator ++(Natural a) => a + One;

        public static Natural operator --(Natural a) => a - One;

        public static Natural operator +(Natural a, Natural b) => Add2(new Natural[] { a, b });

        public static Natural operator -(Natural a, Natural b) => Sub(a, b);

        public static Natural operator *(Natural a, Natural b) => Mul(a, b);

        public static Natural operator /(Natural a, Natural m)
        {
            if (m == Natural.Zero)
                throw new DivideByZeroException();
            Collection.Bits b = new Collection.Bits();
            Natural r = a.Clone(), n = m.Clone();
            n.bits.Shift(r.bits.Length - m.bits.Length + 1);
            for (int i = r.bits.Length - m.bits.Length + 1; r >= m; r -= n, b[i] = true)
                for (; r < n; i--)
                    n.bits.Shift(-1);
            return b;
        }

        public static Natural operator %(Natural a, Natural m)
        {
            Natural r = a.Clone(), n = m.Clone();
            n.bits.Shift(r.bits.Length - m.bits.Length + 1);
            for (; r >= m; r -= n)
                while (r < n)
                    n.bits.Shift(-1);
            return r;
        }

        public static Natural operator &(Natural a, int i)
        {
            Collection.Bits bits = new Collection.Bits(i);
            Loop(0, i, j => bits[j] = a.bits[j]);
            return bits;
        }

        public static Natural operator <<(Natural a, int i) => a.bits << i;

        public static Natural operator >>(Natural a, int i) => a.bits >> i;

        
        public static Natural Add(Natural a, Natural b)
        {
            Collection.Bits r = a.bits.Clone(), d = b.bits.Clone();
            while (d != Zero)
            {
                Collection.Bits c = r & d;
                c.Shift(1);
                r = r ^ d;
                d = c;
            }
            return r;
        }
        
        public static Natural Add2(Natural[] n)
        {
            int length = n[0].bits.Length;
            for (int i = 1; i < n.Length; i++)
                if (n[i].bits.Length > length)
                    length = n[i].bits.Length;
            for (int c = n.Length; c > 0; length++, c >>= 1) ;
            var bits = new Collection.Bits(length);
            for (int j = 0, puffer = 0; j < length; bits[j] = (puffer & 1) == 1, puffer >>= 1, j++)
                for (int i = 0; i < n.Length; i++)
                    if (n[i].bits[j])
                        puffer++;
            return bits;
        }

        static Natural internalAdd(int[] bits)
        {
            var b = new Collection.Bits(bits.Length);
            for (int i = 0; i < bits.Length - 1; bits[i + 1] += (bits[i] >> 1), b[i] = (bits[i] & 1) == 1, i++) ;
            return b;
        }
        
        public static Natural Sub(Natural a, Natural b)
        {
            Natural c = a + b.bits.Not(a.bits.Length) + One;
            c.bits[System.Math.Max(0, c.bits.Length - 1)] = false;
            return c;
        }
        
        //doesnt work
        public static Natural Sub2(Natural a, Natural[] n)
        {
            var bits = new Collection.Bits(a.bits.Length);
            for (int j = 0, puffer = 0; j < a.bits.Length; bits[j] = (puffer & 1) == 0 && a.bits[j], puffer >>= 1, j++)
                for (int i = 0; i < n.Length; i++)
                    if (n[i].bits[j])
                        puffer++;
            return bits;
        }
        
        public static Natural AdditiveOperation(Natural a, Natural b, bool sub)
        {
            Collection.Bits r = a.bits.Clone();
            for (int i = 0; i < b.bits.Length; i++)
                if (b.bits[i])
                    for (int j = i; (r[j] = !r[j]) == sub; j++) ;
            return r;
        }
        
        public static Natural Mul(Natural a, Natural b)
        {
            var n = new int[a.bits.Length + b.bits.Length + 1];
            for (int i = 0; i < b.bits.Length; i++)
                if (b.bits[i])
                    for (int j = 0; j < a.bits.Length; j++)
                        if (a.bits[j])
                            n[j + i]++;
            return internalAdd(n);
        }
        
        public static Natural Karatsuba(Natural x, Natural y)
        {
            if (x.bits.Length == 0 || y.bits.Length == 0)
                return Zero;
            if (x.bits.Length == 1 || y.bits.Length == 1)
                return x.bits.Length == 1 ? y : x;
            int i = System.Math.Max(x.bits.Length, y.bits.Length) >> 1;
            Natural a = x >> i, b = x & i, c = y >> i, d = y & i, ac = Karatsuba(a, c), bd = Karatsuba(b, d);
            return (ac << (i << 1)) + ((Karatsuba(a + b, c + d) - ac - bd) << i) + bd;
        }
        
        public static Natural KaratsubaCombined(Natural x, Natural y)
        {
            if (x.bits.Length == 0 || y.bits.Length == 0)
                return Zero;
            if (x.bits.Length == 1 || y.bits.Length == 1)
                return x.bits.Length == 1 ? y : x;
            if (x.bits.Length < 128 && y.bits.Length < 128)
                return Mul(x, y);
            int i = System.Math.Max(x.bits.Length, y.bits.Length) >> 1;
            Natural a = x >> i, b = x & i, c = y >> i, d = y & i, ac = Karatsuba(a, c), bd = Karatsuba(b, d);
            return (ac << (i << 1)) + ((Karatsuba(a + b, c + d) - ac - bd) << i) + bd;
        }
        
        /// <summary>
        /// x > y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Natural QuarterSquareMultiplication(Natural x, Natural y)
        {
            if (x < y)
                return (Square(y + x) >> 2) - (Square(y - x) >> 2);
            return (Square(x + y) >> 2) - (Square(x - y) >> 2);
        }
        
        /// <summary>
        /// x > y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Natural QuarterSquareMultiplication2(Natural x, Natural y)
        {
            if (x < y)
                return Square((y + x) >> 1) - Square((y - x) >> 1) + (x.bits[0] != y.bits[0] ? x : Zero);
            return Square((x + y) >> 1) - Square((x - y) >> 1) + (x.bits[0] != y.bits[0] ? y : Zero);
        }
        
        public static Natural PowerOfTwo(int e) => new Bits(e + 1) { [e] = true };

        public static Natural MersenneNumber(int e) => new Bits(e, true);
        
        public static Natural Square(Natural n)
        {
            var bits = new int[((n.bits.Length + 1) << 1)];
            for (int i = 0; i < n.bits.Length; i++)
            {
                if (n.bits[n.bits.Length - 1 - i])
                    for (int j = 0; j < n.bits.Length - 1 - i; j++)
                        if (n.bits[j])
                            bits[n.bits.Length + j - i]++;
                if (n.bits[i])
                    bits[i << 1]++;
            }
            return internalAdd(bits);
        }
        
        public static Natural Power(Natural b, Natural e)
        {
            Natural r = One, a = b.Clone();
            for (int i = 0; i < e.bits.Length; a = Square(a), i++)
                if (e.bits[i])
                    r *= a;
            return r;
        }

        public static Natural PowerMod(Natural b, Natural e, Natural m)
        {
            Natural r = One, a = b % m;
            for (int i = 0; i < e.bits.Length; a = Square(a) % m, i++)
                if (e.bits[i])
                    r = (r * a) % m;
            return r;
        }

        public static Natural PowerModMersenneNumber(Natural b, Natural e, int exponent)
        {
            Natural r = One, a = ModMersenneNumber(b, exponent);
            for (int i = 0; i < e.bits.Length; a = ModMersenneNumber(Square(a), exponent), i++)
                if (e.bits[i])
                    r = ModMersenneNumber(r * a, exponent);
            return r;
        }

        public static Natural PowerMulMod(Natural b, Natural e, Natural m)
        {
            Natural r = One, a = b % m;
            for (int i = 0; i < e.bits.Length; a = MulMod(a, a, m), i++)
                if (e.bits[i])
                    r = MulMod(r, a, m);
            return r;
        }
        
        public static Natural MulMod(Natural a, Natural b, Natural m)
        {
            Natural r = Zero, s = a % m, t = b % m;
            for (int i = 0; i < t.bits.Length && s > Zero; s %= m, i++)
            {
                if (t.bits[i])
                    r = (r + s) % m;
                s.bits.Shift(1);
            }
            return r;
        }
        
        public static Natural ModMersenneNumber(Natural n, int exponent)
        {
            Natural s = n.Clone();
            for (; s.bits.Length > exponent; s = (s & exponent) + (s >> exponent)) ;
            return s == MersenneNumber(exponent) ? Zero : s;
        }

        public static Natural ModMersenneNumber2(Natural n, int exponent)
        {
            Natural s = n.Clone();
            for (; s.bits.Length > exponent;)
            {
                int l = System.Math.Max(exponent, s.bits.Length - exponent) + 1;
                Collection.Bits r = new Collection.Bits(l);
                bool b = false;
                for (int j = 0; j < l; s.bits[j] = j < exponent && s.bits[j], r[j] = s.bits[j] == s.bits[j + exponent] == b, b = (b && (s.bits[j] || s.bits[j + exponent])) || (s.bits[j] && s.bits[j + exponent]), j++) ;
                s = r;
            }
            return s == MersenneNumber(exponent) ? Zero : s;
        }


        public static Natural GCD(Natural a, Natural b)
        {
            for (; b != 0;)
            {
                Natural c = a % b;
                a = b;
                b = c;
            }
            return a;
        }

        public static Natural LCM(Natural a, Natural b) => a * b / GCD(a, b);


        public static bool LucasLehmerTest(int exponent)
        {
            Natural a = Three;
            for (int i = 1; i < exponent; a = ModMersenneNumber2(Square(a), exponent), i++) ;
            return new Bits(exponent, true) { [0] = false, [1] = false } == a;
        }

        public static bool LucasLehmerTest2(int exponent)
        {
            Natural a = Three;
            for (int i = 1; i < exponent; i++)
                for (a = Square(a); a.bits.Length > exponent;)
                {
                    int l = System.Math.Max(exponent, a.bits.Length - exponent) + 1;
                    Bits r = new Bits(l);
                    bool b = false;
                    for (int j = 0; j < l; a.bits[j] = j < exponent && a.bits[j], r[j] = a.bits[j] == a.bits[j + exponent] == b, b = (b && (a.bits[j] || a.bits[j + exponent])) || (a.bits[j] && a.bits[j + exponent]), j++) ;
                    a = r;
                }
            return new Bits(exponent, true) { [0] = false, [1] = false } == a;
        }

        public static class MersennePrime
        {
            //1,3
            public static bool Test(ulong exponent, ref ulong startI, ref Natural startS, List<ulong> primes)
            {
                if (!DLib.Math.Prime.Test.Probabilistic.Division(exponent, primes, 3, (ulong)System.Math.Sqrt(exponent) + 1))
                    return false;
                lock (primes)
                {
                    int i = primes.Count;
                    for (; i > 0 && primes[i - 1] > exponent; i--) ;
                    if (i > 0 && primes[i - 1] != exponent)
                        primes.Insert(i, exponent);
                }
                if ((exponent & 3) == 3 && DLib.Math.Prime.Test.Probabilistic.Division((exponent << 1) + 1, primes, 3, (ulong)System.Math.Sqrt((exponent << 1) + 1) + 1))
                    return false;
                Natural mersenneNumber = MersenneNumber((int)exponent);
                return TrialDivision(exponent, mersenneNumber, primes) && LucasLehmerTest(exponent, mersenneNumber, ref startI, ref startS);
            }

            static bool LucasLehmerTest(ulong exponent, Natural mersenneNumber, ref ulong startI, ref Natural startS)//3815
            {
                Natural s = startS.Clone();
                for (ulong i = startI; i < exponent; s = ModMersenneNumber2(Square(s), (int)exponent), i++) ;
                if (startS < mersenneNumber)
                {
                    startI++;
                    startS *= startS;
                }
                return s == mersenneNumber - 3;
            }

            static bool TrialDivision(ulong exponent, Natural mersenneNumber, List<ulong> primes)
            {
                int i = 0;
                switch (exponent % 60)
                {
                    case 1: i = 7; break;
                    case 7: i = 4; break;
                    case 11: i = 15; break;
                    case 13: i = 3; break;
                    case 17: i = 5; break;
                    case 19: i = 1; break;
                    case 23: i = 13; break;
                    case 29: i = 2; break;
                    case 31: i = 11; break;
                    case 37: i = 0; break;
                    case 41: i = 12; break;
                    case 43: i = 8; break;
                    case 47: i = 10; break;
                    case 49: i = 14; break;
                    case 53: i = 9; break;
                    case 59: i = 6; break;
                }
                ulong two = exponent << 1, six = 3 * two, ten = 5 * two;
                var d = new ulong[] { six, ten, exponent << 3, six, ten, six, two, six, ten, six, exponent << 3, ten, six, two, 22 * exponent, two };
                for (ulong dividend = 1, limit = exponent * exponent; (dividend += d[i]) < limit; i = (i + 1) & 15)
                    if (DLib.Math.Prime.Test.Probabilistic.Division(dividend, primes, 7, (ulong)System.Math.Sqrt(System.Math.Sqrt(dividend))) && (mersenneNumber % (Natural)dividend) == 0)
                        return false;
                return true;
            }
        }

        
        public Natural Clone() => new Natural(this);
        
        public override string ToString() => ToDecimal().ToString();

        public string ToBinaryString() => bits.ToString();

        public ulong ToDecimal()
        {
            ulong d = 0, u = 1;
            for (int i = 0; i < bits.Length; i++, u <<= 1)
                if (bits[i])
                    d += u;
            return d;
        }

        public static implicit operator Natural(Bits bits) => new Natural() { bits = bits };

        public static implicit operator Natural(ulong u) => new Natural(u);

        public static implicit operator string(Natural n) => n.ToString();

        public static explicit operator ulong(Natural natural) => natural.ToDecimal();
    }
}