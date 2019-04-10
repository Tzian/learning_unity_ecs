#region This is derived from the following
// FastNoise.cs
//
// MIT License
//
// Copyright(c) 2017 Jordan Peck
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// The developer's email is jorzixdan.me2@gzixmail.com (for great email, take
// off every 'zix'.)
//

// Uncomment the line below to swap all the inputs/outputs/calculations of FastNoise to doubles instead of floats
//#define FN_USE_DOUBLES
#endregion

using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

public struct JobifiedNoise 
{
    const Int16 FN_INLINE = 256; //(Int16)MethodImplOptions.AggressiveInlining;
    const int FN_CELLULAR_INDEX_MAX = 3;


    public enum NoiseType { Value, ValueFractal, Perlin, PerlinFractal, Simplex, SimplexFractal, Cellular, WhiteNoise, Cubic, CubicFractal };
    public enum Interp { Linear, Hermite, Quintic };
    public enum FractalType { FBM, Billow, RigidMulti };
    public enum CellularDistanceFunction { Euclidean, Manhattan, Natural };
    public enum CellularReturnType { CellValue, Distance, Distance2, Distance2Add, Distance2Sub, Distance2Mul, Distance2Div };

    int m_seed;
    float m_frequency;
    Interp m_interp;
    NoiseType m_noiseType;

    int m_octaves;
    float m_lacunarity;
    float m_gain;
    FractalType m_fractalType;

    float m_fractalBounding;
    CellularDistanceFunction m_cellularDistanceFunction;
    CellularReturnType m_cellularReturnType;
    int m_cellularDistanceIndex0;
    int m_cellularDistanceIndex1;
    float m_cellularJitter;

    float m_gradientPerturbAmp;
    CELL_2D cell2D;
    CELL_3D cell3D;
    Grad_2D grad2D;
    Grad_3D grad3D;
    SIMPLEX_4D simplex4D;


    public void FastNoise(int seed = 1337, float frequency = 0.1f, Interp interp = Interp.Quintic, NoiseType noiseType = NoiseType.Simplex, int octaves = 3, float lacunarity = 2.0f, float gain = 0.5f, FractalType fractalType = FractalType.FBM, float gradientPerturbAmp = 1.0f)
    {
        m_seed = seed;
        m_frequency = frequency;
        m_interp = interp;
        m_noiseType = noiseType;
        m_octaves = octaves;
        m_lacunarity = lacunarity;
        m_gain = gain;
        m_fractalType = fractalType;
        m_fractalBounding = 0;
        m_gradientPerturbAmp = gradientPerturbAmp;

        cell2D = new CELL_2D();
        cell3D = new CELL_3D();
        grad2D = new Grad_2D();
        grad3D = new Grad_3D();
        simplex4D = new SIMPLEX_4D();
        
        CalculateFractalBounding();
    }

    // Returns a 0 float/double
    public float GetDecimalType() { return 0; }

    // Returns the seed used by this object
    public int GetSeed() { return m_seed; }

    // Sets seed used for all noise types
    // Default: 1337
    public void SetSeed(int seed) { m_seed = seed; }

    // Sets frequency for all noise types
    // Default: 0.01
    public void SetFrequency(float frequency) { m_frequency = frequency; }

    // Changes the interpolation method used to smooth between noise values
    // Possible interpolation methods (lowest to highest quality) :
    // - Linear
    // - Hermite
    // - Quintic
    // Used in Value, Gradient Noise and Position Perturbing
    // Default: Quintic
    public void SetInterp(Interp interp) { m_interp = interp; }

    // Sets noise return type of GetNoise(...)
    // Default: Simplex
    public void SetNoiseType(NoiseType noiseType) { m_noiseType = noiseType; }

    // Sets octave count for all fractal noise types
    // Default: 3
    public void SetFractalOctaves(int octaves) { m_octaves = octaves; CalculateFractalBounding(); }

    // Sets octave lacunarity for all fractal noise types
    // Default: 2.0
    public void SetFractalLacunarity(float lacunarity) { m_lacunarity = lacunarity; }

    // Sets octave gain for all fractal noise types
    // Default: 0.5
    public void SetFractalGain(float gain) { m_gain = gain; CalculateFractalBounding(); }

    // Sets method for combining octaves in all fractal noise types
    // Default: FBM
    public void SetFractalType(FractalType fractalType) { m_fractalType = fractalType; }

    // Sets return type from cellular noise calculations
    // Note: NoiseLookup requires another FastNoise object be set with SetCellularNoiseLookup() to function
    // Default: CellValue
    public void SetCellularDistanceFunction(CellularDistanceFunction cellularDistanceFunction) { m_cellularDistanceFunction = cellularDistanceFunction; }

    // Sets distance function used in cellular noise calculations
    // Default: Euclidean
    public void SetCellularReturnType(CellularReturnType cellularReturnType) { m_cellularReturnType = cellularReturnType; }

    // Sets the 2 distance indicies used for distance2 return types
    // Default: 0, 1
    // Note: index0 should be lower than index1
    // Both indicies must be >= 0, index1 must be < 4
    public void SetCellularDistance2Indicies(int cellularDistanceIndex0, int cellularDistanceIndex1)
    {
        m_cellularDistanceIndex0 = Math.Min(cellularDistanceIndex0, cellularDistanceIndex1);
        m_cellularDistanceIndex1 = Math.Max(cellularDistanceIndex0, cellularDistanceIndex1);

        m_cellularDistanceIndex0 = Math.Min(Math.Max(m_cellularDistanceIndex0, 0), FN_CELLULAR_INDEX_MAX);
        m_cellularDistanceIndex1 = Math.Min(Math.Max(m_cellularDistanceIndex1, 0), FN_CELLULAR_INDEX_MAX);
    }

    // Sets the maximum distance a cellular point can move from it's grid position
    // Setting this high will make artifacts more common
    // Default: 0.45
    public void SetCellularJitter(float cellularJitter) { m_cellularJitter = cellularJitter; }

    // Sets the maximum perturb distance from original location when using GradientPerturb{Fractal}(...)
    // Default: 1.0
    public void SetGradientPerturbAmp(float gradientPerturbAmp) { m_gradientPerturbAmp = gradientPerturbAmp; }

    [MethodImpl(FN_INLINE)]
    int FastFloor(float f) { return (f >= 0 ? (int)f : (int)f - 1); }

    [MethodImpl(FN_INLINE)]
    int FastRound(float f) { return (f >= 0) ? (int)(f + (float)0.5) : (int)(f - (float)0.5); }

    [MethodImpl(FN_INLINE)]
    float Lerp(float a, float b, float t) { return a + t * (b - a); }

    [MethodImpl(FN_INLINE)]
    float InterpHermiteFunc(float t) { return t * t * (3 - 2 * t); }

    [MethodImpl(FN_INLINE)]
    float InterpQuinticFunc(float t) { return t * t * t * (t * (t * 6 - 15) + 10); }

    [MethodImpl(FN_INLINE)]
    float CubicLerp(float a, float b, float c, float d, float t)
    {
        float p = (d - c) - (a - b);
        return t * t * t * p + t * t * ((a - b) - p) + t * (c - a) + b;
    }

    void CalculateFractalBounding()
    {
        float amp = m_gain;
        float ampFractal = 1;
        for (int i = 1; i < m_octaves; i++)
        {
            ampFractal += amp;
            amp *= m_gain;
        }
        m_fractalBounding = 1 / ampFractal;
    }

    // Hashing
    const int X_PRIME = 1619;
    const int Y_PRIME = 31337;
    const int Z_PRIME = 6971;
    const int W_PRIME = 1013;

    [MethodImpl(FN_INLINE)]
    int Hash2D(int seed, int x, int y)
    {
        int hash = seed;
        hash ^= X_PRIME * x;
        hash ^= Y_PRIME * y;

        hash = hash * hash * hash * 60493;
        hash = (hash >> 13) ^ hash;

        return hash;
    }

    [MethodImpl(FN_INLINE)]
    int Hash3D(int seed, int x, int y, int z)
    {
        int hash = seed;
        hash ^= X_PRIME * x;
        hash ^= Y_PRIME * y;
        hash ^= Z_PRIME * z;

        hash = hash * hash * hash * 60493;
        hash = (hash >> 13) ^ hash;

        return hash;
    }

    [MethodImpl(FN_INLINE)]
    int Hash4D(int seed, int x, int y, int z, int w)
    {
        int hash = seed;
        hash ^= X_PRIME * x;
        hash ^= Y_PRIME * y;
        hash ^= Z_PRIME * z;
        hash ^= W_PRIME * w;

        hash = hash * hash * hash * 60493;
        hash = (hash >> 13) ^ hash;

        return hash;
    }

    [MethodImpl(FN_INLINE)]
    float ValCoord2D(int seed, int x, int y)
    {
        int n = seed;
        n ^= X_PRIME * x;
        n ^= Y_PRIME * y;

        return (n * n * n * 60493) / (float)2147483648.0;
    }

    [MethodImpl(FN_INLINE)]
    float ValCoord3D(int seed, int x, int y, int z)
    {
        int n = seed;
        n ^= X_PRIME * x;
        n ^= Y_PRIME * y;
        n ^= Z_PRIME * z;

        return (n * n * n * 60493) / (float)2147483648.0;
    }

    [MethodImpl(FN_INLINE)]
    float ValCoord4D(int seed, int x, int y, int z, int w)
    {
        int n = seed;
        n ^= X_PRIME * x;
        n ^= Y_PRIME * y;
        n ^= Z_PRIME * z;
        n ^= W_PRIME * w;

        return (n * n * n * 60493) / (float)2147483648.0;
    }

    [MethodImpl(FN_INLINE)]
    float GradCoord2D(int seed, int x, int y, float xd, float yd)
    {
        int hash = seed;
        hash ^= X_PRIME * x;
        hash ^= Y_PRIME * y;

        hash = hash * hash * hash * 60493;
        hash = (hash >> 13) ^ hash;

        float2 g = grad2D[hash & 7];

        return xd * g.x + yd * g.y;
    }

    [MethodImpl(FN_INLINE)]
    float GradCoord3D(int seed, int x, int y, int z, float xd, float yd, float zd)
    {
        int hash = seed;
        hash ^= X_PRIME * x;
        hash ^= Y_PRIME * y;
        hash ^= Z_PRIME * z;

        hash = hash * hash * hash * 60493;
        hash = (hash >> 13) ^ hash;

        float3 g = grad3D[hash & 15];

        return xd * g.x + yd * g.y + zd * g.z;
    }

    [MethodImpl(FN_INLINE)]
    float GradCoord4D(int seed, int x, int y, int z, int w, float xd, float yd, float zd, float wd)
    {
        int hash = seed;
        hash ^= X_PRIME * x;
        hash ^= Y_PRIME * y;
        hash ^= Z_PRIME * z;
        hash ^= W_PRIME * w;

        hash = hash * hash * hash * 60493;
        hash = (hash >> 13) ^ hash;

        hash &= 31;
        float a = yd, b = zd, c = wd;            // X,Y,Z
        switch (hash >> 3)
        {          // OR, DEPENDING ON HIGH ORDER 2 BITS:
            case 1: a = wd; b = xd; c = yd; break;     // W,X,Y
            case 2: a = zd; b = wd; c = xd; break;     // Z,W,X
            case 3: a = yd; b = zd; c = wd; break;     // Y,Z,W
        }
        return ((hash & 4) == 0 ? -a : a) + ((hash & 2) == 0 ? -b : b) + ((hash & 1) == 0 ? -c : c);
    }

    //	RETURN NOISE BETWEEN 0 & 1
    public float GetNoise01(float x, float y)
    {
        return To01(GetNoise(x, y));
    }
    float To01(float value)
    {
        return (value * 0.5f) + 0.5f;
    }

    public float GetNoise(float x, float y, float z)
    {
        x *= m_frequency;
        y *= m_frequency;
        z *= m_frequency;

        switch (m_noiseType)
        {
            case NoiseType.Value:
                return SingleValue(m_seed, x, y, z);
            case NoiseType.ValueFractal:
                switch (m_fractalType)
                {
                    case FractalType.FBM:
                        return SingleValueFractalFBM(x, y, z);
                    case FractalType.Billow:
                        return SingleValueFractalBillow(x, y, z);
                    case FractalType.RigidMulti:
                        return SingleValueFractalRigidMulti(x, y, z);
                    default:
                        return 0;
                }
            case NoiseType.Perlin:
                return SinglePerlin(m_seed, x, y, z);
            case NoiseType.PerlinFractal:
                switch (m_fractalType)
                {
                    case FractalType.FBM:
                        return SinglePerlinFractalFBM(x, y, z);
                    case FractalType.Billow:
                        return SinglePerlinFractalBillow(x, y, z);
                    case FractalType.RigidMulti:
                        return SinglePerlinFractalRigidMulti(x, y, z);
                    default:
                        return 0;
                }
            case NoiseType.Simplex:
                return SingleSimplex(m_seed, x, y, z);
            case NoiseType.SimplexFractal:
                switch (m_fractalType)
                {
                    case FractalType.FBM:
                        return SingleSimplexFractalFBM(x, y, z);
                    case FractalType.Billow:
                        return SingleSimplexFractalBillow(x, y, z);
                    case FractalType.RigidMulti:
                        return SingleSimplexFractalRigidMulti(x, y, z);
                    default:
                        return 0;
                }
            case NoiseType.WhiteNoise:
                return GetWhiteNoise(x, y, z);
            case NoiseType.Cubic:
                return SingleCubic(m_seed, x, y, z);
            case NoiseType.CubicFractal:
                switch (m_fractalType)
                {
                    case FractalType.FBM:
                        return SingleCubicFractalFBM(x, y, z);
                    case FractalType.Billow:
                        return SingleCubicFractalBillow(x, y, z);
                    case FractalType.RigidMulti:
                        return SingleCubicFractalRigidMulti(x, y, z);
                    default:
                        return 0;
                }
            default:
                return 0;
        }
    }

    public float GetNoise(float x, float y)
    {
        x *= m_frequency;
        y *= m_frequency;

        switch (m_noiseType)
        {
            case NoiseType.Value:
                return SingleValue(m_seed, x, y);
            case NoiseType.ValueFractal:
                switch (m_fractalType)
                {
                    case FractalType.FBM:
                        return SingleValueFractalFBM(x, y);
                    case FractalType.Billow:
                        return SingleValueFractalBillow(x, y);
                    case FractalType.RigidMulti:
                        return SingleValueFractalRigidMulti(x, y);
                    default:
                        return 0;
                }
            case NoiseType.Perlin:
                return SinglePerlin(m_seed, x, y);
            case NoiseType.PerlinFractal:
                switch (m_fractalType)
                {
                    case FractalType.FBM:
                        return SinglePerlinFractalFBM(x, y);
                    case FractalType.Billow:
                        return SinglePerlinFractalBillow(x, y);
                    case FractalType.RigidMulti:
                        return SinglePerlinFractalRigidMulti(x, y);
                    default:
                        return 0;
                }
            case NoiseType.Simplex:
                return SingleSimplex(m_seed, x, y);
            case NoiseType.SimplexFractal:
                switch (m_fractalType)
                {
                    case FractalType.FBM:
                        return SingleSimplexFractalFBM(x, y);
                    case FractalType.Billow:
                        return SingleSimplexFractalBillow(x, y);
                    case FractalType.RigidMulti:
                        return SingleSimplexFractalRigidMulti(x, y);
                    default:
                        return 0;
                }
            case NoiseType.WhiteNoise:
                return GetWhiteNoise(x, y);
            case NoiseType.Cubic:
                return SingleCubic(m_seed, x, y);
            case NoiseType.CubicFractal:
                switch (m_fractalType)
                {
                    case FractalType.FBM:
                        return SingleCubicFractalFBM(x, y);
                    case FractalType.Billow:
                        return SingleCubicFractalBillow(x, y);
                    case FractalType.RigidMulti:
                        return SingleCubicFractalRigidMulti(x, y);
                    default:
                        return 0;
                }
            default:
                return 0;
        }
    }

    // White Noise
    [MethodImpl(FN_INLINE)]
    int FloatCast2Int(float f)
    {
        var i = BitConverter.DoubleToInt64Bits(f);

        return (int)(i ^ (i >> 32));
    }

    public float GetWhiteNoise(float x, float y, float z, float w)
    {
        int xi = FloatCast2Int(x);
        int yi = FloatCast2Int(y);
        int zi = FloatCast2Int(z);
        int wi = FloatCast2Int(w);

        return ValCoord4D(m_seed, xi, yi, zi, wi);
    }

    public float GetWhiteNoise(float x, float y, float z)
    {
        int xi = FloatCast2Int(x);
        int yi = FloatCast2Int(y);
        int zi = FloatCast2Int(z);

        return ValCoord3D(m_seed, xi, yi, zi);
    }

    public float GetWhiteNoise(float x, float y)
    {
        int xi = FloatCast2Int(x);
        int yi = FloatCast2Int(y);

        return ValCoord2D(m_seed, xi, yi);
    }

    public float GetWhiteNoiseInt(int x, int y, int z, int w)
    {
        return ValCoord4D(m_seed, x, y, z, w);
    }

    public float GetWhiteNoiseInt(int x, int y, int z)
    {
        return ValCoord3D(m_seed, x, y, z);
    }

    public float GetWhiteNoiseInt(int x, int y)
    {
        return ValCoord2D(m_seed, x, y);
    }

    // Value Noise
    public float GetValueFractal(float x, float y, float z)
    {
        x *= m_frequency;
        y *= m_frequency;
        z *= m_frequency;

        switch (m_fractalType)
        {
            case FractalType.FBM:
                return SingleValueFractalFBM(x, y, z);
            case FractalType.Billow:
                return SingleValueFractalBillow(x, y, z);
            case FractalType.RigidMulti:
                return SingleValueFractalRigidMulti(x, y, z);
            default:
                return 0;
        }
    }

    float SingleValueFractalFBM(float x, float y, float z)
    {
        int seed = m_seed;
        float sum = SingleValue(seed, x, y, z);
        float amp = 1;

        for (int i = 1; i < m_octaves; i++)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;
            z *= m_lacunarity;

            amp *= m_gain;
            sum += SingleValue(++seed, x, y, z) * amp;
        }

        return sum * m_fractalBounding;
    }

    float SingleValueFractalBillow(float x, float y, float z)
    {
        int seed = m_seed;
        float sum = Math.Abs(SingleValue(seed, x, y, z)) * 2 - 1;
        float amp = 1;

        for (int i = 1; i < m_octaves; i++)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;
            z *= m_lacunarity;

            amp *= m_gain;
            sum += (Math.Abs(SingleValue(++seed, x, y, z)) * 2 - 1) * amp;
        }

        return sum * m_fractalBounding;
    }

    float SingleValueFractalRigidMulti(float x, float y, float z)
    {
        int seed = m_seed;
        float sum = 1 - Math.Abs(SingleValue(seed, x, y, z));
        float amp = 1;

        for (int i = 1; i < m_octaves; i++)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;
            z *= m_lacunarity;

            amp *= m_gain;
            sum -= (1 - Math.Abs(SingleValue(++seed, x, y, z))) * amp;
        }

        return sum;
    }

    public float GetValue(float x, float y, float z)
    {
        return SingleValue(m_seed, x * m_frequency, y * m_frequency, z * m_frequency);
    }

    float SingleValue(int seed, float x, float y, float z)
    {
        int x0 = FastFloor(x);
        int y0 = FastFloor(y);
        int z0 = FastFloor(z);
        int x1 = x0 + 1;
        int y1 = y0 + 1;
        int z1 = z0 + 1;

        float xs, ys, zs;
        switch (m_interp)
        {
            default:
            case Interp.Linear:
                xs = x - x0;
                ys = y - y0;
                zs = z - z0;
                break;
            case Interp.Hermite:
                xs = InterpHermiteFunc(x - x0);
                ys = InterpHermiteFunc(y - y0);
                zs = InterpHermiteFunc(z - z0);
                break;
            case Interp.Quintic:
                xs = InterpQuinticFunc(x - x0);
                ys = InterpQuinticFunc(y - y0);
                zs = InterpQuinticFunc(z - z0);
                break;
        }

        float xf00 = Lerp(ValCoord3D(seed, x0, y0, z0), ValCoord3D(seed, x1, y0, z0), xs);
        float xf10 = Lerp(ValCoord3D(seed, x0, y1, z0), ValCoord3D(seed, x1, y1, z0), xs);
        float xf01 = Lerp(ValCoord3D(seed, x0, y0, z1), ValCoord3D(seed, x1, y0, z1), xs);
        float xf11 = Lerp(ValCoord3D(seed, x0, y1, z1), ValCoord3D(seed, x1, y1, z1), xs);

        float yf0 = Lerp(xf00, xf10, ys);
        float yf1 = Lerp(xf01, xf11, ys);

        return Lerp(yf0, yf1, zs);
    }

    public float GetValueFractal(float x, float y)
    {
        x *= m_frequency;
        y *= m_frequency;

        switch (m_fractalType)
        {
            case FractalType.FBM:
                return SingleValueFractalFBM(x, y);
            case FractalType.Billow:
                return SingleValueFractalBillow(x, y);
            case FractalType.RigidMulti:
                return SingleValueFractalRigidMulti(x, y);
            default:
                return 0;
        }
    }

    float SingleValueFractalFBM(float x, float y)
    {
        int seed = m_seed;
        float sum = SingleValue(seed, x, y);
        float amp = 1;

        for (int i = 1; i < m_octaves; i++)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;

            amp *= m_gain;
            sum += SingleValue(++seed, x, y) * amp;
        }

        return sum * m_fractalBounding;
    }

    float SingleValueFractalBillow(float x, float y)
    {
        int seed = m_seed;
        float sum = Math.Abs(SingleValue(seed, x, y)) * 2 - 1;
        float amp = 1;

        for (int i = 1; i < m_octaves; i++)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;
            amp *= m_gain;
            sum += (Math.Abs(SingleValue(++seed, x, y)) * 2 - 1) * amp;
        }

        return sum * m_fractalBounding;
    }

    float SingleValueFractalRigidMulti(float x, float y)
    {
        int seed = m_seed;
        float sum = 1 - Math.Abs(SingleValue(seed, x, y));
        float amp = 1;

        for (int i = 1; i < m_octaves; i++)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;

            amp *= m_gain;
            sum -= (1 - Math.Abs(SingleValue(++seed, x, y))) * amp;
        }

        return sum;
    }

    public float GetValue(float x, float y)
    {
        return SingleValue(m_seed, x * m_frequency, y * m_frequency);
    }

    float SingleValue(int seed, float x, float y)
    {
        int x0 = FastFloor(x);
        int y0 = FastFloor(y);
        int x1 = x0 + 1;
        int y1 = y0 + 1;

        float xs, ys;
        switch (m_interp)
        {
            default:
            case Interp.Linear:
                xs = x - x0;
                ys = y - y0;
                break;
            case Interp.Hermite:
                xs = InterpHermiteFunc(x - x0);
                ys = InterpHermiteFunc(y - y0);
                break;
            case Interp.Quintic:
                xs = InterpQuinticFunc(x - x0);
                ys = InterpQuinticFunc(y - y0);
                break;
        }

        float xf0 = Lerp(ValCoord2D(seed, x0, y0), ValCoord2D(seed, x1, y0), xs);
        float xf1 = Lerp(ValCoord2D(seed, x0, y1), ValCoord2D(seed, x1, y1), xs);

        return Lerp(xf0, xf1, ys);
    }

    // Gradient Noise
    public float GetPerlinFractal(float x, float y, float z)
    {
        x *= m_frequency;
        y *= m_frequency;
        z *= m_frequency;

        switch (m_fractalType)
        {
            case FractalType.FBM:
                return SinglePerlinFractalFBM(x, y, z);
            case FractalType.Billow:
                return SinglePerlinFractalBillow(x, y, z);
            case FractalType.RigidMulti:
                return SinglePerlinFractalRigidMulti(x, y, z);
            default:
                return 0;
        }
    }

    float SinglePerlinFractalFBM(float x, float y, float z)
    {
        int seed = m_seed;
        float sum = SinglePerlin(seed, x, y, z);
        float amp = 1;

        for (int i = 1; i < m_octaves; i++)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;
            z *= m_lacunarity;

            amp *= m_gain;
            sum += SinglePerlin(++seed, x, y, z) * amp;
        }

        return sum * m_fractalBounding;
    }

    float SinglePerlinFractalBillow(float x, float y, float z)
    {
        int seed = m_seed;
        float sum = Math.Abs(SinglePerlin(seed, x, y, z)) * 2 - 1;
        float amp = 1;

        for (int i = 1; i < m_octaves; i++)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;
            z *= m_lacunarity;

            amp *= m_gain;
            sum += (Math.Abs(SinglePerlin(++seed, x, y, z)) * 2 - 1) * amp;
        }

        return sum * m_fractalBounding;
    }

    float SinglePerlinFractalRigidMulti(float x, float y, float z)
    {
        int seed = m_seed;
        float sum = 1 - Math.Abs(SinglePerlin(seed, x, y, z));
        float amp = 1;

        for (int i = 1; i < m_octaves; i++)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;
            z *= m_lacunarity;

            amp *= m_gain;
            sum -= (1 - Math.Abs(SinglePerlin(++seed, x, y, z))) * amp;
        }

        return sum;
    }

    public float GetPerlin(float x, float y, float z)
    {
        return SinglePerlin(m_seed, x * m_frequency, y * m_frequency, z * m_frequency);
    }

    float SinglePerlin(int seed, float x, float y, float z)
    {
        int x0 = FastFloor(x);
        int y0 = FastFloor(y);
        int z0 = FastFloor(z);
        int x1 = x0 + 1;
        int y1 = y0 + 1;
        int z1 = z0 + 1;

        float xs, ys, zs;
        switch (m_interp)
        {
            default:
            case Interp.Linear:
                xs = x - x0;
                ys = y - y0;
                zs = z - z0;
                break;
            case Interp.Hermite:
                xs = InterpHermiteFunc(x - x0);
                ys = InterpHermiteFunc(y - y0);
                zs = InterpHermiteFunc(z - z0);
                break;
            case Interp.Quintic:
                xs = InterpQuinticFunc(x - x0);
                ys = InterpQuinticFunc(y - y0);
                zs = InterpQuinticFunc(z - z0);
                break;
        }

        float xd0 = x - x0;
        float yd0 = y - y0;
        float zd0 = z - z0;
        float xd1 = xd0 - 1;
        float yd1 = yd0 - 1;
        float zd1 = zd0 - 1;

        float xf00 = Lerp(GradCoord3D(seed, x0, y0, z0, xd0, yd0, zd0), GradCoord3D(seed, x1, y0, z0, xd1, yd0, zd0), xs);
        float xf10 = Lerp(GradCoord3D(seed, x0, y1, z0, xd0, yd1, zd0), GradCoord3D(seed, x1, y1, z0, xd1, yd1, zd0), xs);
        float xf01 = Lerp(GradCoord3D(seed, x0, y0, z1, xd0, yd0, zd1), GradCoord3D(seed, x1, y0, z1, xd1, yd0, zd1), xs);
        float xf11 = Lerp(GradCoord3D(seed, x0, y1, z1, xd0, yd1, zd1), GradCoord3D(seed, x1, y1, z1, xd1, yd1, zd1), xs);

        float yf0 = Lerp(xf00, xf10, ys);
        float yf1 = Lerp(xf01, xf11, ys);

        return Lerp(yf0, yf1, zs);
    }

    public float GetPerlinFractal(float x, float y)
    {
        x *= m_frequency;
        y *= m_frequency;

        switch (m_fractalType)
        {
            case FractalType.FBM:
                return SinglePerlinFractalFBM(x, y);
            case FractalType.Billow:
                return SinglePerlinFractalBillow(x, y);
            case FractalType.RigidMulti:
                return SinglePerlinFractalRigidMulti(x, y);
            default:
                return 0;
        }
    }

    float SinglePerlinFractalFBM(float x, float y)
    {
        int seed = m_seed;
        float sum = SinglePerlin(seed, x, y);
        float amp = 1;

        for (int i = 1; i < m_octaves; i++)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;

            amp *= m_gain;
            sum += SinglePerlin(++seed, x, y) * amp;
        }

        return sum * m_fractalBounding;
    }

    float SinglePerlinFractalBillow(float x, float y)
    {
        int seed = m_seed;
        float sum = Math.Abs(SinglePerlin(seed, x, y)) * 2 - 1;
        float amp = 1;

        for (int i = 1; i < m_octaves; i++)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;

            amp *= m_gain;
            sum += (Math.Abs(SinglePerlin(++seed, x, y)) * 2 - 1) * amp;
        }

        return sum * m_fractalBounding;
    }

    float SinglePerlinFractalRigidMulti(float x, float y)
    {
        int seed = m_seed;
        float sum = 1 - Math.Abs(SinglePerlin(seed, x, y));
        float amp = 1;

        for (int i = 1; i < m_octaves; i++)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;

            amp *= m_gain;
            sum -= (1 - Math.Abs(SinglePerlin(++seed, x, y))) * amp;
        }

        return sum;
    }

    public float GetPerlin(float x, float y)
    {
        return SinglePerlin(m_seed, x * m_frequency, y * m_frequency);
    }

    float SinglePerlin(int seed, float x, float y)
    {
        int x0 = FastFloor(x);
        int y0 = FastFloor(y);
        int x1 = x0 + 1;
        int y1 = y0 + 1;

        float xs, ys;
        switch (m_interp)
        {
            default:
            case Interp.Linear:
                xs = x - x0;
                ys = y - y0;
                break;
            case Interp.Hermite:
                xs = InterpHermiteFunc(x - x0);
                ys = InterpHermiteFunc(y - y0);
                break;
            case Interp.Quintic:
                xs = InterpQuinticFunc(x - x0);
                ys = InterpQuinticFunc(y - y0);
                break;
        }

        float xd0 = x - x0;
        float yd0 = y - y0;
        float xd1 = xd0 - 1;
        float yd1 = yd0 - 1;

        float xf0 = Lerp(GradCoord2D(seed, x0, y0, xd0, yd0), GradCoord2D(seed, x1, y0, xd1, yd0), xs);
        float xf1 = Lerp(GradCoord2D(seed, x0, y1, xd0, yd1), GradCoord2D(seed, x1, y1, xd1, yd1), xs);

        return Lerp(xf0, xf1, ys);
    }

    // Simplex Noise
    public float GetSimplexFractal(float x, float y, float z)
    {
        x *= m_frequency;
        y *= m_frequency;
        z *= m_frequency;

        switch (m_fractalType)
        {
            case FractalType.FBM:
                return SingleSimplexFractalFBM(x, y, z);
            case FractalType.Billow:
                return SingleSimplexFractalBillow(x, y, z);
            case FractalType.RigidMulti:
                return SingleSimplexFractalRigidMulti(x, y, z);
            default:
                return 0;
        }
    }

    float SingleSimplexFractalFBM(float x, float y, float z)
    {
        int seed = m_seed;
        float sum = SingleSimplex(seed, x, y, z);
        float amp = 1;

        for (int i = 1; i < m_octaves; i++)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;
            z *= m_lacunarity;

            amp *= m_gain;
            sum += SingleSimplex(++seed, x, y, z) * amp;
        }

        return sum * m_fractalBounding;
    }

    float SingleSimplexFractalBillow(float x, float y, float z)
    {
        int seed = m_seed;
        float sum = Math.Abs(SingleSimplex(seed, x, y, z)) * 2 - 1;
        float amp = 1;

        for (int i = 1; i < m_octaves; i++)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;
            z *= m_lacunarity;

            amp *= m_gain;
            sum += (Math.Abs(SingleSimplex(++seed, x, y, z)) * 2 - 1) * amp;
        }

        return sum * m_fractalBounding;
    }

    float SingleSimplexFractalRigidMulti(float x, float y, float z)
    {
        int seed = m_seed;
        float sum = 1 - Math.Abs(SingleSimplex(seed, x, y, z));
        float amp = 1;

        for (int i = 1; i < m_octaves; i++)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;
            z *= m_lacunarity;

            amp *= m_gain;
            sum -= (1 - Math.Abs(SingleSimplex(++seed, x, y, z))) * amp;
        }

        return sum;
    }

    public float GetSimplex(float x, float y, float z)
    {
        return SingleSimplex(m_seed, x * m_frequency, y * m_frequency, z * m_frequency);
    }

    const float F3 = (float)(1.0 / 3.0);
    const float G3 = (float)(1.0 / 6.0);
    const float G33 = G3 * 3 - 1;

    float SingleSimplex(int seed, float x, float y, float z)
    {
        float t = (x + y + z) * F3;
        int i = FastFloor(x + t);
        int j = FastFloor(y + t);
        int k = FastFloor(z + t);

        t = (i + j + k) * G3;
        float x0 = x - (i - t);
        float y0 = y - (j - t);
        float z0 = z - (k - t);

        int i1, j1, k1;
        int i2, j2, k2;

        if (x0 >= y0)
        {
            if (y0 >= z0)
            {
                i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0;
            }
            else if (x0 >= z0)
            {
                i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1;
            }
            else // x0 < z0
            {
                i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1;
            }
        }
        else // x0 < y0
        {
            if (y0 < z0)
            {
                i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1;
            }
            else if (x0 < z0)
            {
                i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1;
            }
            else // x0 >= z0
            {
                i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0;
            }
        }

        float x1 = x0 - i1 + G3;
        float y1 = y0 - j1 + G3;
        float z1 = z0 - k1 + G3;
        float x2 = x0 - i2 + F3;
        float y2 = y0 - j2 + F3;
        float z2 = z0 - k2 + F3;
        float x3 = x0 + G33;
        float y3 = y0 + G33;
        float z3 = z0 + G33;

        float n0, n1, n2, n3;

        t = (float)0.6 - x0 * x0 - y0 * y0 - z0 * z0;
        if (t < 0) n0 = 0;
        else
        {
            t *= t;
            n0 = t * t * GradCoord3D(seed, i, j, k, x0, y0, z0);
        }

        t = (float)0.6 - x1 * x1 - y1 * y1 - z1 * z1;
        if (t < 0) n1 = 0;
        else
        {
            t *= t;
            n1 = t * t * GradCoord3D(seed, i + i1, j + j1, k + k1, x1, y1, z1);
        }

        t = (float)0.6 - x2 * x2 - y2 * y2 - z2 * z2;
        if (t < 0) n2 = 0;
        else
        {
            t *= t;
            n2 = t * t * GradCoord3D(seed, i + i2, j + j2, k + k2, x2, y2, z2);
        }

        t = (float)0.6 - x3 * x3 - y3 * y3 - z3 * z3;
        if (t < 0) n3 = 0;
        else
        {
            t *= t;
            n3 = t * t * GradCoord3D(seed, i + 1, j + 1, k + 1, x3, y3, z3);
        }

        return 32 * (n0 + n1 + n2 + n3);
    }

    public float GetSimplexFractal(float x, float y)
    {
        x *= m_frequency;
        y *= m_frequency;

        switch (m_fractalType)
        {
            case FractalType.FBM:
                return SingleSimplexFractalFBM(x, y);
            case FractalType.Billow:
                return SingleSimplexFractalBillow(x, y);
            case FractalType.RigidMulti:
                return SingleSimplexFractalRigidMulti(x, y);
            default:
                return 0;
        }
    }

    float SingleSimplexFractalFBM(float x, float y)
    {
        int seed = m_seed;
        float sum = SingleSimplex(seed, x, y);
        float amp = 1;

        for (int i = 1; i < m_octaves; i++)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;

            amp *= m_gain;
            sum += SingleSimplex(++seed, x, y) * amp;
        }

        return sum * m_fractalBounding;
    }

    float SingleSimplexFractalBillow(float x, float y)
    {
        int seed = m_seed;
        float sum = Math.Abs(SingleSimplex(seed, x, y)) * 2 - 1;
        float amp = 1;

        for (int i = 1; i < m_octaves; i++)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;

            amp *= m_gain;
            sum += (Math.Abs(SingleSimplex(++seed, x, y)) * 2 - 1) * amp;
        }

        return sum * m_fractalBounding;
    }

    float SingleSimplexFractalRigidMulti(float x, float y)
    {
        int seed = m_seed;
        float sum = 1 - Math.Abs(SingleSimplex(seed, x, y));
        float amp = 1;

        for (int i = 1; i < m_octaves; i++)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;

            amp *= m_gain;
            sum -= (1 - Math.Abs(SingleSimplex(++seed, x, y))) * amp;
        }

        return sum;
    }

    public float GetSimplex(float x, float y)
    {
        return SingleSimplex(m_seed, x * m_frequency, y * m_frequency);
    }

    const float F2 = (float)(1.0 / 2.0);
    const float G2 = (float)(1.0 / 4.0);

    float SingleSimplex(int seed, float x, float y)
    {
        float t = (x + y) * F2;
        int i = FastFloor(x + t);
        int j = FastFloor(y + t);

        t = (i + j) * G2;
        float X0 = i - t;
        float Y0 = j - t;

        float x0 = x - X0;
        float y0 = y - Y0;

        int i1, j1;
        if (x0 > y0)
        {
            i1 = 1; j1 = 0;
        }
        else
        {
            i1 = 0; j1 = 1;
        }

        float x1 = x0 - i1 + G2;
        float y1 = y0 - j1 + G2;
        float x2 = x0 - 1 + F2;
        float y2 = y0 - 1 + F2;

        float n0, n1, n2;

        t = (float)0.5 - x0 * x0 - y0 * y0;
        if (t < 0) n0 = 0;
        else
        {
            t *= t;
            n0 = t * t * GradCoord2D(seed, i, j, x0, y0);
        }

        t = (float)0.5 - x1 * x1 - y1 * y1;
        if (t < 0) n1 = 0;
        else
        {
            t *= t;
            n1 = t * t * GradCoord2D(seed, i + i1, j + j1, x1, y1);
        }

        t = (float)0.5 - x2 * x2 - y2 * y2;
        if (t < 0) n2 = 0;
        else
        {
            t *= t;
            n2 = t * t * GradCoord2D(seed, i + 1, j + 1, x2, y2);
        }

        return 50 * (n0 + n1 + n2);
    }

    public float GetSimplex(float x, float y, float z, float w)
    {
        return SingleSimplex(m_seed, x * m_frequency, y * m_frequency, z * m_frequency, w * m_frequency);
    }

    const float F4 = (float)((2.23606797 - 1.0) / 4.0);
    const float G4 = (float)((5.0 - 2.23606797) / 20.0);

    float SingleSimplex(int seed, float x, float y, float z, float w)
    {
        float n0, n1, n2, n3, n4;
        float t = (x + y + z + w) * F4;
        int i = FastFloor(x + t);
        int j = FastFloor(y + t);
        int k = FastFloor(z + t);
        int l = FastFloor(w + t);
        t = (i + j + k + l) * G4;
        float X0 = i - t;
        float Y0 = j - t;
        float Z0 = k - t;
        float W0 = l - t;
        float x0 = x - X0;
        float y0 = y - Y0;
        float z0 = z - Z0;
        float w0 = w - W0;

        int c = (x0 > y0) ? 32 : 0;
        c += (x0 > z0) ? 16 : 0;
        c += (y0 > z0) ? 8 : 0;
        c += (x0 > w0) ? 4 : 0;
        c += (y0 > w0) ? 2 : 0;
        c += (z0 > w0) ? 1 : 0;
        c <<= 2;

        int i1 = simplex4D[c] >= 3 ? 1 : 0;
        int i2 = simplex4D[c] >= 2 ? 1 : 0;
        int i3 = simplex4D[c++] >= 1 ? 1 : 0;
        int j1 = simplex4D[c] >= 3 ? 1 : 0;
        int j2 = simplex4D[c] >= 2 ? 1 : 0;
        int j3 = simplex4D[c++] >= 1 ? 1 : 0;
        int k1 = simplex4D[c] >= 3 ? 1 : 0;
        int k2 = simplex4D[c] >= 2 ? 1 : 0;
        int k3 = simplex4D[c++] >= 1 ? 1 : 0;
        int l1 = simplex4D[c] >= 3 ? 1 : 0;
        int l2 = simplex4D[c] >= 2 ? 1 : 0;
        int l3 = simplex4D[c] >= 1 ? 1 : 0;

        float x1 = x0 - i1 + G4;
        float y1 = y0 - j1 + G4;
        float z1 = z0 - k1 + G4;
        float w1 = w0 - l1 + G4;
        float x2 = x0 - i2 + 2 * G4;
        float y2 = y0 - j2 + 2 * G4;
        float z2 = z0 - k2 + 2 * G4;
        float w2 = w0 - l2 + 2 * G4;
        float x3 = x0 - i3 + 3 * G4;
        float y3 = y0 - j3 + 3 * G4;
        float z3 = z0 - k3 + 3 * G4;
        float w3 = w0 - l3 + 3 * G4;
        float x4 = x0 - 1 + 4 * G4;
        float y4 = y0 - 1 + 4 * G4;
        float z4 = z0 - 1 + 4 * G4;
        float w4 = w0 - 1 + 4 * G4;

        t = (float)0.6 - x0 * x0 - y0 * y0 - z0 * z0 - w0 * w0;
        if (t < 0) n0 = 0;
        else
        {
            t *= t;
            n0 = t * t * GradCoord4D(seed, i, j, k, l, x0, y0, z0, w0);
        }
        t = (float)0.6 - x1 * x1 - y1 * y1 - z1 * z1 - w1 * w1;
        if (t < 0) n1 = 0;
        else
        {
            t *= t;
            n1 = t * t * GradCoord4D(seed, i + i1, j + j1, k + k1, l + l1, x1, y1, z1, w1);
        }
        t = (float)0.6 - x2 * x2 - y2 * y2 - z2 * z2 - w2 * w2;
        if (t < 0) n2 = 0;
        else
        {
            t *= t;
            n2 = t * t * GradCoord4D(seed, i + i2, j + j2, k + k2, l + l2, x2, y2, z2, w2);
        }
        t = (float)0.6 - x3 * x3 - y3 * y3 - z3 * z3 - w3 * w3;
        if (t < 0) n3 = 0;
        else
        {
            t *= t;
            n3 = t * t * GradCoord4D(seed, i + i3, j + j3, k + k3, l + l3, x3, y3, z3, w3);
        }
        t = (float)0.6 - x4 * x4 - y4 * y4 - z4 * z4 - w4 * w4;
        if (t < 0) n4 = 0;
        else
        {
            t *= t;
            n4 = t * t * GradCoord4D(seed, i + 1, j + 1, k + 1, l + 1, x4, y4, z4, w4);
        }

        return 27 * (n0 + n1 + n2 + n3 + n4);
    }

    // Cubic Noise
    public float GetCubicFractal(float x, float y, float z)
    {
        x *= m_frequency;
        y *= m_frequency;
        z *= m_frequency;

        switch (m_fractalType)
        {
            case FractalType.FBM:
                return SingleCubicFractalFBM(x, y, z);
            case FractalType.Billow:
                return SingleCubicFractalBillow(x, y, z);
            case FractalType.RigidMulti:
                return SingleCubicFractalRigidMulti(x, y, z);
            default:
                return 0;
        }
    }

    float SingleCubicFractalFBM(float x, float y, float z)
    {
        int seed = m_seed;
        float sum = SingleCubic(seed, x, y, z);
        float amp = 1;
        int i = 0;

        while (++i < m_octaves)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;
            z *= m_lacunarity;

            amp *= m_gain;
            sum += SingleCubic(++seed, x, y, z) * amp;
        }

        return sum * m_fractalBounding;
    }

    float SingleCubicFractalBillow(float x, float y, float z)
    {
        int seed = m_seed;
        float sum = Math.Abs(SingleCubic(seed, x, y, z)) * 2 - 1;
        float amp = 1;
        int i = 0;

        while (++i < m_octaves)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;
            z *= m_lacunarity;

            amp *= m_gain;
            sum += (Math.Abs(SingleCubic(++seed, x, y, z)) * 2 - 1) * amp;
        }

        return sum * m_fractalBounding;
    }

    float SingleCubicFractalRigidMulti(float x, float y, float z)
    {
        int seed = m_seed;
        float sum = 1 - Math.Abs(SingleCubic(seed, x, y, z));
        float amp = 1;
        int i = 0;

        while (++i < m_octaves)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;
            z *= m_lacunarity;

            amp *= m_gain;
            sum -= (1 - Math.Abs(SingleCubic(++seed, x, y, z))) * amp;
        }

        return sum;
    }

    public float GetCubic(float x, float y, float z)
    {
        return SingleCubic(m_seed, x * m_frequency, y * m_frequency, z * m_frequency);
    }

    const float CUBIC_3D_BOUNDING = 1 / (float)(1.5 * 1.5 * 1.5);

    float SingleCubic(int seed, float x, float y, float z)
    {
        int x1 = FastFloor(x);
        int y1 = FastFloor(y);
        int z1 = FastFloor(z);

        int x0 = x1 - 1;
        int y0 = y1 - 1;
        int z0 = z1 - 1;
        int x2 = x1 + 1;
        int y2 = y1 + 1;
        int z2 = z1 + 1;
        int x3 = x1 + 2;
        int y3 = y1 + 2;
        int z3 = z1 + 2;

        float xs = x - (float)x1;
        float ys = y - (float)y1;
        float zs = z - (float)z1;

        return CubicLerp(
            CubicLerp(
            CubicLerp(ValCoord3D(seed, x0, y0, z0), ValCoord3D(seed, x1, y0, z0), ValCoord3D(seed, x2, y0, z0), ValCoord3D(seed, x3, y0, z0), xs),
            CubicLerp(ValCoord3D(seed, x0, y1, z0), ValCoord3D(seed, x1, y1, z0), ValCoord3D(seed, x2, y1, z0), ValCoord3D(seed, x3, y1, z0), xs),
            CubicLerp(ValCoord3D(seed, x0, y2, z0), ValCoord3D(seed, x1, y2, z0), ValCoord3D(seed, x2, y2, z0), ValCoord3D(seed, x3, y2, z0), xs),
            CubicLerp(ValCoord3D(seed, x0, y3, z0), ValCoord3D(seed, x1, y3, z0), ValCoord3D(seed, x2, y3, z0), ValCoord3D(seed, x3, y3, z0), xs),
            ys),
            CubicLerp(
            CubicLerp(ValCoord3D(seed, x0, y0, z1), ValCoord3D(seed, x1, y0, z1), ValCoord3D(seed, x2, y0, z1), ValCoord3D(seed, x3, y0, z1), xs),
            CubicLerp(ValCoord3D(seed, x0, y1, z1), ValCoord3D(seed, x1, y1, z1), ValCoord3D(seed, x2, y1, z1), ValCoord3D(seed, x3, y1, z1), xs),
            CubicLerp(ValCoord3D(seed, x0, y2, z1), ValCoord3D(seed, x1, y2, z1), ValCoord3D(seed, x2, y2, z1), ValCoord3D(seed, x3, y2, z1), xs),
            CubicLerp(ValCoord3D(seed, x0, y3, z1), ValCoord3D(seed, x1, y3, z1), ValCoord3D(seed, x2, y3, z1), ValCoord3D(seed, x3, y3, z1), xs),
            ys),
            CubicLerp(
            CubicLerp(ValCoord3D(seed, x0, y0, z2), ValCoord3D(seed, x1, y0, z2), ValCoord3D(seed, x2, y0, z2), ValCoord3D(seed, x3, y0, z2), xs),
            CubicLerp(ValCoord3D(seed, x0, y1, z2), ValCoord3D(seed, x1, y1, z2), ValCoord3D(seed, x2, y1, z2), ValCoord3D(seed, x3, y1, z2), xs),
            CubicLerp(ValCoord3D(seed, x0, y2, z2), ValCoord3D(seed, x1, y2, z2), ValCoord3D(seed, x2, y2, z2), ValCoord3D(seed, x3, y2, z2), xs),
            CubicLerp(ValCoord3D(seed, x0, y3, z2), ValCoord3D(seed, x1, y3, z2), ValCoord3D(seed, x2, y3, z2), ValCoord3D(seed, x3, y3, z2), xs),
            ys),
            CubicLerp(
            CubicLerp(ValCoord3D(seed, x0, y0, z3), ValCoord3D(seed, x1, y0, z3), ValCoord3D(seed, x2, y0, z3), ValCoord3D(seed, x3, y0, z3), xs),
            CubicLerp(ValCoord3D(seed, x0, y1, z3), ValCoord3D(seed, x1, y1, z3), ValCoord3D(seed, x2, y1, z3), ValCoord3D(seed, x3, y1, z3), xs),
            CubicLerp(ValCoord3D(seed, x0, y2, z3), ValCoord3D(seed, x1, y2, z3), ValCoord3D(seed, x2, y2, z3), ValCoord3D(seed, x3, y2, z3), xs),
            CubicLerp(ValCoord3D(seed, x0, y3, z3), ValCoord3D(seed, x1, y3, z3), ValCoord3D(seed, x2, y3, z3), ValCoord3D(seed, x3, y3, z3), xs),
            ys),
            zs) * CUBIC_3D_BOUNDING;
    }


    public float GetCubicFractal(float x, float y)
    {
        x *= m_frequency;
        y *= m_frequency;

        switch (m_fractalType)
        {
            case FractalType.FBM:
                return SingleCubicFractalFBM(x, y);
            case FractalType.Billow:
                return SingleCubicFractalBillow(x, y);
            case FractalType.RigidMulti:
                return SingleCubicFractalRigidMulti(x, y);
            default:
                return 0;
        }
    }

    float SingleCubicFractalFBM(float x, float y)
    {
        int seed = m_seed;
        float sum = SingleCubic(seed, x, y);
        float amp = 1;
        int i = 0;

        while (++i < m_octaves)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;

            amp *= m_gain;
            sum += SingleCubic(++seed, x, y) * amp;
        }

        return sum * m_fractalBounding;
    }

    float SingleCubicFractalBillow(float x, float y)
    {
        int seed = m_seed;
        float sum = Math.Abs(SingleCubic(seed, x, y)) * 2 - 1;
        float amp = 1;
        int i = 0;

        while (++i < m_octaves)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;

            amp *= m_gain;
            sum += (Math.Abs(SingleCubic(++seed, x, y)) * 2 - 1) * amp;
        }

        return sum * m_fractalBounding;
    }

    float SingleCubicFractalRigidMulti(float x, float y)
    {
        int seed = m_seed;
        float sum = 1 - Math.Abs(SingleCubic(seed, x, y));
        float amp = 1;
        int i = 0;

        while (++i < m_octaves)
        {
            x *= m_lacunarity;
            y *= m_lacunarity;

            amp *= m_gain;
            sum -= (1 - Math.Abs(SingleCubic(++seed, x, y))) * amp;
        }

        return sum;
    }

    public float GetCubic(float x, float y)
    {
        x *= m_frequency;
        y *= m_frequency;

        return SingleCubic(0, x, y);
    }

    const float CUBIC_2D_BOUNDING = 1 / (float)(1.5 * 1.5);

    float SingleCubic(int seed, float x, float y)
    {
        int x1 = FastFloor(x);
        int y1 = FastFloor(y);

        int x0 = x1 - 1;
        int y0 = y1 - 1;
        int x2 = x1 + 1;
        int y2 = y1 + 1;
        int x3 = x1 + 2;
        int y3 = y1 + 2;

        float xs = x - (float)x1;
        float ys = y - (float)y1;

        return CubicLerp(
                   CubicLerp(ValCoord2D(seed, x0, y0), ValCoord2D(seed, x1, y0), ValCoord2D(seed, x2, y0), ValCoord2D(seed, x3, y0),
                       xs),
                   CubicLerp(ValCoord2D(seed, x0, y1), ValCoord2D(seed, x1, y1), ValCoord2D(seed, x2, y1), ValCoord2D(seed, x3, y1),
                       xs),
                   CubicLerp(ValCoord2D(seed, x0, y2), ValCoord2D(seed, x1, y2), ValCoord2D(seed, x2, y2), ValCoord2D(seed, x3, y2),
                       xs),
                   CubicLerp(ValCoord2D(seed, x0, y3), ValCoord2D(seed, x1, y3), ValCoord2D(seed, x2, y3), ValCoord2D(seed, x3, y3),
                       xs),
                   ys) * CUBIC_2D_BOUNDING;
    }

    // Cellular Noise
    public float GetCellular(float x, float y, float z)
    {
        x *= m_frequency;
        y *= m_frequency;
        z *= m_frequency;

        switch (m_cellularReturnType)
        {
            case CellularReturnType.CellValue:
            case CellularReturnType.Distance:
                return SingleCellular(x, y, z);
            default:
                return SingleCellular2Edge(x, y, z);
        }
    }

    private float SingleCellular(float x, float y, float z)
    {
        int xr = FastRound(x);
        int yr = FastRound(y);
        int zr = FastRound(z);

        float distance = 999999;
        int xc = 0, yc = 0, zc = 0;

        switch (m_cellularDistanceFunction)
        {
            case CellularDistanceFunction.Euclidean:
                for (int xi = xr - 1; xi <= xr + 1; xi++)
                {
                    for (int yi = yr - 1; yi <= yr + 1; yi++)
                    {
                        for (int zi = zr - 1; zi <= zr + 1; zi++)
                        {
                            float3 vec = cell3D[Hash3D(m_seed, xi, yi, zi) & 255];

                            float vecX = xi - x + vec.x * m_cellularJitter;
                            float vecY = yi - y + vec.y * m_cellularJitter;
                            float vecZ = zi - z + vec.z * m_cellularJitter;

                            float newDistance = vecX * vecX + vecY * vecY + vecZ * vecZ;

                            if (newDistance < distance)
                            {
                                distance = newDistance;
                                xc = xi;
                                yc = yi;
                                zc = zi;
                            }
                        }
                    }
                }
                break;
            case CellularDistanceFunction.Manhattan:
                for (int xi = xr - 1; xi <= xr + 1; xi++)
                {
                    for (int yi = yr - 1; yi <= yr + 1; yi++)
                    {
                        for (int zi = zr - 1; zi <= zr + 1; zi++)
                        {
                            float3 vec = cell3D[Hash3D(m_seed, xi, yi, zi) & 255];

                            float vecX = xi - x + vec.x * m_cellularJitter;
                            float vecY = yi - y + vec.y * m_cellularJitter;
                            float vecZ = zi - z + vec.z * m_cellularJitter;

                            float newDistance = Math.Abs(vecX) + Math.Abs(vecY) + Math.Abs(vecZ);

                            if (newDistance < distance)
                            {
                                distance = newDistance;
                                xc = xi;
                                yc = yi;
                                zc = zi;
                            }
                        }
                    }
                }
                break;
            case CellularDistanceFunction.Natural:
                for (int xi = xr - 1; xi <= xr + 1; xi++)
                {
                    for (int yi = yr - 1; yi <= yr + 1; yi++)
                    {
                        for (int zi = zr - 1; zi <= zr + 1; zi++)
                        {
                            float3 vec = cell3D[Hash3D(m_seed, xi, yi, zi) & 255];

                            float vecX = xi - x + vec.x * m_cellularJitter;
                            float vecY = yi - y + vec.y * m_cellularJitter;
                            float vecZ = zi - z + vec.z * m_cellularJitter;

                            float newDistance = (Math.Abs(vecX) + Math.Abs(vecY) + Math.Abs(vecZ)) + (vecX * vecX + vecY * vecY + vecZ * vecZ);

                            if (newDistance < distance)
                            {
                                distance = newDistance;
                                xc = xi;
                                yc = yi;
                                zc = zi;
                            }
                        }
                    }
                }
                break;
        }

        switch (m_cellularReturnType)
        {
            case CellularReturnType.CellValue:
                return ValCoord3D(m_seed, xc, yc, zc);

            case CellularReturnType.Distance:
                return distance;
            default:
                return 0;
        }
    }

    private float SingleCellular2Edge(float x, float y, float z)
    {
        int xr = FastRound(x);
        int yr = FastRound(y);
        int zr = FastRound(z);

        float[] distance = { 999999, 999999, 999999, 999999 };

        switch (m_cellularDistanceFunction)
        {
            case CellularDistanceFunction.Euclidean:
                for (int xi = xr - 1; xi <= xr + 1; xi++)
                {
                    for (int yi = yr - 1; yi <= yr + 1; yi++)
                    {
                        for (int zi = zr - 1; zi <= zr + 1; zi++)
                        {
                            float3 vec = cell3D[Hash3D(m_seed, xi, yi, zi) & 255];

                            float vecX = xi - x + vec.x * m_cellularJitter;
                            float vecY = yi - y + vec.y * m_cellularJitter;
                            float vecZ = zi - z + vec.z * m_cellularJitter;

                            float newDistance = vecX * vecX + vecY * vecY + vecZ * vecZ;

                            for (int i = m_cellularDistanceIndex1; i > 0; i--)
                                distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
                            distance[0] = Math.Min(distance[0], newDistance);
                        }
                    }
                }
                break;
            case CellularDistanceFunction.Manhattan:
                for (int xi = xr - 1; xi <= xr + 1; xi++)
                {
                    for (int yi = yr - 1; yi <= yr + 1; yi++)
                    {
                        for (int zi = zr - 1; zi <= zr + 1; zi++)
                        {
                            float3 vec = cell3D[Hash3D(m_seed, xi, yi, zi) & 255];

                            float vecX = xi - x + vec.x * m_cellularJitter;
                            float vecY = yi - y + vec.y * m_cellularJitter;
                            float vecZ = zi - z + vec.z * m_cellularJitter;

                            float newDistance = Math.Abs(vecX) + Math.Abs(vecY) + Math.Abs(vecZ);

                            for (int i = m_cellularDistanceIndex1; i > 0; i--)
                                distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
                            distance[0] = Math.Min(distance[0], newDistance);
                        }
                    }
                }
                break;
            case CellularDistanceFunction.Natural:
                for (int xi = xr - 1; xi <= xr + 1; xi++)
                {
                    for (int yi = yr - 1; yi <= yr + 1; yi++)
                    {
                        for (int zi = zr - 1; zi <= zr + 1; zi++)
                        {
                            float3 vec = cell3D[Hash3D(m_seed, xi, yi, zi) & 255];

                            float vecX = xi - x + vec.x * m_cellularJitter;
                            float vecY = yi - y + vec.y * m_cellularJitter;
                            float vecZ = zi - z + vec.z * m_cellularJitter;

                            float newDistance = (Math.Abs(vecX) + Math.Abs(vecY) + Math.Abs(vecZ)) + (vecX * vecX + vecY * vecY + vecZ * vecZ);

                            for (int i = m_cellularDistanceIndex1; i > 0; i--)
                                distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
                            distance[0] = Math.Min(distance[0], newDistance);
                        }
                    }
                }
                break;
            default:
                break;
        }

        switch (m_cellularReturnType)
        {
            case CellularReturnType.Distance2:
                return distance[m_cellularDistanceIndex1];
            case CellularReturnType.Distance2Add:
                return distance[m_cellularDistanceIndex1] + distance[m_cellularDistanceIndex0];
            case CellularReturnType.Distance2Sub:
                return distance[m_cellularDistanceIndex1] - distance[m_cellularDistanceIndex0];
            case CellularReturnType.Distance2Mul:
                return distance[m_cellularDistanceIndex1] * distance[m_cellularDistanceIndex0];
            case CellularReturnType.Distance2Div:
                return distance[m_cellularDistanceIndex0] / distance[m_cellularDistanceIndex1];
            default:
                return 0;
        }
    }

    public float GetCellular(float x, float y)
    {
        x *= m_frequency;
        y *= m_frequency;

        switch (m_cellularReturnType)
        {
            case CellularReturnType.CellValue:
            case CellularReturnType.Distance:
                return SingleCellular(x, y);
            default:
                return SingleCellular2Edge(x, y);
        }
    }

    private float SingleCellular(float x, float y)
    {
        int xr = FastRound(x);
        int yr = FastRound(y);

        float distance = 999999;
        int xc = 0, yc = 0;

        switch (m_cellularDistanceFunction)
        {
            default:
            case CellularDistanceFunction.Euclidean:
                for (int xi = xr - 1; xi <= xr + 1; xi++)
                {
                    for (int yi = yr - 1; yi <= yr + 1; yi++)
                    {
                        float2 vec = cell2D[Hash2D(m_seed, xi, yi) & 255];

                        float vecX = xi - x + vec.x * m_cellularJitter;
                        float vecY = yi - y + vec.y * m_cellularJitter;

                        float newDistance = vecX * vecX + vecY * vecY;

                        if (newDistance < distance)
                        {
                            distance = newDistance;
                            xc = xi;
                            yc = yi;
                        }
                    }
                }
                break;
            case CellularDistanceFunction.Manhattan:
                for (int xi = xr - 1; xi <= xr + 1; xi++)
                {
                    for (int yi = yr - 1; yi <= yr + 1; yi++)
                    {
                        float2 vec = cell2D[Hash2D(m_seed, xi, yi) & 255];

                        float vecX = xi - x + vec.x * m_cellularJitter;
                        float vecY = yi - y + vec.y * m_cellularJitter;

                        float newDistance = (Math.Abs(vecX) + Math.Abs(vecY));

                        if (newDistance < distance)
                        {
                            distance = newDistance;
                            xc = xi;
                            yc = yi;
                        }
                    }
                }
                break;
            case CellularDistanceFunction.Natural:
                for (int xi = xr - 1; xi <= xr + 1; xi++)
                {
                    for (int yi = yr - 1; yi <= yr + 1; yi++)
                    {
                        float2 vec = cell2D[Hash2D(m_seed, xi, yi) & 255];

                        float vecX = xi - x + vec.x * m_cellularJitter;
                        float vecY = yi - y + vec.y * m_cellularJitter;

                        float newDistance = (Math.Abs(vecX) + Math.Abs(vecY)) + (vecX * vecX + vecY * vecY);

                        if (newDistance < distance)
                        {
                            distance = newDistance;
                            xc = xi;
                            yc = yi;
                        }
                    }
                }
                break;
        }

        switch (m_cellularReturnType)
        {
            case CellularReturnType.CellValue:
                return ValCoord2D(m_seed, xc, yc);

            case CellularReturnType.Distance:
                return distance;
            default:
                return 0;
        }
    }

    private float SingleCellular2Edge(float x, float y)
    {
        int xr = FastRound(x);
        int yr = FastRound(y);

        float[] distance = { 999999, 999999, 999999, 999999 };

        switch (m_cellularDistanceFunction)
        {
            default:
            case CellularDistanceFunction.Euclidean:
                for (int xi = xr - 1; xi <= xr + 1; xi++)
                {
                    for (int yi = yr - 1; yi <= yr + 1; yi++)
                    {
                        float2 vec = cell2D[Hash2D(m_seed, xi, yi) & 255];

                        float vecX = xi - x + vec.x * m_cellularJitter;
                        float vecY = yi - y + vec.y * m_cellularJitter;

                        float newDistance = vecX * vecX + vecY * vecY;

                        for (int i = m_cellularDistanceIndex1; i > 0; i--)
                            distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
                        distance[0] = Math.Min(distance[0], newDistance);
                    }
                }
                break;
            case CellularDistanceFunction.Manhattan:
                for (int xi = xr - 1; xi <= xr + 1; xi++)
                {
                    for (int yi = yr - 1; yi <= yr + 1; yi++)
                    {
                        float2 vec = cell2D[Hash2D(m_seed, xi, yi) & 255];

                        float vecX = xi - x + vec.x * m_cellularJitter;
                        float vecY = yi - y + vec.y * m_cellularJitter;

                        float newDistance = Math.Abs(vecX) + Math.Abs(vecY);

                        for (int i = m_cellularDistanceIndex1; i > 0; i--)
                            distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
                        distance[0] = Math.Min(distance[0], newDistance);
                    }
                }
                break;
            case CellularDistanceFunction.Natural:
                for (int xi = xr - 1; xi <= xr + 1; xi++)
                {
                    for (int yi = yr - 1; yi <= yr + 1; yi++)
                    {
                        float2 vec = cell2D[Hash2D(m_seed, xi, yi) & 255];

                        float vecX = xi - x + vec.x * m_cellularJitter;
                        float vecY = yi - y + vec.y * m_cellularJitter;

                        float newDistance = (Math.Abs(vecX) + Math.Abs(vecY)) + (vecX * vecX + vecY * vecY);

                        for (int i = m_cellularDistanceIndex1; i > 0; i--)
                            distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
                        distance[0] = Math.Min(distance[0], newDistance);
                    }
                }
                break;
        }

        switch (m_cellularReturnType)
        {
            case CellularReturnType.Distance2:
                return distance[m_cellularDistanceIndex1];
            case CellularReturnType.Distance2Add:
                return distance[m_cellularDistanceIndex1] + distance[m_cellularDistanceIndex0];
            case CellularReturnType.Distance2Sub:
                return distance[m_cellularDistanceIndex1] - distance[m_cellularDistanceIndex0];
            case CellularReturnType.Distance2Mul:
                return distance[m_cellularDistanceIndex1] * distance[m_cellularDistanceIndex0];
            case CellularReturnType.Distance2Div:
                return distance[m_cellularDistanceIndex0] / distance[m_cellularDistanceIndex1];
            default:
                return 0;
        }
    }

    public void GradientPerturb(ref float x, ref float y, ref float z)
    {
        SingleGradientPerturb(m_seed, m_gradientPerturbAmp, m_frequency, ref x, ref y, ref z);
    }

    public void GradientPerturbFractal(ref float x, ref float y, ref float z)
    {
        int seed = m_seed;
        float amp = m_gradientPerturbAmp * m_fractalBounding;
        float freq = m_frequency;

        SingleGradientPerturb(seed, amp, m_frequency, ref x, ref y, ref z);

        for (int i = 1; i < m_octaves; i++)
        {
            freq *= m_lacunarity;
            amp *= m_gain;
            SingleGradientPerturb(++seed, amp, freq, ref x, ref y, ref z);
        }
    }

    void SingleGradientPerturb(int seed, float perturbAmp, float frequency, ref float x, ref float y, ref float z)
    {
        float xf = x * frequency;
        float yf = y * frequency;
        float zf = z * frequency;

        int x0 = FastFloor(xf);
        int y0 = FastFloor(yf);
        int z0 = FastFloor(zf);
        int x1 = x0 + 1;
        int y1 = y0 + 1;
        int z1 = z0 + 1;

        float xs, ys, zs;
        switch (m_interp)
        {
            default:
            case Interp.Linear:
                xs = xf - x0;
                ys = yf - y0;
                zs = zf - z0;
                break;
            case Interp.Hermite:
                xs = InterpHermiteFunc(xf - x0);
                ys = InterpHermiteFunc(yf - y0);
                zs = InterpHermiteFunc(zf - z0);
                break;
            case Interp.Quintic:
                xs = InterpQuinticFunc(xf - x0);
                ys = InterpQuinticFunc(yf - y0);
                zs = InterpQuinticFunc(zf - z0);
                break;
        }

        float3 vec0 = cell3D[Hash3D(seed, x0, y0, z0) & 255];
        float3 vec1 = cell3D[Hash3D(seed, x1, y0, z0) & 255];

        float lx0x = Lerp(vec0.x, vec1.x, xs);
        float ly0x = Lerp(vec0.y, vec1.y, xs);
        float lz0x = Lerp(vec0.z, vec1.z, xs);

        vec0 = cell3D[Hash3D(seed, x0, y1, z0) & 255];
        vec1 = cell3D[Hash3D(seed, x1, y1, z0) & 255];

        float lx1x = Lerp(vec0.x, vec1.x, xs);
        float ly1x = Lerp(vec0.y, vec1.y, xs);
        float lz1x = Lerp(vec0.z, vec1.z, xs);

        float lx0y = Lerp(lx0x, lx1x, ys);
        float ly0y = Lerp(ly0x, ly1x, ys);
        float lz0y = Lerp(lz0x, lz1x, ys);

        vec0 = cell3D[Hash3D(seed, x0, y0, z1) & 255];
        vec1 = cell3D[Hash3D(seed, x1, y0, z1) & 255];

        lx0x = Lerp(vec0.x, vec1.x, xs);
        ly0x = Lerp(vec0.y, vec1.y, xs);
        lz0x = Lerp(vec0.z, vec1.z, xs);

        vec0 = cell3D[Hash3D(seed, x0, y1, z1) & 255];
        vec1 = cell3D[Hash3D(seed, x1, y1, z1) & 255];

        lx1x = Lerp(vec0.x, vec1.x, xs);
        ly1x = Lerp(vec0.y, vec1.y, xs);
        lz1x = Lerp(vec0.z, vec1.z, xs);

        x += Lerp(lx0y, Lerp(lx0x, lx1x, ys), zs) * perturbAmp;
        y += Lerp(ly0y, Lerp(ly0x, ly1x, ys), zs) * perturbAmp;
        z += Lerp(lz0y, Lerp(lz0x, lz1x, ys), zs) * perturbAmp;
    }

    public void GradientPerturb(ref float x, ref float y)
    {
        SingleGradientPerturb(m_seed, m_gradientPerturbAmp, m_frequency, ref x, ref y);
    }

    public void GradientPerturbFractal(ref float x, ref float y)
    {
        int seed = m_seed;
        float amp = m_gradientPerturbAmp * m_fractalBounding;
        float freq = m_frequency;

        SingleGradientPerturb(seed, amp, m_frequency, ref x, ref y);

        for (int i = 1; i < m_octaves; i++)
        {
            freq *= m_lacunarity;
            amp *= m_gain;
            SingleGradientPerturb(++seed, amp, freq, ref x, ref y);
        }
    }

    void SingleGradientPerturb(int seed, float perturbAmp, float frequency, ref float x, ref float y)
    {
        float xf = x * frequency;
        float yf = y * frequency;

        int x0 = FastFloor(xf);
        int y0 = FastFloor(yf);
        int x1 = x0 + 1;
        int y1 = y0 + 1;

        float xs, ys;
        switch (m_interp)
        {
            default:
            case Interp.Linear:
                xs = xf - x0;
                ys = yf - y0;
                break;
            case Interp.Hermite:
                xs = InterpHermiteFunc(xf - x0);
                ys = InterpHermiteFunc(yf - y0);
                break;
            case Interp.Quintic:
                xs = InterpQuinticFunc(xf - x0);
                ys = InterpQuinticFunc(yf - y0);
                break;
        }

        float2 vec0 = cell2D[Hash2D(seed, x0, y0) & 255];
        float2 vec1 = cell2D[Hash2D(seed, x1, y0) & 255];

        float lx0x = Lerp(vec0.x, vec1.x, xs);
        float ly0x = Lerp(vec0.y, vec1.y, xs);

        vec0 = cell2D[Hash2D(seed, x0, y1) & 255];
        vec1 = cell2D[Hash2D(seed, x1, y1) & 255];

        float lx1x = Lerp(vec0.x, vec1.x, xs);
        float ly1x = Lerp(vec0.y, vec1.y, xs);

        x += Lerp(lx0x, lx1x, ys) * perturbAmp;
        y += Lerp(ly0x, ly1x, ys) * perturbAmp;
    }
}