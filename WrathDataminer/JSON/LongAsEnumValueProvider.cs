﻿using Kingmaker.Visual.CharacterSystem;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomBlueprints
{
    class LongAsEnumValueProvider : IValueProvider
    {
        FieldInfo field;
        Type enumType;
        public LongAsEnumValueProvider(FieldInfo field, Type enumType)
        {
            this.field = field;
            this.enumType = enumType;
        }
        public object GetValue(object target)
        {
            var value = field.GetValue(target);
            return Enum.ToObject(enumType, value);
        }
        public void SetValue(object target, object value)
        {
            field.SetValue(target, (long)value);
        }
    }
}
