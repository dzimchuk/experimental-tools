using System;

namespace ExperimentalTools.Options
{
    internal class FeatureState
    {
        private bool enabled;

        public FeatureState(Version currentVersion)
            : this(currentVersion, null, null)
        {
        }

        public FeatureState(Version currentVersion, Version from, Version to)
        {
            CurrentVersion = currentVersion;

            From = from;
            To = to;

            Enabled = true;
        }

        public bool Enabled
        {
            get => enabled;
            set
            {
                if (From != null && To != null)
                {
                    enabled = CurrentVersion >= From && CurrentVersion <= To ? value : false;
                }
                else if (From != null)
                {
                    enabled = CurrentVersion >= From ? value : false;
                }
                else if (To != null)
                {
                    enabled = CurrentVersion <= To ? value : false;
                }
                else
                {
                    enabled = value;
                }
            }
        }

        public Version From { get; }
        public Version To { get; }
        public Version CurrentVersion { get; }
    }
}
