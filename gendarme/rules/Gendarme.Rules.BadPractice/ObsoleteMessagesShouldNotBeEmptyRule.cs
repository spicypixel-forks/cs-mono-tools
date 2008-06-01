// 
// Gendarme.Rules.BadPractice.ObsoleteMessagesShouldNotBeEmptyRule
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	[Problem ("The [Obsolete] attribute was used but no help, alternative or description was provided.")]
	[Solution ("Provide helpful advice to help developers abandon old features and migrate to newer ones.")]
	public class ObsoleteMessagesShouldNotBeEmptyRule : Rule, ITypeRule {

		private const string ObsoleteAttribute = "System.ObsoleteAttribute";
		private const string ErrorMessage = "An empty, unhelpful message is returned to the developer.";

		private static bool CheckAttributes (CustomAttributeCollection cac)
		{
			foreach (CustomAttribute ca in cac) {
				if (ca.Constructor.DeclaringType.FullName != ObsoleteAttribute)
					continue;

				// no parameter == empty description
				// note: we don't have to check fields since they cannot be used
				// (as the Message property isn't read/write it cannot be a named argument)
				if (ca.ConstructorParameters.Count == 0)
					return true;

				// Message is the first parameter in both ctors (with params)
				return String.IsNullOrEmpty ((string) ca.ConstructorParameters [0]);
			}
			// no System.ObsoleteAttribute found inside the collection
			return false;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			// handles AttributeTargets.[Class | Struct | Enum | Interface | Delegate]
			if (CheckAttributes (type.CustomAttributes))
				Runner.Report (type, Severity.Medium, Confidence.High, String.Empty);

			// handles AttributeTargets.Property
			// properties can be obsoleted - but this is different
			// than the getter/setter that CheckMethod will report
			foreach (PropertyDefinition property in type.Properties) {
				if (CheckAttributes (property.CustomAttributes))
					Runner.Report (property, Severity.Medium, Confidence.High, String.Empty);
			}

			// handle AttributeTargets.Event
			foreach (EventDefinition evnt in type.Events) {
				if (CheckAttributes (evnt.CustomAttributes))
					Runner.Report (evnt, Severity.Medium, Confidence.High, String.Empty);
			}

			// handle AttributeTargets.Field
			foreach (FieldDefinition field in type.Fields) {
				if (CheckAttributes (field.CustomAttributes))
					Runner.Report (field, Severity.Medium, Confidence.High, String.Empty);
			}

			// handles AttributeTargets.[Constructor | Method]
			foreach (MethodDefinition method in type.AllMethods ()) {
				if (CheckAttributes (method.CustomAttributes))
					Runner.Report (method, Severity.Medium, Confidence.High, String.Empty);
			}

			return Runner.CurrentRuleResult;
		}
	}
}