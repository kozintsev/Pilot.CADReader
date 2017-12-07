using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace Ascon.Pilot.SDK.SpwReader
{
    internal class SpwReaderSettings : INotifyPropertyChanged,  IObserver<KeyValuePair<string, string>>, IObserver<IDataObject>
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IObjectsRepository _repository;

        public SpwReaderSettings(IPersonalSettings personalSettings, IObjectsRepository repository)
        {
            _repository = repository;
             personalSettings.SubscribeSetting(SettingsFeatureKeys.FeatureKey).Subscribe(this);
        }

        public string Settings { get; private set; }

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
                Settings = value.Value;
            }
        }
    }
}
