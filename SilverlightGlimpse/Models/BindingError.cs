namespace SilverlightGlimpse.Models
{
    public class BindingError
    {
        #region Fields

        private readonly string _controlName = string.Empty;

        #endregion

        #region Constructor

        public BindingError(string controlName, string controlTypeName, string propertyName, string path)
        {
            _controlName = controlName;
            ControlType = controlTypeName;
            PropertyName = propertyName;
            Path = path;
        }

        #endregion

        #region Properties

        public string ControlName { get { return string.IsNullOrEmpty(_controlName) ? "(none)" : _controlName; } }
        public string ControlType { get; private set; }
        public string Path { get; private set; }
        public string PropertyName { get; private set; }
        public string ToStringProperty { get { return ToString(); } }
 
        #endregion

        #region Methods

        public override string ToString()
        {
            return string.Format(
                "Control Name: {0}, Type: {1}, Property: {2}, Path: {3}",
                ControlName,
                ControlType,
                PropertyName,
                Path);
        }

        #endregion
    }
}