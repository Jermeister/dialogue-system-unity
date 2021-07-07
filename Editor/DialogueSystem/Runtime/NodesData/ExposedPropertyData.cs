using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ExposedPropertyData
{
    public string propertyName;
    public string propertyValue;
    public BlackboardType propertyType;
}

[System.Serializable]
public enum BlackboardType
{
    None,
    Character,
}