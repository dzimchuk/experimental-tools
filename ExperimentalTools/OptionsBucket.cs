using System;
using System.Collections.Generic;

namespace ExperimentalTools
{
    public class OptionsBucket
    {
        private static Lazy<OptionsBucket> instance = new Lazy<OptionsBucket>(true);
        public static OptionsBucket Instance => instance.Value;

        public Dictionary<string, bool> Features { get; } = new Dictionary<string, bool>();
    }
}
