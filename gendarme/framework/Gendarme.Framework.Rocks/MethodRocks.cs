//
// Gendarme.Framework.Rocks.MethodRocks
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Adrian Tsai <adrian_tsai@hotmail.com>
//	Daniel Abramov <ex@vingrad.ru>
//	Andreas Noever <andreas.noever@gmail.com>
//
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
// Copyright (c) 2007 Adrian Tsai
// Copyright (C) 2008 Daniel Abramov
// (C) 2008 Andreas Noever
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using Mono.Cecil;

namespace Gendarme.Framework.Rocks {

	// add CustomAttribute[Collection] extensions methods here
	// only if:
	// * you supply minimal documentation for them (xml)
	// * you supply unit tests for them
	// * they are required somewhere to simplify, even indirectly, the rules
	//   (i.e. don't bloat the framework in case of x, y or z in the future)

	/// <summary>
	/// MethodRocks contains extensions methods for Method[Definition|Reference]
	/// and the related collection classes.
	/// 
	/// Note: whenever possible try to use MethodReference since it's extend the
	/// reach/usability of the code.
	/// </summary>
	public static class MethodRocks {

		/// <summary>
		/// Check if the method contains an attribute of a specified type.
		/// </summary>
		/// <param name="self">The MethodDefinition on which the extension method can be called.</param>
		/// <param name="attributeName">Full name of the attribute class</param>
		/// <returns>True if the method contains an attribute of the same name,
		/// False otherwise.</returns>
		public static bool HasAttribute (this MethodDefinition self, string attributeName)
		{
			return self.CustomAttributes.ContainsType (attributeName);
		}

		/// <summary>
		/// Check if the MethodReference is defined as the entry point of it's assembly.
		/// </summary>
		/// <param name="self">The MethodReference on which the extension method can be called.</param>
		/// <returns>True if the method is defined as the entry point of it's assembly, False otherwise</returns>
		public static bool IsEntryPoint (this MethodReference self)
		{
			return (self == self.DeclaringType.Module.Assembly.EntryPoint);
		}

		/// <summary>
		/// Check if the MethodReference is a finalizer.
		/// </summary>
		/// <param name="self">The MethodReference on which the extension method can be called.</param>
		/// <returns>True if the method is a finalizer, False otherwise.</returns>
		public static bool IsFinalizer (this MethodReference self)
		{
			return (self.HasThis && (self.Parameters.Count == 0) && (self.Name == "Finalize") &&
				(self.ReturnType.ReturnType.FullName == "System.Void"));
		}

		/// <summary>
		/// Check if the method, or it's declaring type, was generated by the compiler or a tool (i.e. not by the developer).
		/// </summary>
		/// <param name="self">The MethodDefinition on which the extension method can be called.</param>
		/// <returns>True if the code is not generated directly by the developer, 
		/// False otherwise (e.g. compiler or tool generated)</returns>
		public static bool IsGeneratedCode (this MethodDefinition self)
		{
			if (self.CustomAttributes.ContainsAnyType (CustomAttributeRocks.GeneratedCodeAttributes))
				return true;

			return self.DeclaringType.IsGeneratedCode ();
		}

		/// <summary>
		/// Check if the signature of a method is consitent for it's use as a Main method.
		/// Note: it doesn't check that the method is the EntryPoint of it's assembly.
		/// <code>
		/// static [void|int] Main ()
		/// static [void|int] Main (string[] args)
		/// </code>
		/// </summary>gre
		/// <param name="self">The MethodDefinition on which the extension method can be called.</param>
		/// <returns>True if the method is a valid Main, False otherwise.</returns>
		public static bool IsMain (this MethodDefinition self)
		{
			// Main must be static
			if (!self.IsStatic)
				return false;

			if (self.Name != "Main")
				return false;

			// Main must return void or int
			switch (self.ReturnType.ReturnType.Name) {
			case "Void":
			case "Int32":
				// ok, continue checks
				break;
			default:
				return false;
			}

			switch (self.Parameters.Count) {
			case 0:
				// Main (void)
				return true;
			case 1:
				// Main (string[] args)
				return (self.Parameters [0].ParameterType.Name == "String[]");
			default:
				return false;
			}
		}

		/// <summary>
		/// Check if the method corresponds to the get or set operation on a property.
		/// </summary>
		/// <param name="self">The MethodDefinition on which the extension method can be called.</param>
		/// <returns>True if the method is a getter or a setter, False otherwise</returns>
		public static bool IsProperty (this MethodDefinition self)
		{
			return ((self.SemanticsAttributes & (MethodSemanticsAttributes.Getter | MethodSemanticsAttributes.Setter)) != 0);
		}

		/// <summary>
		/// Check if the method is visible outside of the assembly.
		/// </summary>
		/// <param name="self">The MethodDefinition on which the extension method can be called.</param>
		/// <returns>True if the method can be used from outside of the assembly, false otherwise.</returns>
		public static bool IsVisible (this MethodDefinition self)
		{
			if (self.IsPrivate || self.IsAssembly)
				return false;
			return ((TypeDefinition) self.DeclaringType).IsVisible ();
		}
	}
}
