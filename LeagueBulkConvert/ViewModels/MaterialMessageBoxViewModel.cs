using System.Windows;
using System.Windows.Input;

namespace LeagueBulkConvert.ViewModels
{
    public class MaterialMessageBoxViewModel
    {
        public ICommand Ok { get; }

        public string ButtonText { get; set; } = "Ok";

        public string Message { get; set; }

        public Window Owner { get; set; }

        public string Title { get; set; } = "LeagueBulkConvert";

        public MaterialMessageBoxViewModel()
        {
            Ok = new Command(p => Owner.Close(), () => true);
        }
    }
}
