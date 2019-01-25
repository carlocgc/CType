//The MIT License (MIT) https://bitbucket.org/jacobalbano/glide

//Copyright (c) 2013 Jacob Albano

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Type.Glide
{
    internal class MemberAccessor
    {
        public object Target { get; private set; }
        public string MemberName { get; private set; }
        public System.Type MemberType { get; private set; }

        public object Value
        {
            get { return getMethod(Target); }
            set { setMethod(Target, value); }
        }

        public MemberAccessor(object target, string name, bool writeRequired = true)
        {
            var T = target.GetType();
            PropertyInfo propInfo = null;
            FieldInfo fieldInfo = null;
            Target = target;

            if ((propInfo = T.GetProperty(name, flags)) != null)
            {
                MemberType = propInfo.PropertyType;
                MemberName = propInfo.Name;

                {
                    var param = Expression.Parameter(typeof(object));
                    var instance = Expression.Convert(param, propInfo.DeclaringType);
                    var convert = Expression.TypeAs(Expression.Property(instance, propInfo), typeof(object));
                    getMethod = Expression.Lambda<Func<object, object>>(convert, param).Compile();
                }

                if (writeRequired)
                {
                    var param = Expression.Parameter(typeof(object));
                    var argument = Expression.Parameter(typeof(object));
                    var setterCall = Expression.Call(
                        Expression.Convert(param, propInfo.DeclaringType),
                        propInfo.GetSetMethod(),
                        Expression.Convert(argument, propInfo.PropertyType));

                    setMethod = Expression.Lambda<Action<object, object>>(setterCall, param, argument).Compile();
                }
            }
            else if ((fieldInfo = T.GetField(name, flags)) != null)
            {
                MemberType = fieldInfo.FieldType;
                MemberName = fieldInfo.Name;

                {
                    var self = Expression.Parameter(typeof(object));
                    var instance = Expression.Convert(self, fieldInfo.DeclaringType);
                    var field = Expression.Field(instance, fieldInfo);
                    var convert = Expression.TypeAs(field, typeof(object));
                    getMethod = Expression.Lambda<Func<object, object>>(convert, self).Compile();
                }

                {
                    var self = Expression.Parameter(typeof(object));
                    var value = Expression.Parameter(typeof(object));

                    var fieldExp = Expression.Field(Expression.Convert(self, fieldInfo.DeclaringType), fieldInfo);
                    var assignExp = Expression.Assign(fieldExp, Expression.Convert(value, fieldInfo.FieldType));

                    setMethod = Expression.Lambda<Action<object, object>>(assignExp, self, value).Compile();
                }
            }
            else
            {
                throw new Exception(string.Format("Field or {0} property '{1}' not found on object of type {2}.",
                        writeRequired ? "read/write" : "readable",
                        name, T.FullName));
            }
        }

        protected Func<object, object> getMethod;
        protected Action<object, object> setMethod;
        private static BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    }
}