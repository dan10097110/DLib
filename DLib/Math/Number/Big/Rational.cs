﻿namespace DLib.Math.Number.Big
{
    public class Rational : INumber
    {
        public Integer Numerator { get; private set; }
        public Natural Denominator { get; private set; }


        public Rational() { }

        public Rational(double d)
        {
            Numerator = (long)(d * 100000000);
            Denominator = 100000000;
            Shorten();
        }

        public Rational(Integer numerator, Natural denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
            Shorten();
        }

        public Rational(Integer integer)
        {
            Numerator = integer.Clone();
            Denominator = Natural.One;
        }

        public Rational(Rational rational)
        {
            Numerator = rational.Numerator.Clone();
            Denominator = rational.Denominator.Clone();
        }


        void Shorten()
        {
            Natural gcd = Natural.GCD(Denominator, (Natural)Numerator);
            Numerator /= gcd;
            Denominator /= gcd;
        }


        public static bool operator ==(Rational a, Rational b) => Compare(a, b) == 0;

        public static bool operator !=(Rational a, Rational b) => Compare(a, b) != 0;

        public static bool operator <(Rational a, Rational b) => Compare(a, b) == -1;

        public static bool operator >(Rational a, Rational b) => Compare(a, b) == 1;

        public static bool operator <=(Rational a, Rational b) => Compare(a, b) != 1;

        public static bool operator >=(Rational a, Rational b) => Compare(a, b) != -1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>a < b: - 1; a == b: 0; a > b: == 1</b></returns>
        public static int Compare(Rational a, Rational b)
        {
            Natural lcm = Natural.LCM(a.Denominator, b.Denominator);
            return Integer.Compare(a.Numerator * lcm / a.Denominator, b.Numerator * lcm / a.Denominator);
        }


        public static Rational operator ++(Rational a) => a + Natural.One;

        public static Rational operator --(Rational a) => a - Natural.One;

        public static Rational operator +(Rational a, Rational b)
        {
            Rational r = new Rational() { Numerator = a.Numerator * b.Denominator + b.Numerator * a.Denominator, Denominator = a.Denominator * b.Denominator };
            r.Shorten();
            return r;
        }

        public static Rational operator +(Rational a, Integer b)
        {
            Rational r = new Rational() { Numerator = a.Numerator + b * a.Denominator, Denominator = a.Denominator };
            r.Shorten();
            return r;
        }

        public static Rational operator -(Rational a, Rational b)
        {
            Rational r = new Rational() { Numerator = a.Numerator * b.Denominator - b.Numerator * a.Denominator, Denominator = a.Denominator * b.Denominator };
            r.Shorten();
            return r;
        }

        public static Rational operator -(Rational a, Integer b)
        {
            Rational r = new Rational() { Numerator = a.Numerator - b * a.Denominator, Denominator = a.Denominator };
            r.Shorten();
            return r;
        }

        public static Rational operator *(Rational a, Rational b)
        {
            Rational r = new Rational() { Numerator = a.Numerator * b.Numerator, Denominator = a.Denominator * b.Denominator };
            r.Shorten();
            return r;
        }

        public static Rational operator *(Rational a, Integer b)
        {
            Rational r = new Rational() { Numerator = a.Numerator * b, Denominator = a.Denominator };
            r.Shorten();
            return r;
        }

        public static Rational operator /(Rational a, Rational b)
        {
            Rational r = new Rational() { Numerator = a.Numerator * b.Denominator * a.Numerator.Sign(), Denominator = a.Numerator.Abs() * b.Denominator };
            r.Shorten();
            return r;
        }

        public static Rational operator /(Rational a, Integer b)
        {
            Rational r = new Rational() { Numerator = a.Numerator * b.Sign(), Denominator = a.Denominator * b.Abs() };
            r.Shorten();
            return r;
        }

        public static Rational operator <<(Rational a, int i)
        {
            var r = new Rational() { Numerator = a.Numerator << i, Denominator = a.Denominator };
            r.Shorten();
            return r;
        }

        public static Rational operator >>(Rational a, int i)
        {
            var r = new Rational() { Numerator = a.Numerator, Denominator = a.Denominator << i };
            r.Shorten();
            return r;
        }


        public Rational Sqrt(int iterations)
        {
            Rational root = (Integer)Natural.One;
            for (int i = 0; i < iterations; root = (root + (Numerator * root.Denominator) / (Denominator * root.Numerator)) >> 1, i++) ;
            return root;
        }


        public Rational Reciprocal() => new Rational() { Numerator = (Integer)Denominator * Numerator.Sign(), Denominator = Numerator.Abs() };

        public Integer Round() => IntegerPart();

        public Rational FractionalPart() => new Rational(Numerator % Denominator, Denominator);

        public Integer IntegerPart() => Numerator / Denominator;


        public bool IsZero() => Numerator.IsZero();

        public bool IsInteger() => Denominator == 1;


        public Rational Clone() => new Rational(this);
        
        public override string ToString() => ToDecimal().ToString();

        public double ToDecimal() => (this << 10).Round().ToDecimal() / System.Math.Pow(2, 10);

        public static implicit operator Rational(Natural natural) => new Rational(natural);

        public static implicit operator Rational(Integer integer) => new Rational(integer);

        public static implicit operator Rational(double u) => new Rational(u);

        public static explicit operator Integer(Rational rational) => rational.Round();

        public static implicit operator string(Rational n) => n.ToString();
    }
}
