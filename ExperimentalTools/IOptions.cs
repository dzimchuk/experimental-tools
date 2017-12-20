namespace ExperimentalTools
{
    public interface IOptions
    {
        bool IsFeatureEnabled(string identifier);
        string VSVersion { get; }
    }
}
