﻿using System;

namespace DLib.Math.Function
{
    public class Power : Function
    {
        public double Exponent { get; private set; }
        public double Factor { get; private set; }

        public Power(double factor, double exponent)
        {
            Exponent = exponent;
            Factor = factor;
        }

        public static bool operator ==(Power a, Power b) => a.Equals(b);

        public static bool operator !=(Power a, Power b) => !a.Equals(b);

        public override bool Equals(object obj) => Exponent == ((Power)obj).Exponent && Factor == ((Power)obj).Factor;

        public static Power operator +(Power a, Power b) => a.Exponent == b.Exponent ? new Power(a.Factor + b.Factor, a.Exponent) : throw new ArgumentException("Exponents do not fit");

        public static Power operator -(Power a, Power b) => a.Exponent == b.Exponent ? new Power(a.Factor - b.Factor, a.Exponent) : throw new ArgumentException("Exponents do not fit");

        public static Power operator *(Power a, Power b) => new Power(a.Factor * b.Factor, a.Exponent + b.Exponent);

        public static Power operator /(Power a, Power b) => new Power(a.Factor / b.Factor, a.Exponent - b.Exponent);

        public static Power operator *(Power a, double b) => new Power(a.Factor * b, a.Exponent);

        public static Power operator *(double a, Power b) => b * a;

        public static Power operator /(Power a, double b) => a * (1 / b);

        public override Function Derivate() => Exponent == 0 ? new Power(0, 0) : new Power(Factor * Exponent, Exponent - 1);

        public override Function Integrate() => Exponent == -1 ? throw new Exception("integration not possible") : new Power(Factor / (Exponent + 1), Exponent + 1);

        public override double Y(double x) => Factor * System.Math.Pow(x, Exponent);

        public override double[] Roots() => new double[] { 0 };

        public double[] Intersection(Power a) => (new Polynomial(this) - new Polynomial(a)).Roots();

        public override Function Clone() => new Power(Factor, Exponent);

        public override string ToString() => Factor + "*" + "x^" + Exponent;

        public static implicit operator string(Power a) => a.ToString();

        public override Function Inverse() => new Power(1 / Factor, 1 / Exponent);

        public override double Limit(double p)
        {
            throw new NotImplementedException();
        }
    }
}
