using System;
using System.ComponentModel.Composition;
using Ascon.Pilot.SDK.ObjectCard;

namespace Ascon.Pilot.SDK.KompasAttrReader
{
    [Export(typeof(IAutoimportHandler))]
    [Export(typeof(IObjectCardHandler))]
    public class KompasAttrReader : IAutoimportHandler, IObjectCardHandler
    {

        private readonly IObjectModifier _modifier;
        private readonly IPilotDialogService _dialogService;

        [ImportingConstructor]
        public KompasAttrReader(IObjectModifier modifier, IPilotDialogService dialogService)
        {
            _modifier = modifier;
            _dialogService = dialogService;
        }


        public bool Handle(string filePath, string sourceFilePath, AutoimportSource autoimportSource)
        {
            throw new NotImplementedException();
        }

        public bool Handle(IAttributeModifier modifier, ObjectCardContext context)
        {
            throw new NotImplementedException();
        }

        public bool OnValueChanged(IAttribute sender, AttributeValueChangedEventArgs args, IAttributeModifier modifier)
        {
            throw new NotImplementedException();
        }
    }
}
