﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace PGF
{
	public class FunctionExpression : Expression
	{
		public override R Accept<R> (IVisitor<R> visitor)
		{
			return visitor.VisitApplication (Name, new Expression[] {});
		}
		
		internal FunctionExpression (IntPtr expr, NativeGU.NativeMemoryPool pool) : base(expr,pool) {}
		public string Name => Native.NativeString.StringFromNativeUtf8(DataPtr);
	}
}

