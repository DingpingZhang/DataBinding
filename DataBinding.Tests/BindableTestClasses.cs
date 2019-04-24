using System;
using System.Collections.Generic;
using System.Text;

namespace DataBinding.Tests
{
    public class A : BindableBase
    {
        private string _name;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private A _propertyA;

        public A PropertyA
        {
            get => _propertyA;
            set => SetProperty(ref _propertyA, value);
        }

        private B _propertyB;

        public B PropertyB
        {
            get => _propertyB;
            set => SetProperty(ref _propertyB, value);
        }
    }

    public class B : BindableBase
    {
        private bool _condition;

        public bool Condition
        {
            get => _condition;
            set => SetProperty(ref _condition, value);
        }

        private C _propertyC;

        public C PropertyC
        {
            get => _propertyC;
            set => SetProperty(ref _propertyC, value);
        }
    }

    public class C : BindableBase
    {
        private string _name;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}
