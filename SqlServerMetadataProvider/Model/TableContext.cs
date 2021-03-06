﻿namespace Std.Tools.Data.Metadata.Model
{
	public class TableContext
	{
		public GeneratedTextTransformation Transformation;
		public string TableName;

		public TableContext Column(string columnName,
			string MemberName = null,
			string Type = null,
			bool? IsNullable = null,
			string Conditional = null)
		{
			var c = Transformation.GetColumn(TableName, columnName);

			if (MemberName != null)
				c.MemberName = MemberName;
			if (Type != null)
				c.Type = Type;
			if (IsNullable != null)
				c.IsNullable = IsNullable.Value;
			if (Conditional != null)
				c.Conditional = Conditional;

			return this;
		}

		public TableContext FK(string fkName,
			string MemberName = null,
			AssociationType? AssociationType = null,
			bool? CanBeNull = null)
		{
			var c = Transformation.GetFK(TableName, fkName);

			if (MemberName != null)
				c.MemberName = MemberName;
			if (AssociationType != null)
				c.AssociationType = AssociationType.Value;
			if (CanBeNull != null)
				c.CanBeNull = CanBeNull.Value;

			return this;
		}
	}
}
