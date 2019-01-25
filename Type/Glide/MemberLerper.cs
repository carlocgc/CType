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

namespace Type.Glide
{
	public abstract class MemberLerper
	{
		[Flags]
		public enum Behavior
		{
			None = 0,
			Reflect = 1,
			Rotation = 2,
			RotationRadians = 4,
			RotationDegrees = 8,
			Round = 16
		}

		protected const float DEG = 180f / (float) Math.PI;
		protected const float RAD = (float) Math.PI / 180f;

		public abstract void Initialize(Object fromValue, Object toValue, Behavior behavior);
		public abstract object Interpolate(float t, object currentValue, Behavior behavior);
	}
}