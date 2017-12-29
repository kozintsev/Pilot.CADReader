using System;
using System.Collections.Generic;

namespace Ascon.Pilot.SDK.KompasAttrAutoImport
{
    public class SettingLoader : IObserver<KeyValuePair<string, string>>
    {
        public string Json { get; private set; }

        public SettingLoader(IPersonalSettings personalSettings)
        {
            personalSettings.SubscribeSetting("KompasAttrAutoImport-E74EA6D5-C31E-4FE2-84E9-5AB64E503126").Subscribe(this);
        }

        public void OnNext(KeyValuePair<string, string> value)
        {
            if (value.Key == "KompasAttrAutoImport-E74EA6D5-C31E-4FE2-84E9-5AB64E503126")
            {
                Json = value.Value;
            }
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }
    }
}
