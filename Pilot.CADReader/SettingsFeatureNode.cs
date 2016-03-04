using System.Collections.Generic;

namespace Ascon.Pilot.SDK.CADReader
{
    class SettingsFeatureNode
    {
        private readonly List<string> _settings = new List<string>();

        public SettingsFeatureNode(string featureTitle)
        {
            FeatureTitle = featureTitle;
        }

        public List<string> Settings
        {
            get { return _settings; }
        }
        public string FeatureTitle { get; private set; }
    }
}
