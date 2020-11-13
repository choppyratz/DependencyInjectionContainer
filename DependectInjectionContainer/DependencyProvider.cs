using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Collections;


namespace DependectInjectionContainer
{
    public class DependencyProvider
    {
        private DependenciesConfiguration Config;
        private ArrayList singletons = new ArrayList();
        private Stack<int> stack = new Stack<int>();
        public DependencyProvider (DependenciesConfiguration _config) 
        {
            Config = _config;
        }

        public object Resolve<T>()
        {
            return Resolve(typeof(T));

        }

        public object Resolve(Type type)
        {
            try
            {
                if (stack.Contains(type.MetadataToken))
                {
                    return null;
                }

                stack.Push(type.MetadataToken);
                Type targetType;
                if (type.MetadataToken == typeof(IEnumerable<>).MetadataToken)
                {
                    Type[] gParams = type.GetGenericArguments();
                    targetType = gParams[0];
                }
                else
                {
                    targetType = type;
                }

                List<DependencyInfo> DInfo = null;
                bool isOpenGeneric = false;

                if (!Config.Config.ContainsKey(targetType) && targetType.IsGenericType)
                {
                    DInfo = Config.Config.Where(item => item.Key.Name == targetType.Name).First().Value;

                    if (!DInfo[0].isOpenGeneric)
                    {
                        throw new Exception();
                    }else
                    {
                        DInfo[0].DependencyType = DInfo[0].DependencyType.MakeGenericType(type.GetGenericArguments());
                        DInfo[0].InterfaceType = DInfo[0].InterfaceType.MakeGenericType(type.GetGenericArguments());
                        isOpenGeneric = true;
                    }
                }
                else
                {
                    DInfo = Config.Config[targetType];
                }

                if (type.MetadataToken == typeof(IEnumerable<>).MetadataToken)
                {
                    List<object> result = new List<object>();
                    Type IType = null;
                    foreach (DependencyInfo info in DInfo)
                    {
                        if (isOpenGeneric)
                        {
                            info.DependencyType = info.DependencyType.MakeGenericType(type.GetGenericArguments());
                            info.InterfaceType = info.InterfaceType.MakeGenericType(type.GetGenericArguments());
                        }
                        if (info.isSingleton && info.isCreated)
                        {
                            object obj = getSingletonIfExists(info.DependencyType);
                            if (obj != null)
                            {
                                result.Add(obj);
                            }
                            else
                            {
                                obj = CreateDependency(info.DependencyType, info);
                                singletons.Add(obj);
                                result.Add(obj);
                            }
                        }
                        else if (info.isSingleton && !info.isCreated)
                        {
                            object obj = CreateDependency(info.DependencyType, info);
                            singletons.Add(obj);
                            result.Add(obj);
                        }
                        else
                        {
                            result.Add(CreateDependency(info.DependencyType, info));
                        }
                        IType = info.InterfaceType;
                    }

                    Type d1 = typeof(IEnumerable<>);
                    var d2 = d1.MakeGenericType(IType);
                    return ConvertList(result, d2);
                }

                if (DInfo[0].isSingleton && DInfo[0].isCreated)
                {
                    object obj = getSingletonIfExists(DInfo[0].DependencyType);
                    if (obj != null)
                    {
                        return obj;
                    }
                    return null;
                }
                else if (DInfo[0].isSingleton && !DInfo[0].isCreated)
                {
                    object obj = CreateDependency(DInfo[0].DependencyType, DInfo[0]);
                    singletons.Add(obj);
                    return obj;
                }else
                {
                    return CreateDependency(DInfo[0].DependencyType, DInfo[0]);
                }
            }
            catch (Exception e)
            {
                return null;
            }
            finally
            {
                if (stack.Count != 0)
                {
                    stack.Pop();
                }
            }

        }

        public object CreateDependency(Type type, DependencyInfo info)
        {
            object obj = null;
            ConstructorInfo[] constructors = type.GetConstructors();
            constructors = constructors.OrderByDescending(constructor => constructor.GetParameters().Length).
                Append(typeof(object).GetConstructor(Type.EmptyTypes)).
                ToArray();

            for (int i = 0; i < constructors.Length; i++)
            {
                ParameterInfo[] parameters = constructors[i].GetParameters();
                object[] constructorParams = new object[parameters.Length];
                for (int j = 0; j < parameters.Length; j++)
                {
                    object result;
                    if (parameters[j].ParameterType.IsGenericTypeDefinition)
                    {
                        Type d1 = parameters[j].ParameterType;
                        Type constructed = d1.MakeGenericType(new Type[] { parameters[j].ParameterType.GetGenericArguments()[0] });
                        result = Activator.CreateInstance(constructed);
                    }
                    result = Resolve(parameters[j].ParameterType);
                    constructorParams[j] = result;
                }

               try
               {
                    obj = Activator.CreateInstance(type, constructorParams);
                    info.isCreated = true;
                    break;
               }
               catch(Exception e) { }
            }
            return obj;
        }

        private object getSingletonIfExists(Type type)
        {
            foreach (var obj in singletons)
            {
                if (obj.GetType().FullName == type.FullName)
                {
                    return obj;
                }
            }
            return null;
        }
        private object ConvertList(List<object> items, Type type, bool performConversion = false)
        {
            var containedType = type.GenericTypeArguments.First();
            var enumerableType = typeof(System.Linq.Enumerable);
            var castMethod = enumerableType.GetMethod(nameof(System.Linq.Enumerable.Cast)).MakeGenericMethod(containedType);
            var toListMethod = enumerableType.GetMethod(nameof(System.Linq.Enumerable.ToList)).MakeGenericMethod(containedType);

            IEnumerable<object> itemsToCast = items;

            var castedItems = castMethod.Invoke(null, new[] { itemsToCast });

            return toListMethod.Invoke(null, new[] { castedItems });
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
            }
            else
            {
                return false;
            }
        }
    }
}
