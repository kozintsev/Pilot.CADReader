using System.ComponentModel.Composition;
using System.Windows;

namespace Ascon.Pilot.SDK.CADReader
{
    [Export(typeof(ISettingsFeature))]
    public class SampleSettingsFeature : ISettingsFeature
    {
        private ISettingValueProvider _settingValueProvider;

        public void SetValueProvider(ISettingValueProvider settingValueProvider)
        {
            _settingValueProvider = settingValueProvider;
        }

        public string Key
        {
            get { return SettingsFeatureKeys.SampleFeatureKey; }
        }

        public string Title
        {
            get { return "Настройки Компас-спецификации"; }
        }

        public FrameworkElement Editor
        {
            get
            {
                return null;
            }
        }

    }

   
}
