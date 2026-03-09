using UnityEngine;

public abstract class ResourceDefinition : ScriptableObject
{
    [SerializeField] private string _resourceName;
    public abstract IResource CreateRuntimeResource();
}