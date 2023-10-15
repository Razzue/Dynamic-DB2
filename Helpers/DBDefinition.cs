using System;
using System.Collections.Generic;

public abstract class DBDefinition
{
    public bool IsUpdated = false;
    public Dictionary<int, object> Db2Data = new();
    public abstract bool Update();
    public void SortData(string property)
    {
        try
        {

        }
        catch (Exception e) { Console.WriteLine(e); }
    }
    public abstract object GetData(int id, string property);
    public abstract void SetData(int id, string property, object value);
}