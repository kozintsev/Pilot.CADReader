using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.ComponentModel.Composition;
using Ascon.Pilot.SDK.ObjectCard;

namespace Ascon.Pilot.SDK.KompasAttrReader
{
    [Export(typeof(IAutoimportHandler))]
    [Export(typeof(IObjectCardHandler))]
    public class KompasAttrAutoImport : IAutoimportHandler, IObjectCardHandler
    {

        private readonly IObjectModifier _modifier;
        private readonly IPilotDialogService _dialogService;

        [ImportingConstructor]
        public KompasAttrAutoImport(IObjectModifier modifier, IPilotDialogService dialogService)
        {
            _modifier = modifier;
            _dialogService = dialogService;
        }


        public bool Handle(string filePath, string sourceFilePath, AutoimportSource autoimportSource)
        {
            try
            {
                var selection = _dialogService.ShowDocumentsSelectorDialog().ToList();
                if (selection.Count != 1)
                    return false;
                var document = selection.First();
                if (!document.Type.HasFiles)
                    MessageBox.Show("Error", "Selected element can not have files", MessageBoxButton.OK, MessageBoxImage.Error);

                var message = "Auto-imported from " + Localize(autoimportSource);
                _modifier
                    .Edit(document)
                    .CreateFileSnapshot(message)
                    .AddFile(filePath);
                _modifier.Apply();
                return true;
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        public bool Handle(IAttributeModifier modifier, ObjectCardContext context)
        {
            throw new NotImplementedException();
        }

        public bool OnValueChanged(IAttribute sender, AttributeValueChangedEventArgs args, IAttributeModifier modifier)
        {
            throw new NotImplementedException();
        }

        private string Localize(AutoimportSource autoimportSource)
        {
            switch (autoimportSource)
            {
                case AutoimportSource.Unknown:
                    return "Unknown";
                case AutoimportSource.PilotXps:
                    return "Pilot XPS printer";
                case AutoimportSource.UserFolder:
                    return "user auto-import directory";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
