using System.Windows.Controls;

namespace SilverlightGlimpse.Models
{
    public class ValidationWrapper
    {
        #region Constructor

        public ValidationWrapper(
            ValidationErrorEventAction enumAction,
            string controlName,
            string controlType,
            string errorContent)
        {
            Action = enumAction;
            ControlName = controlName;
            ControlType = controlType;
            ErrorContent = errorContent;
        }

        #endregion

        #region Properties

        public ValidationErrorEventAction Action { get; private set; }
        public string ControlName { get; private set; }
        public string ControlType { get; private set; }
        public string ErrorContent { get; private set; }
        public string ToStringProperty { get { return ToString(); } }

        #endregion

        #region Methods

        public override string ToString()
        {
            return string.Format(
                "Validation Action: {0}, Name: {1} Type: {2}, ErrorContent: {3}",
                Action,
                ControlName,
                ControlType,
                ErrorContent);
        }


        #endregion
    }
}