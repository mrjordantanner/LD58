using UnityEngine;
using System.Collections;

public interface IInitializable 
{
    public IEnumerator Init();
    public string Name { get; }

}
