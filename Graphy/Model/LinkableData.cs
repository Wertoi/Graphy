using GalaSoft.MvvmLight;


namespace Graphy.Model
{
    public class LinkableData<T> : ObservableObject
    {
        public LinkableData()
        {
            LinkedParameter = new DesignTableParameter();
        }

        private T _value;
        private bool _isLinkOn = false;
        private DesignTableParameter _linkedParameter;

        public T Value
        {
            get => _value;
            set
            {
                Set(() => Value, ref _value, value);
            }
        }

        public bool IsLinkOn
        {
            get => _isLinkOn;
            set
            {
                Set(() => IsLinkOn, ref _isLinkOn, value);
            }
        }

        public DesignTableParameter LinkedParameter
        {
            get => _linkedParameter;
            set
            {
                Set(() => LinkedParameter, ref _linkedParameter, value);

                if (LinkedParameter.Equals(DesignTableParameter.NoLinkParameter()) || LinkedParameter.Name == null)
                    IsLinkOn = false;
                else
                    IsLinkOn = true;
            }
        }
    }
}
