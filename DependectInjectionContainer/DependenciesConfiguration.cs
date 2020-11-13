using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace DependectInjectionContainer
{
    public class DependenciesConfiguration
    {
        public Dictionary<Type, List<DependencyInfo>> Config = new Dictionary<Type, List<DependencyInfo>>();

        public void Registr<T1, T2>(bool isSingleton)
        {
            Registr(typeof(T1), typeof(T2), isSingleton);
        }

        public void Registr(Type IType, Type DType, bool isSingleton)
        {
            if (!IType.IsAssignableFrom(DType) && !IsOpenGenericType(IType))
            {
                return;
            }else if (IsOpenGenericType(IType) && IsOpenGenericType(DType))
            {
                if (DType.GetInterface(IType.Name) == null)
                {
                    return; 
                }
            }

            DependencyInfo DInfo = new DependencyInfo(IType, DType, isSingleton);

            if (IsOpenGenericType(IType) && IsOpenGenericType(DType))
            {
                DInfo.isOpenGeneric = true;
            }

            if (Config.ContainsKey(IType))
            {
                if (Config[IType].Where(item => item.DependencyType.Equals(DType)).Count() == 0)
                {
                    Config[IType].Add(DInfo);
                }
            }
            else
            {
                Config.Add(IType, new List<DependencyInfo>() { DInfo });
            }
        }

        public bool IsOpenGenericType(Type t)
        {
            if (!t.IsGenericType)
            {
                return false;
            }

            Type[] Params = t.GetGenericArguments();
            if (Params[0].FullName == null)
            {
                return true;
            }else
            {
                return false;
            }
        }
    }
}
