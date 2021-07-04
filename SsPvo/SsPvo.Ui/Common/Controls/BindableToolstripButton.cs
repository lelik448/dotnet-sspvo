using System.ComponentModel;
using System.Windows.Forms;

namespace SsPvo.Ui.Common.Controls
{
    public class BindableToolstripButton : ToolStripButton, IBindableComponent
    {
        private BindingContext _bindingContext;
        private ControlBindingsCollection _dataBindings;
        [Browsable(false)]
        public BindingContext BindingContext
        {
            get => _bindingContext ?? (_bindingContext = new BindingContext());
            set => _bindingContext = value;
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ControlBindingsCollection DataBindings => _dataBindings ?? (_dataBindings = new ControlBindingsCollection(this));
    }
}
