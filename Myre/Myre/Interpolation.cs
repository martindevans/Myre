using System;

namespace Myre
{
    /// <summary>
    /// Various interpolation functions, courtesy of Rob Penner http://www.robertpenner.com/easing/
    /// </summary>
    public static class Interpolation
    {
        private const float EPSILON = float.Epsilon;

        #region Derivative
        public static Func<float, float> Derivative(this Func<float, float> interpolation, float dt)
        {
            return t => {
                var a = interpolation(t);
                var b = interpolation(t + dt);
                return b - a;
            };
        }
        #endregion

        #region none
        /// <summary>
        /// Returns a constant value, meaning no interpolation at all
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Func<float, float> None(float a = 0)
        {
            return _ => a;
        }
        #endregion

        #region Linear
        /// <summary>
        /// Easing equation function for a simple linear tweening, with no easing.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> Linear(float b = 0, float c = 1, float d = 1)
        {
            return t => c * t / d + b;
        }
        #endregion

        #region Expo
        /// <summary>
        /// Easing equation function for an exponential (2^t) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> ExpoEaseOut(float b = 0, float c = 1, float d = 1)
        {
            return t => (Math.Abs(t - d) < EPSILON) ? b + c : c * (-(float)Math.Pow(2, -10 * t / d) + 1) + b;
        }

        /// <summary>
        /// Easing equation function for an exponential (2^t) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> ExpoEaseIn(float b = 0, float c = 1, float d = 1)
        {
            return t => (Math.Abs(t - 0) < EPSILON) ? b : c * (float)Math.Pow(2, 10 * (t / d - 1)) + b;
        }

        /// <summary>
        /// Easing equation function for an exponential (2^t) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> ExpoEaseInOut(float b = 0, float c = 1, float d = 1)
        {
            return t =>
            {
                if (Math.Abs(t - 0) < EPSILON)
                    return b;

                if (Math.Abs(t - d) < EPSILON)
                    return b + c;

                if ((t /= d / 2) < 1)
                    return c / 2 * (float)Math.Pow(2, 10 * (t - 1)) + b;

                return c / 2 * (-(float)Math.Pow(2, -10 * --t) + 2) + b;
            };
        }

        /// <summary>
        /// Easing equation function for an exponential (2^t) easing out/in: 
        /// deceleration until halfway, then acceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> ExpoEaseOutIn(float b = 0, float c = 1, float d = 1)
        {
            var easeOut = ExpoEaseOut(b, c / 2, d);
            var easeIn = ExpoEaseIn(b + c / 2, c / 2, d);

            return t =>
            {
                if (t < d / 2)
                    return easeOut(t * 2);

                return easeIn((t * 2) - d);
            };
        }

        #endregion

        #region Circular
        /// <summary>
        /// Easing equation function for a circular (sqrt(1-t^2)) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> CircEaseOut(float b = 0, float c = 1, float d = 1)
        {
            return t => c * (float)Math.Sqrt(1 - (t = t / d - 1) * t) + b;
        }

        /// <summary>
        /// Easing equation function for a circular (sqrt(1-t^2)) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> CircEaseIn(float b = 0, float c = 1, float d = 1)
        {
            return t => -c * ((float)Math.Sqrt(1 - (t /= d) * t) - 1) + b;
        }

        /// <summary>
        /// Easing equation function for a circular (sqrt(1-t^2)) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> CircEaseInOut(float b = 0, float c = 1, float d = 1)
        {
            return t =>
            {
                if ((t /= d / 2) < 1)
                    return -c / 2 * ((float) Math.Sqrt(1 - t * t) - 1) + b;

                return c / 2 * ((float) Math.Sqrt(1 - (t -= 2) * t) + 1) + b;
            };
        }

        /// <summary>
        /// Easing equation function for a circular (sqrt(1-t^2)) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> CircEaseOutIn(float b = 0, float c = 1, float d = 1)
        {
            var easeOut = CircEaseOut(b, c / 2, d);
            var easeIn = CircEaseIn(b + c / 2, c / 2, d);

            return t =>
            {
                if (t < d / 2)
                    return easeOut(t * 2);

                return easeIn((t * 2) - d);
            };
        }
        #endregion

        #region Quad
        /// <summary>
        /// Easing equation function for a quadratic (t^2) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> QuadEaseOut(float b = 0, float c = 1, float d = 1)
        {
            return t => -c * (t /= d) * (t - 2) + b;
        }

        /// <summary>
        /// Easing equation function for a quadratic (t^2) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> QuadEaseIn(float b = 0, float c = 1, float d = 1)
        {
            return t => c * (t /= d) * t + b;
        }

        /// <summary>
        /// Easing equation function for a quadratic (t^2) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> QuadEaseInOut(float b = 0, float c = 1, float d = 1)
        {
            return t =>
            {
                if ((t /= d / 2) < 1)
                    return c / 2 * t * t + b;

                return -c / 2 * ((--t) * (t - 2) - 1) + b;
            };
        }

        /// <summary>
        /// Easing equation function for a quadratic (t^2) easing out/in: 
        /// deceleration until halfway, then acceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> QuadEaseOutIn(float b = 0, float c = 1, float d = 1)
        {
            var easeOut = QuadEaseOut(b, c / 2, d);
            var easeIn = QuadEaseIn(b + c / 2, c / 2, d);

            return t =>
            {
                if (t < d / 2)
                    return easeOut(t * 2);

                return easeIn((t * 2) - d);
            };
        }
        #endregion

        #region Sine
        /// <summary>
        /// Easing equation function for a sinusoidal (sin(t)) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> SineEaseOut(float b = 0, float c = 1, float d = 1)
        {
            return t => c * (float)Math.Sin(t / d * (Math.PI / 2)) + b;
        }

        /// <summary>
        /// Easing equation function for a sinusoidal (sin(t)) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> SineEaseIn(float b = 0, float c = 1, float d = 1)
        {
            return t => -c * (float)Math.Cos(t / d * (Math.PI / 2)) + c + b;
        }

        /// <summary>
        /// Easing equation function for a sinusoidal (sin(t)) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> SineEaseInOut(float b = 0, float c = 1, float d = 1)
        {
            return t =>
            {
                if ((t /= d / 2) < 1)
                    return c / 2 * ((float) Math.Sin(Math.PI * t / 2)) + b;

                return -c / 2 * ((float) Math.Cos(Math.PI * --t / 2) - 2) + b;
            };
        }

        /// <summary>
        /// Easing equation function for a sinusoidal (sin(t)) easing in/out: 
        /// deceleration until halfway, then acceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> SineEaseOutIn(float b = 0, float c = 1, float d = 1)
        {
            var easeOut = SineEaseOut(b, c / 2, d);
            var easeIn = SineEaseIn(b + c / 2, c / 2, d);

            return t =>
            {
                if (t < d / 2)
                    return easeOut(t * 2);

                return easeIn((t * 2) - d);
            };
        }
        #endregion

        #region Cubic
        /// <summary>
        /// Easing equation function for a cubic (t^3) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> CubicEaseOut(float b = 0, float c = 1, float d = 1)
        {
            return t => c * ((t = t / d - 1) * t * t + 1) + b;
        }

        /// <summary>
        /// Easing equation function for a cubic (t^3) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> CubicEaseIn(float b = 0, float c = 1, float d = 1)
        {
            return t => c * (t /= d) * t * t + b;
        }

        /// <summary>
        /// Easing equation function for a cubic (t^3) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> CubicEaseInOut(float b = 0, float c = 1, float d = 1)
        {
            return t =>
            {
                if ((t /= d / 2) < 1)
                    return c / 2 * t * t * t + b;

                return c / 2 * ((t -= 2) * t * t + 2) + b;
            };
        }

        /// <summary>
        /// Easing equation function for a cubic (t^3) easing out/in: 
        /// deceleration until halfway, then acceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> CubicEaseOutIn(float b = 0, float c = 1, float d = 1)
        {
            var easeIn = CubicEaseOut(b, c / 2, d);
            var easeOut = CubicEaseIn(b + c / 2, c / 2, d);

            return t =>
            {
                if (t < d / 2)
                    return easeIn(t * 2);

                return easeOut((t * 2) - d);
            };
        }
        #endregion

        #region Quartic
        /// <summary>
        /// Easing equation function for a quartic (t^4) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> QuartEaseOut(float b = 0, float c = 1, float d = 1)
        {
            return t => -c * ((t = t / d - 1) * t * t * t - 1) + b;
        }

        /// <summary>
        /// Easing equation function for a quartic (t^4) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> QuartEaseIn(float b = 0, float c = 1, float d = 1)
        {
            return t => c * (t /= d) * t * t * t + b;
        }

        /// <summary>
        /// Easing equation function for a quartic (t^4) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> QuartEaseInOut(float b = 0, float c = 1, float d = 1)
        {
            return t =>
            {
                if ((t /= d / 2) < 1)
                    return c / 2 * t * t * t * t + b;

                return -c / 2 * ((t -= 2) * t * t * t - 2) + b;
            };
        }

        /// <summary>
        /// Easing equation function for a quartic (t^4) easing out/in: 
        /// deceleration until halfway, then acceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> QuartEaseOutIn(float b = 0, float c = 1, float d = 1)
        {
            var easeOut = QuartEaseOut(b, c / 2, d);
            var easeIn = QuartEaseIn(b + c / 2, c / 2, d);

            return t =>
            {
                if (t < d / 2)
                    return easeOut(t * 2);

                return easeIn((t * 2) - d);
            };
        }
        #endregion

        #region Quintic
        /// <summary>
        /// Easing equation function for a quintic (t^5) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> QuintEaseOut(float b = 0, float c = 1, float d = 1)
        {
            return t => c * ((t = t / d - 1) * t * t * t * t + 1) + b;
        }

        /// <summary>
        /// Easing equation function for a quintic (t^5) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> QuintEaseIn(float b = 0, float c = 1, float d = 1)
        {
            return t => c * (t /= d) * t * t * t * t + b;
        }

        /// <summary>
        /// Easing equation function for a quintic (t^5) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> QuintEaseInOut(float b = 0, float c = 1, float d = 1)
        {
            return t =>
            {
                if ((t /= d / 2) < 1)
                    return c / 2 * t * t * t * t * t + b;
                return c / 2 * ((t -= 2) * t * t * t * t + 2) + b;
            };
        }

        /// <summary>
        /// Easing equation function for a quintic (t^5) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> QuintEaseOutIn(float b = 0, float c = 1, float d = 1)
        {
            var easeOut = QuintEaseOut(b, c / 2, d);
            var easeIn = QuintEaseIn(b + c / 2, c / 2, d);

            return t =>
            {
                if (t < d / 2)
                    return easeOut(t * 2);
                return easeIn((t * 2) - d);
            };
        }
        #endregion

        #region Elastic
        /// <summary>
        /// Easing equation function for an elastic (exponentially decaying sine wave) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> ElasticEaseOut(float b = 0, float c = 1, float d = 1)
        {
            return t =>
            {
                if (Math.Abs((t /= d) - 1) < EPSILON)
                    return b + c;

                float p = d * 0.3f;
                float s = p / 4;

                return (c * (float)Math.Pow(2, -10 * t) * (float)Math.Sin((t * d - s) * (2 * Math.PI) / p) + c + b);
            };
        }

        /// <summary>
        /// Easing equation function for an elastic (exponentially decaying sine wave) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> ElasticEaseIn(float b = 0, float c = 1, float d = 1)
        {
            return t =>
            {
                if (Math.Abs((t /= d) - 1) < EPSILON)
                    return b + c;

                float p = d * 0.3f;
                float s = p / 4;

                return -(c * (float) Math.Pow(2, 10 * (t -= 1)) * (float) Math.Sin((t * d - s) * (2 * Math.PI) / p)) + b;
            };
        }

        /// <summary>
        /// Easing equation function for an elastic (exponentially decaying sine wave) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> ElasticEaseInOut(float b = 0, float c = 1, float d = 1)
        {
            return t =>
            {
                if (Math.Abs((t /= d / 2) - 2) < EPSILON)
                    return b + c;

                float p = d * (0.3f * 1.5f);
                float s = p / 4;

                if (t < 1)
                    return (float)(-0.5f * (c * Math.Pow(2, 10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p)) + b);
                return (float)(c * Math.Pow(2, -10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p) * 0.5f + c + b);
            };
        }

        /// <summary>
        /// Easing equation function for an elastic (exponentially decaying sine wave) easing out/in: 
        /// deceleration until halfway, then acceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> ElasticEaseOutIn(float b = 0, float c = 1, float d = 1)
        {
            var easeOut = ElasticEaseOut(b, c / 2, d);
            var easeIn = ElasticEaseIn(b + c / 2, c / 2, d);

            return t =>
            {
                if (t < d / 2)
                    return easeOut(t * 2);
                return easeIn((t * 2) - d);
            };
        }
        #endregion

        #region Bounce
        /// <summary>
        /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> BounceEaseOut(float b = 0, float c = 1, float d = 1)
        {
            return t =>
            {
                if ((t /= d) < (1 / 2.75f))
                    return c * (7.5625f * t * t) + b;
                else if (t < (2 / 2.75f))
                    return c * (7.5625f * (t -= (1.5f / 2.75f)) * t + 0.75f) + b;
                else if (t < (2.5f / 2.75f))
                    return c * (7.5625f * (t -= (2.25f / 2.75f)) * t + 0.9375f) + b;
                else
                    return c * (7.5625f * (t -= (2.625f / 2.75f)) * t + 0.984375f) + b;
            };
        }

        /// <summary>
        /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> BounceEaseIn(float b = 0, float c = 1, float d = 1)
        {
            var bounceOut = BounceEaseOut(0, c, d);

            return t => c - bounceOut(d - t) + b;
        }

        /// <summary>
        /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> BounceEaseInOut(float b = 0, float c = 1, float d = 1)
        {
            var easeIn = BounceEaseIn(0, c, d);
            var easeOut = BounceEaseOut(0, c, d);

            return t =>
            {
                if (t < d / 2)
                    return easeIn(t * 2) * 0.5f + b;
                else
                    return easeOut(t * 2 - d) * 0.5f + c * 0.5f + b;
            };
        }

        /// <summary>
        /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing out/in: 
        /// deceleration until halfway, then acceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> BounceEaseOutIn(float b = 0, float c = 1, float d = 1)
        {
            var easeOut = BounceEaseOut(b, c / 2, d);
            var easeIn = BounceEaseIn(b + c / 2, c / 2, d);

            return t =>
            {
                if (t < d / 2)
                    return easeOut(t * 2);
                return easeIn((t * 2) - d);
            };
        }
        #endregion

        #region Back
        /// <summary>
        /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> BackEaseOut(float b = 0, float c = 1, float d = 1)
        {
            return t => c * ((t = t / d - 1) * t * ((1.70158f + 1) * t + 1.70158f) + 1) + b;
        }

        /// <summary>
        /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing in: 
        /// accelerating from zero velocity.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> BackEaseIn(float b = 0, float c = 1, float d = 1)
        {
            return t => c * (t /= d) * t * ((1.70158f + 1) * t - 1.70158f) + b;
        }

        /// <summary>
        /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing in/out: 
        /// acceleration until halfway, then deceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> BackEaseInOut(float b = 0, float c = 1, float d = 1)
        {
            return t =>
            {
                float s = 1.70158f;
                if ((t /= d / 2) < 1)
                    return c / 2 * (t * t * (((s *= (1.525f)) + 1) * t - s)) + b;
                return c / 2 * ((t -= 2) * t * (((s *= (1.525f)) + 1) * t + s) + 2) + b;
            };
        }

        /// <summary>
        /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing out/in: 
        /// deceleration until halfway, then acceleration.
        /// </summary>
        /// <param name="b">Starting value.</param>
        /// <param name="c">Final value.</param>
        /// <param name="d">Duration of animation.</param>
        /// <returns>A function which returns the correct value.</returns>
        public static Func<float, float> BackEaseOutIn(float b = 0, float c = 1, float d = 1)
        {
            var easeOut = BackEaseOut(b, c / 2, d);
            var easeIn = BackEaseIn(b + c / 2, c / 2, d);

            return t =>
            {
                if (t < d / 2)
                    return easeOut(t * 2);
                return easeIn((t * 2) - d);
            };
        }

        #endregion
    }
}
