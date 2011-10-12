﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.f
// See the License for the specific language governing permissions and
// limitations under the License.

#if !SL3
namespace Castle.Components.DictionaryAdapter.Xml
{
	using System;
	using System.Xml.XPath;

	public class XPathReadOnlyCursor : XPathNode, IXmlCursor
	{
		private XPathNodeIterator iterator;

		private readonly ILazy<XPathNavigator> parent;
		private readonly XPathExpression path;
		private readonly IXmlIncludedTypeMap includedTypes;
		private readonly CursorFlags flags;

		public XPathReadOnlyCursor(ILazy<XPathNavigator> parent, CompiledXPath path, IXmlIncludedTypeMap includedTypes, CursorFlags flags)
		{
			if (parent == null)
				throw Error.ArgumentNull("parent");
			if (path == null)
				throw Error.ArgumentNull("path");
			if (includedTypes == null)
				throw Error.ArgumentNull("includedTypes");

			this.parent        = parent;
			this.path          = path.Path;
			this.includedTypes = includedTypes;
			this.flags         = flags;

			Reset();
		}

		public void Reset()
		{
			if (parent.HasValue)
				iterator = parent.Value.Select(path);
		}

		public bool MoveNext()
		{
			for (;;)
			{
				var hasNext
					= iterator != null
					&& iterator.MoveNext()
					&& (flags.AllowsMultipleItems() || !iterator.MoveNext());

				if (!hasNext)
					return SetAtEnd();
				if (SetAtNext())
					return true;
			}
		}

		private bool SetAtEnd()
		{
			node = null;
			type = null;
			return false;
		}

		private bool SetAtNext()
		{
			node = iterator.Current;

			IXmlIncludedType includedType;
			if (!includedTypes.TryGet(XsiType, out includedType))
				return false;

			type = includedType.ClrType;
			return true;
		}

		public void MoveTo(IXmlNode position)
		{
			var source = position as ILazy<XPathNavigator>;
			if (source == null || !source.HasValue)
				throw Error.CursorCannotMoveToGivenNode();

			var positionNode = source.Value;

			Reset();

			if (iterator != null)
				while (iterator.MoveNext())
					if (iterator.Current.IsSamePosition(positionNode))
						{ SetAtNext(); return; }

			throw Error.CursorCannotMoveToGivenNode();
		}

		public void MoveToEnd()
		{
			if (iterator != null)
				while (iterator.MoveNext()) ;
			SetAtEnd();
		}

		bool IXmlCursor.IsNil
		{
			get { return IsNil; }
			set { throw Error.CursorNotMutable(); }
		}

		public void SetAttribute(XmlName name, string value)
		{
			throw Error.CursorNotMutable();
		}

		public string EnsurePrefix(string namespaceUri)
		{
			throw Error.CursorNotMutable();
		}

		public void MakeNext(Type type)
		{
			throw Error.CursorNotMutable();
		}

		public void Create(Type type)
		{
			throw Error.CursorNotMutable();
		}

		public void Coerce(Type type)
		{
			throw Error.CursorNotMutable();
		}

		public void Remove()
		{
			throw Error.CursorNotMutable();
		}

		public void RemoveAllNext()
		{
			throw Error.CursorNotMutable();
		}

		public IXmlNode Save()
		{
			return new XPathNode(node.Clone(), type);
		}
	}
}
#endif