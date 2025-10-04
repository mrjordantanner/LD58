using System;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(menuName = "Data/Inventory Data")]
public class LocalInventoryData : ScriptableObject
{
    public List<string> InventoryItems = new();

    public void ClearData()
    {
        Debug.Log($"Inventory: Clearing {InventoryItems.Count} Items from Local Inventory Data...");
        InventoryItems.Clear();

    }
}
