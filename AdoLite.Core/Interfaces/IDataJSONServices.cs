using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoLite.Core.Interfaces
{
    /// <summary>
    /// Provides methods to convert between JSON strings, DataTables, DataSets, and common .NET types.
    /// </summary>
    public interface IDataJSONServices
    {
        /// <summary>
        /// Converts a DataTable into a JSON string representing a single JSON array of objects.
        /// </summary>
        /// <param name="dataTable">The DataTable to convert.</param>
        /// <returns>A JSON string representing the data in the DataTable.</returns>
        string DataTableToJSON(DataTable dataTable);

        /// <summary>
        /// Converts a DataSet into a JSON string representing multiple JSON arrays, one per DataTable.
        /// </summary>
        /// <param name="dataset">The DataSet to convert.</param>
        /// <returns>A JSON string representing the data in the DataSet.</returns>
        string DataSetToJSON(DataSet dataset);

        /// <summary>
        /// Converts a JSON string to a DataTable.
        /// </summary>
        /// <param name="json">The JSON string to convert.</param>
        /// <returns>A DataTable representing the JSON data.</returns>
        DataTable JSONToDataTable(string json);

        /// <summary>
        /// Converts a JSON string to a DataSet.
        /// </summary>
        /// <param name="json">The JSON string to convert.</param>
        /// <returns>A DataSet representing the JSON data.</returns>
        DataSet JSONToDataSet(string json);

        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A JSON string representing the object.</returns>
        string ObjectToJSON(object obj);

        /// <summary>
        /// Deserializes a JSON string into a strongly typed object.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>An instance of type T.</returns>
        T JSONToObject<T>(string json);

        /// <summary>
        /// Converts a list of objects to a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of objects in the list.</typeparam>
        /// <param name="list">The list to convert.</param>
        /// <returns>A JSON string representing the list.</returns>
        string ListToJSON<T>(List<T> list);

        /// <summary>
        /// Converts a JSON string to a list of strongly typed objects.
        /// </summary>
        /// <typeparam name="T">The type of objects in the resulting list.</typeparam>
        /// <param name="json">The JSON string to convert.</param>
        /// <returns>A list of objects of type T.</returns>
        List<T> JSONToList<T>(string json);
    }
}
