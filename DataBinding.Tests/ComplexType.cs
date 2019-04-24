using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DataBinding.Tests
{
    public class ComplexType : BindableBase
    {
        private string _stringProp;
        private int _intProp;
        private bool _boolProp;
        private ObservableCollection<ComplexType> _complexList;
        private ComplexType _nestedProp;
        private IList<ComplexType> _iListProperty;

        public string StringProp
        {
            get => _stringProp;
            set => SetProperty(ref _stringProp, value);
        }

        public int IntProp
        {
            get => _intProp;
            set => SetProperty(ref _intProp, value);
        }

        public bool BoolProp
        {
            get => _boolProp;
            set => SetProperty(ref _boolProp, value);
        }

        public ObservableCollection<ComplexType> ComplexList
        {
            get => _complexList;
            set => SetProperty(ref _complexList, value);
        }

        public IList<ComplexType> IListProperty
        {
            get => _iListProperty;
            set => SetProperty(ref _iListProperty, value);
        }

        public ComplexType NestedProp
        {
            get => _nestedProp;
            set => SetProperty(ref _nestedProp, value);
        }

        public ComplexType NestedField;

        public static ComplexType operator +(ComplexType left, ComplexType right)
        {
            return new ComplexType
            {
                StringProp = left.StringProp + right.StringProp,
                IntProp = left.IntProp + right.IntProp,
                BoolProp = left.BoolProp && left.BoolProp
            };
        }
    }

}
