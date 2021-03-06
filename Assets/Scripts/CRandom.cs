﻿using System;
using System.Security.Cryptography;

namespace Assets.Scripts.Common
{
	public static class CRandom
	{
	    private static readonly byte[] Buffer = new byte[1024];
	    private static int _bufferOffset = Buffer.Length;
	    private static readonly RNGCryptoServiceProvider CryptoProvider = new RNGCryptoServiceProvider();

	    public static int Next()
	    {
	        if (_bufferOffset >= Buffer.Length)
	        {
	            FillBuffer();
	        }

	        var val = BitConverter.ToInt32(Buffer, _bufferOffset) & 0x7fffffff;

	        _bufferOffset += sizeof (int);

	        return val;
	    }


        // 0 <= result < maxValue
	    public static int Next(int maxValue)
	    {
	        return Next() % maxValue;
	    }

	    public static int Next(int minValue, int maxValue)
	    {
	        if (maxValue < minValue)
	        {
	            throw new ArgumentOutOfRangeException();
	        }

	        var range = maxValue - minValue;

	        return minValue + Next(range);
	    }

	    /// <summary>
	    /// Chance 0-100
	    /// </summary>
	    public static bool Chance(int chance)
	    {
	        return Next(0, 100) < chance;
	    }

        /// <summary>
	    /// Chance 0-100
	    /// </summary>
	    public static bool Chance(long chance)
        {
            return Next(0, 100) < chance;
        }

        /// <summary>
        /// Chance 0-1f
        /// </summary>
        public static bool Chance(float chance)
	    {
	        return Chance((int) (100 * chance));
	    }

        private static void FillBuffer()
        {
            CryptoProvider.GetBytes(Buffer);
            _bufferOffset = 0;
        }
	}
}
