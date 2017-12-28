using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.ComponentModel.Composition;
using Ascon.Pilot.SDK.ObjectCard;

namespace Ascon.Pilot.SDK.KompasAttrAutoImport
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
            var isObjectModification = context.EditiedObject != null;
            if (isObjectModification || context.IsReadOnly)
                return false;

            var parent = context.Parent;
            var sourceAttr = context.DisplayAttributes.FirstOrDefault(a => a.Type == AttributeType.String);
            if (sourceAttr == null)
                return false;

            var sourceValue = parent.Attributes.FirstOrDefault(a => a.Key == sourceAttr.Name);
            if (sourceValue.Value == null)
                return false;

            var targetAttr = context.DisplayAttributes.FirstOrDefault(a => a.Type == AttributeType.String);
            if (targetAttr == null)
                return false;

            var valueToSet =
                $"Parent is {sourceValue.Value}; IsDocument:{context.Parent.Type.HasFiles}; Can be mount:{context.Parent.Type.IsMountable}";

            modifier.SetValue(targetAttr.Name, valueToSet);
            return true;
        }

        public bool OnValueChanged(IAttribute sender, AttributeValueChangedEventArgs args, IAttributeModifier modifier)
        {
            var currentAttributeValues = string.Empty;
            foreach (var displayAttribute in args.Context.DisplayAttributes)
            {
                currentAttributeValues += displayAttribute.Name == sender.Name
                    ? args.NewValue
                    : displayAttribute.Name + ": " + args.Context.AttributesValues[displayAttribute.Name] + Environment.NewLine;
            }

            if (args.Context.Type.Name == "Document" && sender.Name == "Sheet_number")
            {
                var newNameAttrValue = "Sheet no " + args.NewValue + "; " + (args.Context.EditiedObject == null ? " New object " : " Existed object");
                modifier.SetValue("Name", newNameAttrValue);
                return true;
            }

            return false;
        }

        private static string Localize(AutoimportSource autoimportSource)
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
