using System;

namespace DedupSharp.Apps.ImageProcessing.Utilities
{
    public class GaussianRandom
    {
        private readonly Random random = new Random();
        private readonly double mean;
        private readonly double standardDeviation;

        /// <summary>
        /// Creates a new instance of a normally distributed random value generator
        /// using the specified mean and standard deviation.
        /// </summary>
        /// <param name="mean">The average value produced by this generator</param>
        /// <param name="standardDeviation">The amount of variation in the values produced by this generator</param>
        public GaussianRandom(double mean, double standardDeviation)
        {
            random = new Random();
            this.mean = mean;
            this.standardDeviation = standardDeviation;
        }

        /// <summary>
        /// Creates a new instance of a normally distributed random value generator
        /// using the specified mean, standard deviation and seed.
        /// </summary>
        /// <param name="mean">The average value produced by this generator</param>
        /// <param name="standardDeviation">The amount of variation in the values produced by this generator</param>
        /// <param name="seed">A number used to calculate a starting value for the pseudo-random number
        /// sequence. If a negative number is specified, the absolute value of the number
        /// is used.</param>
        public GaussianRandom(double mean, double standardDeviation, int seed)
        {
            random = new Random(seed);
            this.mean = mean;
            this.standardDeviation = standardDeviation;
        }

        /// <summary>
        /// Samples the distribution and returns a random integer
        /// </summary>
        /// <returns>A normally distributed random number rounded to the nearest integer</returns>
        public int NextInteger()
            => (int)Math.Floor(Next() + 0.5);


        /// <summary>
        /// Samples the distribution
        /// </summary>
        /// <returns>A random sample from a normal distribution</returns>
        public double Next()
        {
            double x = 0.0;

            // get the next value in the interval (0, 1) from the underlying uniform distribution
            while (x == 0.0 || x == 1.0)
                x = random.NextDouble();

            // transform uniform into normal
            return GaussianInverse(x, mean, standardDeviation);
        }

        /// <summary>
        /// Calculates an approximation of the inverse of the cumulative normal distribution.
        /// </summary>
        /// <param name="cumulativeDistribution">The percentile as a fraction (.50 is the fiftieth percentile).
        /// Must be greater than 0 and less than 1.</param>
        /// <param name="mean">The underlying distribution's average (i.e., the value at the 50th percentile) (</param>
        /// <param name="standardDeviation">The distribution's standard deviation</param>
        /// <returns>The value whose cumulative normal distribution (given mean and stddev) is the percentile given as an argument.</returns>
        public static double GaussianInverse(double cumulativeDistribution, double mean, double standardDeviation)
        {
            if (!(0.0 < cumulativeDistribution && cumulativeDistribution < 1.0))
                throw new ArgumentOutOfRangeException("cumulativeDistribution");

            double result = GaussianInverse(cumulativeDistribution);
            return mean + result * standardDeviation;
        }

        // Adaptation of Peter J. Acklam's Perl implementation. See http://home.online.no/~pjacklam/notes/invnorm/
        // This approximation has a relative error of 1.15 × 10−9 or less.
        private static double GaussianInverse(double value)
        {
            // Lower and upper breakpoints
            const double plow = 0.02425;
            const double phigh = 1.0 - plow;

            double p = (phigh < value) ? 1.0 - value : value;
            double sign = (phigh < value) ? -1.0 : 1.0;
            double q;

            if (p < plow)
            {
                // Rational approximation for tail
                var c = new double[]{-7.784894002430293e-03, -3.223964580411365e-01,
                                         -2.400758277161838e+00, -2.549732539343734e+00,
                                         4.374664141464968e+00, 2.938163982698783e+00};

                var d = new double[]{7.784695709041462e-03, 3.224671290700398e-01,
                                       2.445134137142996e+00, 3.754408661907416e+00};
                q = Math.Sqrt(-2 * Math.Log(p));
                return sign * (((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) /
                                                ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1);
            }
            else
            {
                // Rational approximation for central region
                var a = new double[]{-3.969683028665376e+01, 2.209460984245205e+02,
                                         -2.759285104469687e+02, 1.383577518672690e+02,
                                         -3.066479806614716e+01, 2.506628277459239e+00};

                var b = new double[]{-5.447609879822406e+01, 1.615858368580409e+02,
                                         -1.556989798598866e+02, 6.680131188771972e+01,
                                         -1.328068155288572e+01};
                q = p - 0.5;
                var r = q * q;
                return (((((a[0] * r + a[1]) * r + a[2]) * r + a[3]) * r + a[4]) * r + a[5]) * q /
                                         (((((b[0] * r + b[1]) * r + b[2]) * r + b[3]) * r + b[4]) * r + 1);
            }
        }
    }
}