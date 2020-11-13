using System;
using System.Collections.Generic;
using System.Text;

namespace DependectInjectionContainer
{
    public class DependencyInfo
    {
        public DependencyInfo(Type IType, Type OType, bool _isSingleton)
        {
            InterfaceType = IType;
            DependencyType = OType;
            isSingleton = _isSingleton;
        }
        public Type InterfaceType;
        public Type DependencyType;
        public bool isCreated = false;
        public bool isSingleton = false;
        public bool isOpenGeneric = false;
    }
}
