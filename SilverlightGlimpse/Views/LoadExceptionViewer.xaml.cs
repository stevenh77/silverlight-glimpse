using System;

namespace SilverlightGlimpse.Views
{
    public partial class LoadExceptionViewer
    {
        public LoadExceptionViewer()
        {
            InitializeComponent();
        }

        public LoadExceptionViewer(Exception e, string sourceLocation) : this()
        {
            txtTitle.Text = string.Concat("Exception ", sourceLocation.TrimStart());

            var ex = e;

            while (ex != null)
            {
                lbExceptions.Items.Add(ex);
                ex = ex.InnerException;
            }

            if (lbExceptions.Items.Count > 0)
            {
                lbExceptions.SelectedIndex = 0;
            }
        }
    }
}