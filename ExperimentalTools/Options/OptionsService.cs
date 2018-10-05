using System.Composition;

namespace ExperimentalTools.Options
{
    [Export(typeof(IOptions))]
    internal class OptionsService : IOptions
    {
        public bool IsFeatureEnabled(string identifier)
        {
            var features = OptionsBucket.Instance.Features;
            return features.ContainsKey(identifier) ? features[identifier].Enabled : false;
        }
    }
}
