using Microsoft.CodeAnalysis;

namespace ExperimentalTools.Tests.Infrastructure.Diagnostics
{
    internal struct DiagnosticResult
    {
        private DiagnosticResultLocation[] locations;

        public DiagnosticResultLocation[] Locations
        {
            get
            {
                if (locations == null)
                {
                    locations = new DiagnosticResultLocation[] { };
                }

                return locations;
            }

            set
            {
                locations = value;
            }
        }

        public DiagnosticSeverity Severity { get; set; }

        public string Id { get; set; }

        public string Message { get; set; }

        public string Path => Locations.Length > 0 ? Locations[0].Path : "";

        public int Line => Locations.Length > 0 ? Locations[0].Line : -1;

        public int Column => Locations.Length > 0 ? Locations[0].Column : -1;
    }
}
