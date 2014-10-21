using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using System.Data;

namespace NzbDrone.Core.Datastore.Converters
{
	public class ConverterContext
	{
		public ColumnMap ColumnMap { get; set; }
		public object DbValue { get; set; }
		public ColumnMapCollection MapCollection { get; set; }
		public IDataRecord DataRecord { get; set; }
	}

}