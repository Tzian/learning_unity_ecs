using Unity.Mathematics;

struct Grad_2D
{
    public float2 this[int index]
    {
        get
        {
            switch (index)
            {
                case 0: return new float2(-1, -1);
                case 1: return new float2(1, -1);
                case 2: return new float2(-1, 1);
                case 3: return new float2(1, 1);
                case 4: return new float2(0, -1);
                case 5: return new float2(-1, 0);
                case 6: return new float2(0, 1);
                case 7: return new float2(1, 0);

                default: throw new System.IndexOutOfRangeException();
            }
        }
    }
}

struct Grad_3D
{
    public float3 this[int index]
    {
        get
        {
            switch (index)
            {
                case 0: return new float3(1, 1, 0);
                case 1: return new float3(-1, 1, 0);
                case 2: return new float3(1, -1, 0);
                case 3: return new float3(-1, -1, 0);
                case 4: return new float3(1, 0, 1);
                case 5: return new float3(-1, 0, 1);
                case 6: return new float3(1, 0, -1);
                case 7: return new float3(-1, 0, -1);
                case 8: return new float3(0, 1, 1);
                case 9: return new float3(0, -1, 1);
                case 10: return new float3(0, 1, -1);
                case 11: return new float3(0, -1, -1);
                case 12: return new float3(1, 1, 0);
                case 13: return new float3(0, -1, 1);
                case 14: return new float3(-1, 1, 0);
                case 15: return new float3(0, -1, -1);

                default: throw new System.IndexOutOfRangeException();
            }
        }
    }
}