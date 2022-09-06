﻿// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Interfaces;
using Baracuda.Monitoring.Source.Types;
using Baracuda.Monitoring.Source.Units;
using Baracuda.Utilities.Extensions;
using Baracuda.Utilities.Reflection;
using UnityEngine;

namespace Baracuda.Monitoring.Source.Profiles
{
    public sealed class MethodProfile<TTarget, TValue> : NotifiableProfile<TTarget, TValue> where TTarget : class
    {
        private readonly Func<TTarget, MethodResult<TValue>> _getValueDelegate;

        private MethodProfile(
            MethodInfo methodInfo,
            MonitorAttribute attribute,
            MonitorProfileCtorArgs args) : base(methodInfo, attribute, typeof(TTarget), typeof(TValue),
            MemberType.Method, args)
        {
            var valueProcessor = MonitoringSystems.Resolve<IValueProcessorFactory>().CreateProcessorForType<TValue>(FormatData);
            
            var parameter = CreateParameterArray(methodInfo, attribute);
            _getValueDelegate = CreateGetDelegate(methodInfo, parameter, valueProcessor, FormatData, args.Settings);
        }

        internal override MonitorUnit CreateUnit(object target)
        {
            return new MethodUnit<TTarget, TValue>((TTarget)target, _getValueDelegate, this);
        }

        //--------------------------------------------------------------------------------------------------------------

        private static Func<TTarget, MethodResult<TValue>> CreateGetDelegate(MethodInfo methodInfo, object[] parameter, Func<TValue, string> valueProcessor, IFormatData format, IMonitoringSettings settings)
        {
            var sb = new StringBuilder();
            var parameterInfos = methodInfo.GetParameters();
            var parameterHandles = CreateParameterHandles(parameterInfos, format, settings);
            
            
            if (methodInfo.ReturnType == typeof(void))
            {
                var @void = new VoidValue().ConvertFast<VoidValue, TValue>();
                
                return target =>
                {
                    sb.Clear();
                    methodInfo.Invoke(target, parameter);
                    sb.Append(valueProcessor(@void));
                    foreach (var pair in parameterHandles)
                    {
                        sb.Append('\n');
                        var key = pair.Key;
                        var handle = pair.Value;
                        sb.Append(handle.GetValueAsString(parameter[key]));
                    }
                    return new MethodResult<TValue>(@void, sb.ToString());
                };
            }
            else
            {
                return target =>
                {
                    sb.Clear();
                    var result = (TValue)methodInfo.Invoke(target, parameter);
                    sb.Append(valueProcessor(result));
                    foreach (var pair in parameterHandles)
                    {
                        sb.Append('\n');
                        var key = pair.Key;
                        var handle = pair.Value;
                        sb.Append(handle.GetValueAsString(parameter[key]));
                    }
                    return new MethodResult<TValue>(result, sb.ToString());
                };
            }
        }

        private static Dictionary<int, OutParameterHandle> CreateParameterHandles(IReadOnlyList<ParameterInfo> parameterInfos, IFormatData format, IMonitoringSettings settings)
        {
            var handles = new Dictionary<int, OutParameterHandle>(parameterInfos.Count);
            for (var i = 0; i < parameterInfos.Count; i++)
            {
                var current = parameterInfos[i];
                if (current.IsOut)
                {
                    var outArgName = $"  {"out".ColorizeString(settings.OutParamColor)} {current.Name}";
                    var parameterFormat = new FormatData
                    {
                        Format = format.Format,
                        ShowIndexer = format.ShowIndexer,
                        Label = outArgName,
                        FontSize = format.FontSize,
                        Position = format.Position,
                        AllowGrouping = format.AllowGrouping,
                        Group = format.Group,
                        ElementIndent = Mathf.Max(format.ElementIndent * 2, 4)
                    };
                    
                    
                    var handle = OutParameterHandle.CreateForType(current.ParameterType, parameterFormat);
                    handles.Add(i, handle);
                }
            }
            return handles;
        }

        private static object[] CreateParameterArray(MethodInfo methodInfo, MonitorAttribute attribute)
        {
            var parameterInfos = methodInfo.GetParameters();
            var paramArray = new object[parameterInfos.Length];
            var monitorMethodAttribute = attribute as MonitorMethodAttribute;
            
            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var current = parameterInfos[i];
                var currentType = current.ParameterType;
                if (monitorMethodAttribute?.Args?.Length > i && !current.IsOut)
                {
                    paramArray[i] = Convert.ChangeType(monitorMethodAttribute.Args[i] ?? currentType.GetDefault(), currentType);
                }
                else
                {
                    var defaultValue = current.HasDefaultValue? current.DefaultValue : currentType.GetDefault();
                    paramArray[i] = defaultValue;
                }
            }

            return paramArray;
        }
    }
}