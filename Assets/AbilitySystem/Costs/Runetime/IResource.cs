public interface IResource
{
    public float ResourceAmount { get; }
    public string ResourceName { get; }
    public void Consume(float amount);
    public bool CanConsume(float amount);

}