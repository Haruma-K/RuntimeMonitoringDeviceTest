﻿// Copyright (c) 2022 Jonathan Lang

using System;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring
{
    [Preserve]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Class)]
    public class MTextColorAttribute : MColorAttribute
    {
        /// <summary>
        /// Determine the text color for the displayed value.
        /// </summary>
        /// <param name="r">Red channel value</param>
        /// <param name="g">Green channel value</param>
        /// <param name="b">Blue channel value</param>
        /// <param name="a">Alpha channel value</param>
        public MTextColorAttribute(float r, float g, float b, float a = 1) : base(r, g, b, a)
        {
        }

        /// <summary>
        /// Determine the text color for the displayed value.
        /// </summary>
        /// <param name="colorPreset">Chose a preset of predefined color values</param>
        public MTextColorAttribute(ColorPreset colorPreset)  : base(colorPreset)
        {
        }
        
        /// <summary>
        /// Determine the text color for the displayed value.
        /// </summary>
        /// <param name="colorValueHex">Set the color via hexadecimal value</param>
        public MTextColorAttribute(string colorValueHex)  : base(colorValueHex)
        {
        } 
    }
}