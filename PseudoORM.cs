using System.Data;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

public static class PseudoORM
{
    public static string conAddress = "localhost";
    public static string conDB = "maindb";
    public static string conUser = "root";
    public static string conPassword = "";
    public static string conPort = "3306";

    public static MySqlDataReader reader;
	static string connectionString = "server=" + conAddress + ";user="+conUser+";database="+conDB+";port="+conPort+";password="+conPassword+";default command timeout=30;Connection Timeout=30;";

    public static MySqlConnection connection = new MySqlConnection();


    /// <summary>
    /// Opens a new MySQL connection. Closes an existing connection if already exists.
    /// </summary>

    public static void DBOpenConnection()
    {
        if (connection.State == ConnectionState.Open)
        {
            MessageBox.Show("Database connection is already opened. Current connection will be closed then re-opened again.");
            connection.Close();
        }
		
        connection.ConnectionString = connectionString;
        connection.Open();

    }

    /// <summary>
    /// Closes an existing MySQL connection.
    /// </summary>

    public static void DBCloseConnection()
    {
        if (connection.State == ConnectionState.Closed)
        {
            MessageBox.Show("Database connection is already closed.");
            return;
        }

        connection.Close();

    }

    /// <summary>
    /// MySQL Select
    /// </summary>
    /// <param name="tableName">The name of table</param>
    /// <param name="dbqCollection">The array of DBQuery element (columnname, value)</param>
    public static DataTable DBSelect(string tableName, params DBQueryElement[] dbqCollection)
    {
        DataTable dt = new DataTable();

        DBOpenConnection();

        using (MySqlCommand command = new MySqlCommand())
        {
            command.Connection = connection;
            command.CommandText = "SELECT * FROM " + tableName + (dbqCollection.Length > 0 ? " WHERE " : "");
            for (int x = 0; x <= dbqCollection.Length - 1; x++)
            {
                DBQueryElement i = dbqCollection[x];
                command.CommandText += "" + i.queryName + " = @param_" + i.queryName + (x == dbqCollection.Length - 1 ? "" : " AND ");
                command.Parameters.AddWithValue("@param_" + i.queryName, i.queryValue);
            }

            MySqlDataAdapter da = new MySqlDataAdapter
            {
                SelectCommand = command
            };
            da.Fill(dt);

        }

        DBCloseConnection();

        return dt;
    }


    /// <summary>
    /// Execute a custom MySQL command
    /// </summary>
    /// <param name="query">The MySQL command string to execute</param>
    public static DataTable DBRaw(string query)
    {
        DataTable dt = new DataTable();

        DBOpenConnection();

        using (MySqlCommand command = new MySqlCommand())
        {
            command.Connection = connection;
            command.CommandText = query;

            MySqlDataAdapter da = new MySqlDataAdapter
            {
                SelectCommand = command
            };
            da.Fill(dt);

        }

        DBCloseConnection();

        return dt;
    }

    /// <summary>
    /// MySQL Select using LIKE, commonly used in search functions
    /// </summary>
    /// <param name="tableName">The name of table</param>
    /// <param name="dBQuery">The query using DBQuery element (columnname, value)</param>
    public static DataTable DBSelectLike(string tableName, DBQueryElement dBQuery)
    {
        DataTable dt = new DataTable();

        DBOpenConnection();

        using (MySqlCommand command = new MySqlCommand())
        {
            command.Connection = connection;
            command.CommandText = "SELECT * FROM " + tableName + " WHERE ";
            DBQueryElement i = dBQuery;
            command.CommandText += "" + i.queryName + " LIKE @param_" + i.queryName;
            command.Parameters.AddWithValue("@param_" + i.queryName, i.queryValue);

            MySqlDataAdapter da = new MySqlDataAdapter
            {
                SelectCommand = command
            };
            da.Fill(dt);

        }

        DBCloseConnection();

        return dt;
    }

    /// <summary>
    /// MySQL Select all records in a table
    /// </summary>
    /// <param name="tableName">The name of table</param>    
    public static DataTable DBSelectAll(string tableName)
    {
        DataTable dt = new DataTable();

        DBOpenConnection();

        using (MySqlCommand command = new MySqlCommand())
        {
            command.Connection = connection;
            command.CommandText = "SELECT * FROM " + tableName + ";";

            MySqlDataAdapter da = new MySqlDataAdapter
            {
                SelectCommand = command
            };
            da.Fill(dt);

        }

        DBCloseConnection();

        return dt;
    }

    /// <summary>
    /// MySQL Select only chosen columns
    /// </summary>
    /// <param name="tableName">The name of table</param>
    /// <param name="output">The column you want to select</param>
    /// <param name="dbqCollection">The array of DBQuery element (columnname, value)</param>
    public static DataTable DBSelect(string tableName, string output, params DBQueryElement[] dbqCollection)
    {
        DataTable dt = new DataTable();

        DBOpenConnection();

        using (MySqlCommand command = new MySqlCommand())
        {
            command.Connection = connection;

            if (dbqCollection == null)
            {
                command.CommandText = "SELECT " + output + " FROM " + tableName;
            }
            else
            {
                command.CommandText = "SELECT " + output + " FROM " + tableName + (dbqCollection.Length > 0 ? " WHERE " : "");

                for (int x = 0; x <= dbqCollection.Length - 1; x++)
                {
                    DBQueryElement i = dbqCollection[x];

                    command.CommandText += "" + i.queryName + " = @param_" + i.queryName + (x == dbqCollection.Length - 1 ? "" : " AND ");
                    command.Parameters.AddWithValue("@param_" + i.queryName, i.queryValue);
                }
            }


            MySqlDataAdapter da = new MySqlDataAdapter
            {
                SelectCommand = command
            };
            da.Fill(dt);

        }

        DBCloseConnection();

        return dt;
    }

    /// <summary>
    /// MySQL Insert
    /// </summary>
    /// <param name="tableName">The name of table</param>
    /// <param name="dbqCollection">The array of DBQuery element (columnname, value to insert)</param>
    public static int DBInsert(string tableName, params DBQueryElement[] dbqCollection)
    {
        DBOpenConnection();

        long id = -1;
        using (MySqlCommand command = new MySqlCommand())
        {
            command.Connection = connection;
            command.CommandText = "INSERT INTO " + tableName + " (";

            for (int x = 0; x <= dbqCollection.Length - 1; x++)
            {
                DBQueryElement i = dbqCollection[x];
                command.CommandText += "" + i.queryName + "" + (x == dbqCollection.Length - 1 ? "" : ", ");
            }

            command.CommandText += ") VALUES (";

            for (int x = 0; x <= dbqCollection.Length - 1; x++)
            {
                DBQueryElement i = dbqCollection[x];
                command.CommandText += "@param_" + i.queryName + "" + (x == dbqCollection.Length - 1 ? "" : ", ");
                MySqlParameter p = new MySqlParameter("@param_" + i.queryName, i.queryValue);
                command.Parameters.Add(p);
            }

            command.CommandText += ")";

            command.ExecuteNonQuery();
            id = command.LastInsertedId;
        }
        
        DBCloseConnection();
        return (int)id;
    }

    /// <summary>
    /// MySQL Replace
    /// </summary>
    /// <param name="tableName">The name of table</param>
    /// <param name="dbqCollection">The array of DBQuery element (columnname, value to be replaced)</param>
    public static int DBReplace(string tableName, params DBQueryElement[] dbqCollection)
    {
        DBOpenConnection();

        long id = -1;
        using (MySqlCommand command = new MySqlCommand())
        {
            command.Connection = connection;
            command.CommandText = "REPLACE INTO " + tableName + " (";

            for (int x = 0; x <= dbqCollection.Length - 1; x++)
            {
                DBQueryElement i = dbqCollection[x];
                command.CommandText += "" + i.queryName + "" + (x == dbqCollection.Length - 1 ? "" : ", ");
            }

            command.CommandText += ") VALUES (";

            for (int x = 0; x <= dbqCollection.Length - 1; x++)
            {
                DBQueryElement i = dbqCollection[x];
                command.CommandText += "@param_" + i.queryName + "" + (x == dbqCollection.Length - 1 ? "" : ", ");
                MySqlParameter p = new MySqlParameter("@param_" + i.queryName, i.queryValue);
                command.Parameters.Add(p);
            }

            command.CommandText += ")";

            command.ExecuteNonQuery();
            id = command.LastInsertedId;
        }

        DBCloseConnection();
        return (int)id;
    }

    /// <summary>
    /// MySQL Update
    /// </summary>
    /// <param name="tableName">The name of table</param>
    /// <param name="id">The ID of the record to be updated</param>
    /// <param name="dbqCollection">The array of DBQuery element (columnname, value to update)</param>
    public static void DBUpdate(string tableName, int id, params DBQueryElement[] dbqCollection)
	{
		DBUpdate(tableName, "ID", id, dbqCollection);
	}

    /// <summary>
    /// MySQL Update
    /// </summary>
    /// <param name="tableName">The name of table</param>
    /// <param name="idName">The column name of the Primary Key</param>
    /// <param name="id">The ID of the record to be updated</param>
    /// <param name="dbqCollection">The array of DBQuery element (columnname, value to update)</param>
    public static void DBUpdate(string tableName, string idName, int id, params DBQueryElement[] dbqCollection)
    {
        DBOpenConnection();

        using (MySqlCommand command = new MySqlCommand())
        {
            command.Connection = connection;
            command.CommandText = "UPDATE " + tableName + " SET ";

            for (int x = 0; x <= dbqCollection.Length - 1; x++)
            {
                DBQueryElement i = dbqCollection[x];
                command.CommandText += "" + i.queryName + " = @param_" + i.queryName + "" + (x == dbqCollection.Length - 1 ? "" : ", ");
                MySqlParameter p = new MySqlParameter("@param_" + i.queryName, i.queryValue);
                command.Parameters.Add(p);
            }

            command.CommandText += " WHERE "+idName+" = " + id.ToString();

            command.ExecuteNonQuery();

        }

        DBCloseConnection();
	}

    /// <summary>
    /// MySQL Delete
    /// </summary>
    /// <param name="tableName">The name of table</param>
    /// <param name="id">The ID of the record to be deleted</param>
    public static void DBDelete(string tableName, int id)
    {
        DBOpenConnection();

        using (MySqlCommand command = new MySqlCommand())
        {
            command.Connection = connection;
            command.CommandText = "DELETE FROM " + tableName + " WHERE ID = " + id.ToString();
            command.ExecuteNonQuery();
        }

        DBCloseConnection();
    }
}

/// <summary>
/// CLass used in PseudoORM to handle column->value data
/// </summary>
public class DBQueryElement
{
    public DBQueryElement(string n, object v)
    {
        queryName = n;
        queryValue = v;
    }

    public string queryName;
    public object queryValue;
}
