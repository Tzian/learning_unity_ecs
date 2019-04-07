using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public class Data 
{
    private static Data m_Instance;
    public static Data Store
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new Data();
            }
            return m_Instance;
        }
    }

    public Matrix3D<Entity> viewZoneMatrix;


    public Data()
    {

    }
}
