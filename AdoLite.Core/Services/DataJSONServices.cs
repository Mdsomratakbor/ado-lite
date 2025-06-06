using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdoLite.Core.Interfaces;
using System.Xml;
using Newtonsoft.Json;

namespace AdoLite.Core.Services
{
    public class DataJSONServices : IDataJSONServices
    {
        public virtual string DataTableToJSON(DataTable dataTable)
        {
            try
            {
                var rows = new List<Dictionary<string, object>>();

                foreach (DataRow row in dataTable.Rows)
                {
                    var rowDict = new Dictionary<string, object>();
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        rowDict[column.ColumnName] = row[column];
                    }
                    rows.Add(rowDict);
                }

                return JsonConvert.SerializeObject(rows);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error converting DataTable to JSON", ex);
            }
        }

        public virtual string DataSetToJSON(DataSet dataset)
        {
            try
            {
                var datasetDict = new Dictionary<string, object>();

                foreach (DataTable table in dataset.Tables)
                {
                    var tableJson = DataTableToJSON(table);
                    datasetDict[table.TableName] = JsonConvert.DeserializeObject(tableJson);
                }

                return JsonConvert.SerializeObject(datasetDict, Newtonsoft.Json.Formatting.Indented);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error converting DataSet to JSON", ex);
            }
        }

        public virtual DataTable JSONToDataTable(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<DataTable>(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error converting JSON to DataTable", ex);
            }
        }

        public virtual DataSet JSONToDataSet(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<DataSet>(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error converting JSON to DataSet", ex);
            }
        }

        public virtual string ObjectToJSON(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error converting object to JSON", ex);
            }
        }

        public virtual T JSONToObject<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error converting JSON to object of type {typeof(T).Name}", ex);
            }
        }

        public virtual string ListToJSON<T>(List<T> list)
        {
            try
            {
                return JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error converting list to JSON", ex);
            }
        }

        public virtual List<T> JSONToList<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<T>>(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error converting JSON to list of type {typeof(T).Name}", ex);
            }
        }
    }
}
