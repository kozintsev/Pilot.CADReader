using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace Ascon.Pilot.SDK.SpwReader
{
    class SpwReaderSettings : INotifyPropertyChanged,  IObserver<KeyValuePair<string, string>>, IObserver<IDataObject>
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IObjectsRepository _repository;
        private string settings;

        public SpwReaderSettings(IPersonalSettings personalSettings, IObjectsRepository repository)
        {
            _repository = repository;
             personalSettings.SubscribeSetting(SettingsFeatureKeys.FeatureKey).Subscribe(this);
        }

        public string Settings
        {
            get { return settings; }
        }

        public void OnCompleted()
        {
            
        }

        public void OnError(Exception error)
        {
            
        }

        public void OnNext(IDataObject value)
        {
            
        }

        public void OnNext(KeyValuePair<string, string> value)
        {
            if (value.Key == SettingsFeatureKeys.FeatureKey)
            {
                settings = value.Value.ToString();
            }
        }
    }
}
