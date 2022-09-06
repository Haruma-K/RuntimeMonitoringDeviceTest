﻿// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Class)]
    public class MGroupElementAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// Whether or not the unit should be wrapped in an object or type group.
        /// </summary>
        public readonly bool GroupElement;

        /// <summary>
        /// Whether or not the unit should be wrapped in an object or type group.
        /// </summary>
        public MGroupElementAttribute(bool groupElement)
        {
            GroupElement = groupElement;
        }
    }
}