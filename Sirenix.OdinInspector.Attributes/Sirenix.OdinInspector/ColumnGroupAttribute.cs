using System.Collections.Generic;
using System.Diagnostics;
using Sirenix.OdinInspector.Internal;

namespace Sirenix.OdinInspector;

public class ColumnGroupAttribute : PropertyGroupAttribute, ISubGroupProviderAttribute
{
	[Conditional("UNITY_EDITOR")]
	public class ColumnSubGroupAttribute : PropertyGroupAttribute
	{
		public ColumnSize Size;

		public ColumnSubGroupAttribute(ColumnGroupAttribute column, string groupId, float order)
			: base(groupId, order)
		{
			if (column == null)
			{
				Size = ColumnSize.Auto;
			}
			else
			{
				Size = column.Size;
			}
		}
	}

	public const string DEFAULT_ROW_NAME = "_DefaultRow";

	public string ColumnId;

	public List<ColumnGroupAttribute> Columns;

	public ColumnSize Size;

	public ColumnGroupAttribute(string rowId, string columnId, ColumnType columnType = ColumnType.Auto, float columnSize = 0f, float order = 0f)
		: base(rowId, order)
	{
		ColumnId = columnId;
		Size = new ColumnSize(columnType, columnSize);
		Columns = new List<ColumnGroupAttribute> { this };
	}

	public ColumnGroupAttribute(string rowId, string columnId, float columnSize, float order = 0f)
		: base(rowId, order)
	{
		ColumnId = columnId;
		Size = ((columnSize <= 0f) ? ColumnSize.Auto : ((!(columnSize <= 1f) || !(columnSize >= 0f)) ? ColumnSize.Pixel(columnSize) : ColumnSize.Percent(columnSize)));
		Columns = new List<ColumnGroupAttribute> { this };
	}

	public ColumnGroupAttribute(string columnId)
		: this("_DefaultRow", columnId)
	{
	}

	public ColumnGroupAttribute(string columnId, float columnSize, float order = 0f)
		: this("_DefaultRow", columnId, columnSize, order)
	{
	}

	public ColumnGroupAttribute(string columnId, ColumnType columnType, float columnSize, float order = 0f)
		: this("_DefaultRow", columnId, columnType, columnSize, order)
	{
	}

	public IList<PropertyGroupAttribute> GetSubGroupAttributes()
	{
		int num = 0;
		List<PropertyGroupAttribute> list = new List<PropertyGroupAttribute>(Columns.Count)
		{
			new ColumnSubGroupAttribute(this, GroupID + "/" + ColumnId, num++)
		};
		foreach (ColumnGroupAttribute column in Columns)
		{
			if (column.ColumnId != ColumnId)
			{
				list.Add(new ColumnSubGroupAttribute(column, GroupID + "/" + column.ColumnId, num++));
			}
		}
		return list;
	}

	public string RepathMemberAttribute(PropertyGroupAttribute attr)
	{
		ColumnGroupAttribute columnGroupAttribute = (ColumnGroupAttribute)attr;
		return GroupID + "/" + columnGroupAttribute.ColumnId;
	}

	protected override void CombineValuesWith(PropertyGroupAttribute other)
	{
		ColumnGroupAttribute columnGroupAttribute = (ColumnGroupAttribute)other;
		for (int i = 0; i < Columns.Count; i++)
		{
			if (Columns[i].ColumnId == columnGroupAttribute.ColumnId)
			{
				return;
			}
		}
		Columns.Add(columnGroupAttribute);
	}
}
