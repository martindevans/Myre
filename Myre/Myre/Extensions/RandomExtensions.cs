﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Myre.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class RandomExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static Vector3 RandomNormalVector(this Random random)
        {

            float randomA = (float)random.NextDouble() * 2 - 1;
            float randomB = (float)random.NextDouble() * 2 - 1;
            float randomC = (float)random.NextDouble() * 2 - 1;
            var randomVector = Vector3.Normalize(new Vector3(randomA, randomB, randomC));
            return randomVector;
        }
    }
}
