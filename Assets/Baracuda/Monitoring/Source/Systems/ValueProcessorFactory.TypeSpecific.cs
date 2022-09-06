// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.API;
using Baracuda.Utilities.Extensions;
using Baracuda.Utilities.Reflection;
using UnityEngine;

namespace Baracuda.Monitoring.Source.Systems
{
    /// <summary>
    /// Class creates custom ValueProcessor delegates for Monitoring units based on their values type.
    /// </summary>
    internal partial class ValueProcessorFactory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Func<TValue, string> CreateTypeSpecificProcessorInternal<TValue>(IFormatData formatData)
        {
            var type = typeof(TValue);

            // Global predefined value processor for type
            if (_globalValueProcessors.TryGetValue(type, out var globalProcessor) 
                && globalProcessor is Func<IFormatData, TValue, string> typeSpecificGlobal)
            {
                return (value) => typeSpecificGlobal(formatData, value);
            }
            
            if (type == typeof(Transform))
            {
                return (Func<TValue, string>)(Delegate) TransformProcessor(formatData);
            }

            if (type == typeof(bool))
            {
                return (Func<TValue, string>)(Delegate) CreateBooleanProcessor(formatData);
            }

            if (type == typeof(bool[]))
            {
                return (Func<TValue, string>)(Delegate) BooleanArrayProcessor(formatData);
            }
            
            // Dictionary<TKey, TValue>
            if (type.IsGenericIDictionary())
            {
                try
                {
                    var keyType = type.GetGenericArguments()[0];
                    var valueType = type.GetGenericArguments()[1];
                    var genericMethod = createDictionaryProcessorMethod.MakeGenericMethod(keyType, valueType);
                    return (Func<TValue, string>) genericMethod.Invoke(null, new object[] {formatData});
                }
#pragma warning disable CS0618
                //IL2CPP runtime does throw this exception!
                catch (ExecutionEngineException engineException)
#pragma warning restore CS0618
                {
                    Debug.LogException(engineException);
                }
            }
            
            // IEnumerable<bool>
            if (type.HasInterface<IEnumerable<bool>>())
            {
                return (Func<TValue, string>) (Delegate) IEnumerableBooleanProcessor(formatData);
            }
            
            if (type.IsArray)
            {
                try
                {
                    var elementType = type.GetElementType();
                
                    Debug.Assert(elementType != null, nameof(elementType) + " != null");
                
                    var genericMethod = elementType.IsValueType ? createValueTypeArrayMethod.MakeGenericMethod(elementType) : createReferenceTypeArrayMethod.MakeGenericMethod(elementType);
                
                    return (Func<TValue, string>) genericMethod.Invoke(null, new object[]{formatData});
                }
#pragma warning disable CS0618 
                //IL2CPP runtime does throw this exception!
                catch (ExecutionEngineException engineException)
#pragma warning restore CS0618
                {
                    Debug.LogException(engineException);
                }
            }

            // IEnumerable<T>
            if (type.IsGenericIEnumerable(true))
            {
                try
                {
                    var elementType = type.GetElementType() ?? type.GetGenericArguments()[0];
                    var genericMethod = createGenericIEnumerableMethod.MakeGenericMethod(elementType);
                    return (Func<TValue, string>) genericMethod.Invoke(null, new object[]{formatData});
                }
#pragma warning disable CS0618 
                //IL2CPP runtime does throw this exception!
                catch (ExecutionEngineException engineException)
#pragma warning restore CS0618
                {
                    Debug.LogException(engineException);
                }
            }
            
            if (type.IsIEnumerable(true))
            {
                return (Func<TValue, string>) (Delegate) IEnumerableProcessor(formatData, type);
            }

            if (type == typeof(Quaternion))
            {
                return (Func<TValue, string>) (Delegate) QuaternionProcessor(formatData);
            }

            if (type == typeof(Vector3))
            {
                return (Func<TValue, string>) (Delegate) Vector3Processor(formatData);
            }
            
            if (type == typeof(Vector2))
            {
                return (Func<TValue, string>) (Delegate) Vector2Processor(formatData);
            }

            if (type == typeof(Color))
            {
                return (Func<TValue, string>) (Delegate) ColorProcessor(formatData);
            }
            
            if (type == typeof(Color32))
            {
                return (Func<TValue, string>) (Delegate) Color32Processor(formatData);
            }

            if (type.HasInterface<IFormattable>() && formatData.Format != null)
            {
                return FormattedProcessor<TValue>(formatData);
            }
            
            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                return (Func<TValue, string>) (Delegate) UnityEngineObjectProcessor(formatData);
            }
            
            if (type.IsInt32())
            {
                return (Func<TValue, string>) (Delegate) Int32Processor(formatData);
            }
            
            if (type.IsInt64())
            {
                return (Func<TValue, string>) (Delegate) Int64Processor(formatData);
            }
            
            if (type.IsSingle())
            {
                return (Func<TValue, string>) (Delegate) SingleProcessor(formatData);
            }
            
            if (type.IsDouble())
            {
                return (Func<TValue, string>) (Delegate) DoubleProcessor(formatData);
            }

            if (type.IsValueType)
            {
                return ValueTypeProcessor<TValue>(formatData);
            }
            
            // Everything else that is a reference type.
            return DefaultProcessor<TValue>(formatData);
        }
    }
}