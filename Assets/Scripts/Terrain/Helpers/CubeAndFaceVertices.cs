using Unity.Mathematics;


public struct CubeVertices
{
    public float3 v0;
    public float3 v1;
    public float3 v2;
    public float3 v3;
    public float3 v4;
    public float3 v5;
    public float3 v6;
    public float3 v7;

    public CubeVertices(bool param)
    {
        v0 = new float3(-0.5f, -0.5f,  0.5f); // Northwest bottom
        v1 = new float3( 0.5f, -0.5f,  0.5f); // Northeast bottom
        v2 = new float3( 0.5f, -0.5f, -0.5f); // Southeast bottom
        v3 = new float3(-0.5f, -0.5f, -0.5f); // Southwest bottom
        v4 = new float3(-0.5f,  0.5f,  0.5f); // Northwest top
        v5 = new float3( 0.5f,  0.5f,  0.5f); // Northeast top
        v6 = new float3( 0.5f,  0.5f, -0.5f); // Southeast top
        v7 = new float3(-0.5f,  0.5f, -0.5f); // Southwest top
    }

    public FaceVertices FaceVertices(int side)
    {
        switch (side)
        {
            case 0: // North
                return new FaceVertices(
                    v4 = new float3(-0.5f,  0.5f, 0.5f), // Northwest top
                    v5 = new float3( 0.5f,  0.5f, 0.5f), // Northeast top
                    v1 = new float3( 0.5f, -0.5f, 0.5f), // Northeast bottom
                    v0 = new float3(-0.5f, -0.5f, 0.5f)  // Northwest bottom
                );
            case 1: // South
                return new FaceVertices(
                    v6 = new float3( 0.5f,  0.5f, -0.5f), // Southeast top
                    v7 = new float3(-0.5f,  0.5f, -0.5f), // Southwest top
                    v3 = new float3(-0.5f, -0.5f, -0.5f), // Southwest bottom
                    v2 = new float3( 0.5f, -0.5f, -0.5f)  // Southeast bottom
                );
            case 2: // East
                return new FaceVertices(
                    v5 = new float3(0.5f,  0.5f,  0.5f), // Northeast top
                    v6 = new float3(0.5f,  0.5f, -0.5f), // Southeast top
                    v2 = new float3(0.5f, -0.5f, -0.5f), // Southeast bottom
                    v1 = new float3(0.5f, -0.5f,  0.5f)  // Northeast bottom
                );
            case 3: // West
                return new FaceVertices(
                    v7 = new float3(-0.5f,  0.5f, -0.5f), // Southwest top
                    v4 = new float3(-0.5f,  0.5f,  0.5f), // Northwest top
                    v0 = new float3(-0.5f, -0.5f,  0.5f), // Northwest bottom
                    v3 = new float3(-0.5f, -0.5f, -0.5f)  // Southwest bottom
                );
            case 4: // Up
                return new FaceVertices(
                    v7 = new float3(-0.5f, 0.5f, -0.5f), // Southwest top
                    v6 = new float3( 0.5f, 0.5f, -0.5f), // Southeast top
                    v5 = new float3( 0.5f, 0.5f,  0.5f), // Northeast top
                    v4 = new float3(-0.5f, 0.5f,  0.5f)  // Northwest top
                );
            case 5: // Down
                return new FaceVertices(
                    v0 = new float3(-0.5f, -0.5f,  0.5f), // Northwest bottom
                    v1 = new float3( 0.5f, -0.5f,  0.5f), // Northeast bottom
                    v2 = new float3( 0.5f, -0.5f, -0.5f), // Southeast bottom
                    v3 = new float3(-0.5f, -0.5f, -0.5f)  // Southwest bottom
                );
            default: throw new System.ArgumentOutOfRangeException("Index out of range 5: " + side);
        }
    }

    public float3 this[int vert]
    {
        get
        {
            switch (vert)
            {
                case 0: return v0;
                case 1: return v1;
                case 2: return v2;
                case 3: return v3;
                case 4: return v4;
                case 5: return v5;
                case 6: return v6;
                case 7: return v7;
                default: throw new System.ArgumentOutOfRangeException("Index out of range 7: " + vert);
            }
        }
    }
}

public struct FaceVertices
{
    public readonly float3 v0, v1, v2, v3;
    public FaceVertices(float3 v0, float3 v1, float3 v2, float3 v3)
    {
        this.v0 = v0;
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
    }

    public float3 this[int side]
    {
        get
        {
            switch (side)
            {
                case 0: return v0;
                case 1: return v1;
                case 2: return v2;
                case 3: return v3;
                default: throw new System.ArgumentOutOfRangeException("Index out of range 3: " + side);
            }
        }
    }
}