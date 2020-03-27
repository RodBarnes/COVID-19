using System;
using System.Data.SqlClient;
using System.IO;

namespace DataClasses
{
    public class DatabaseConnection : IDisposable
    {
        private SqlConnection sqlConn;

        public DatabaseConnection()
        {
            var connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB; AttachDbFilename=D:\Source\BitBucket\COVID-19\WpfViewer\COVID-19.mdf; " +
                    "Integrated Security=True; MultipleActiveResultSets=True";
            sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();
        }

        void IDisposable.Dispose() => sqlConn.Close();

        public void ClearData()
        {
            var filePath = @"D:\Source\BitBucket\COVID-19\Clear all data.sql";
            using (var file = new StreamReader(filePath))
            {
                int rows;
                var cmd = new SqlCommand();
                cmd.Connection = sqlConn;

                while (!file.EndOfStream)
                {
                    var line = file.ReadLine();
                    if (line != "" && line != "GO")
                    {
                        cmd.CommandText = line;
                        rows = cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void ReportInsert(DailyReport report)
        {
            try
            {
                // Check name tables
                var countryRegionId = CountryRegionManage(report.Country);
                var stateProvinceId = StateProvinceManage(report.State);
                var countyDistrictId = CountyDistrictManage(report.County);
                
                // Insert DailyReport
                int rows;
                var sql = "INSERT INTO DailyReport(CountryRegionId, StateProvinceId, CountyDistrictId, " +
                    "RecordDate, TotalConfirmed, TotalRecovered, TotalDeaths, TotalActive, Latitude, Longitude, " +
                    "NewConfirmed, NewRecovered, NewDeaths) " +
                    "VALUES (@countryRegionId, @stateProvinceId, @countyDistrictId, @recordDate, " +
                    "@totalConfirmed, @totalRecovered, @totalDeaths, @totalActive, @latitude, @longitude, @newConfirmed, " +
                    "@newRecovered, @newDeaths)";
                var cmd = new SqlCommand(sql, sqlConn);
                cmd.Parameters.AddWithValue("@countryRegionId", countryRegionId);
                cmd.Parameters.AddWithValue("@stateProvinceId", stateProvinceId);
                cmd.Parameters.AddWithValue("@countyDistrictId", countyDistrictId);
                cmd.Parameters.AddWithValue("@recordDate", report.RecordDate);
                cmd.Parameters.AddWithValue("@totalConfirmed", report.TotalConfirmed);
                cmd.Parameters.AddWithValue("@totalRecovered", report.TotalRecovered);
                cmd.Parameters.AddWithValue("@totalDeaths", report.TotalDeaths);
                cmd.Parameters.AddWithValue("@totalActive", report.TotalActive);
                cmd.Parameters.AddWithValue("@latitude", report.Latitude);
                cmd.Parameters.AddWithValue("@longitude", report.Longitude);
                cmd.Parameters.AddWithValue("@newConfirmed", report.NewConfirmed);
                cmd.Parameters.AddWithValue("@newRecovered", report.NewRecovered);
                cmd.Parameters.AddWithValue("@newDeaths", report.NewDeaths);
                //foreach (SqlParameter param in cmd.Parameters)
                //    System.Diagnostics.Debug.WriteLine($"name={param.ParameterName}, type={param.SqlDbType}, value={param.Value}");
                rows = cmd.ExecuteNonQuery();
                if (rows < 0)
                    throw new Exception($"ReportInsert failed: Report='{report.Country},{report.State},{report.County}'\nsql={cmd.CommandText}.");
            }
            catch (Exception ex)
            {
                throw new Exception($"ReportInsert failed: Report='{report.Country},{report.State},{report.County}'.", ex);
            }
        }

        public void ReportUpdate(DailyReport report)
        {
            try
            {
                // Update DailyReport
                object obj;
                var sql = $"UPDATE DailyReport SET TotalConfirmed=@totalConfirmed, TotalRecovered=@totalRecovered, TotalDeaths=@totalDeaths, " +
                    $"TotalActive=@totalActive, NewConfirmed=@newConfirmed, NewRecovered=@newRecovered, NewDeaths=@newDeaths " +
                    "Latitude=@latitude, Longitude=@longitude " +
                    "FROM DailyReport dr " +
                    $"JOIN CountryRegion cr ON cr.[Name] = @countryRegion " +
                    $"JOIN StateProvince sp ON sp.[Name] = @stateProvince " +
                    $"JOIN CountyDistrict cd on cd.[Name] = @countyDistrict";
                var cmd = new SqlCommand(sql, sqlConn);
                cmd.Parameters.AddWithValue("@countryRegion", report.Country);
                cmd.Parameters.AddWithValue("@stateProvince", report.State);
                cmd.Parameters.AddWithValue("@countyDistrict", report.County);
                cmd.Parameters.AddWithValue("@recordDate", report.RecordDate);
                cmd.Parameters.AddWithValue("@confirmed", report.TotalConfirmed);
                cmd.Parameters.AddWithValue("@recovered", report.TotalRecovered);
                cmd.Parameters.AddWithValue("@totalDeaths", report.TotalDeaths);
                cmd.Parameters.AddWithValue("@totalActive", report.TotalActive);
                cmd.Parameters.AddWithValue("@latitude", report.Latitude);
                cmd.Parameters.AddWithValue("@longitude", report.Longitude);
                cmd.Parameters.AddWithValue("@newConfirmed", report.NewConfirmed);
                cmd.Parameters.AddWithValue("@newRecovered", report.NewRecovered);
                cmd.Parameters.AddWithValue("@newDeaths", report.NewDeaths);
                //foreach (SqlParameter param in cmd.Parameters)
                //    System.Diagnostics.Debug.WriteLine($"name={param.ParameterName}, type={param.SqlDbType}, value={param.Value}");
                obj = cmd.ExecuteScalar();
                if (obj == null)
                    throw new Exception($"ReportUpdate failed: Report='{report.Country},{report.State},{report.County}'\nsql={cmd.CommandText}.");
            }
            catch (Exception ex)
            {
                throw new Exception($"ReportUpdate failed: Report='{report.Country},{report.State},{report.County}'.", ex);
            }
        }

        public bool ReportExists(string countryRegion, string stateProvince, string countyDistrict, DateTime recordDate)
        {
            bool exists;

            try
            {
                // Find DailyReport
                object obj;

                var sql = $"SELECT cr.[Name] as CountryRegion, sp.[Name] as StateProvince, cd.[Name] as CountyDistrict, " +
                    "dr.RecordDate " +
                    $"FROM DailyReport dr " +
                    "JOIN CountryRegion cr ON cr.CountryRegionId = dr.countryRegionId " +
                    "JOIN StateProvince sp ON sp.StateProvinceId = dr.StateProvinceId " +
                    "JOIN CountyDistrict cd ON cd.CountyDistrictId = dr.CountyDistrictId " +
                    "WHERE cr.[Name]=@countryRegion AND sp.[Name]=@stateProvince " +
                    "AND cd.[Name]=@countyDistrict AND dr.RecordDate=@recordDate";
                var cmd = new SqlCommand(sql, sqlConn);
                cmd.Parameters.AddWithValue("@countryRegion", countryRegion);
                cmd.Parameters.AddWithValue("@stateProvince", stateProvince);
                cmd.Parameters.AddWithValue("@countyDistrict", countyDistrict);
                cmd.Parameters.AddWithValue("@recordDate", recordDate);
                //foreach (SqlParameter param in cmd.Parameters)
                //    System.Diagnostics.Debug.WriteLine($"name={param.ParameterName}, type={param.SqlDbType}, value={param.Value}");
                obj = cmd.ExecuteScalar();
                exists = (obj != null);
            }
            catch (Exception ex)
            {
                throw new Exception($"ReportExists failed.", ex);
            }

            return exists;
        }

        public DailyReport ReportRead(string countryRegion, string stateProvince, string countyDistrict, DateTime recordDate)
        {
            DailyReport report = null;

            try
            {
                // Find DailyReport
                var sql = $"SELECT cr.[Name] as CountryRegion, sp.[Name] as StateProvince, cd.[Name] as CountyDistrict, " +
                    "dr.RecordDate, dr.TotalConfirmed, dr.TotalRecovered, dr.TotalDeaths, " +
                    "dr.NewConfirmed, dr.NewRecovered, dr.NewDeaths, dr.TotalActive, " +
                    "dr.Latitude, dr.Longitude " +
                    $"FROM DailyReport dr " +
                    "JOIN CountryRegion cr ON cr.CountryRegionId=dr.CountryRegionId " +
                    "JOIN StateProvince sp ON sp.StateProvinceId=dr.StateProvinceId " +
                    "JOIN CountyDistrict cd ON cd.CountyDistrictId=dr.CountyDistrictId " +
                    "WHERE cr.[Name]=@countryRegion AND sp.[Name]=@stateProvince " +
                    "AND cd.[Name]=@countyDistrict AND dr.RecordDate=@recordDate";
                var cmd = new SqlCommand(sql, sqlConn);
                cmd.Parameters.AddWithValue("@countryRegion", countryRegion);
                cmd.Parameters.AddWithValue("@stateProvince", stateProvince);
                cmd.Parameters.AddWithValue("@countyDistrict", countyDistrict);
                cmd.Parameters.AddWithValue("@recordDate", recordDate);
                //foreach (SqlParameter param in cmd.Parameters)
                //    System.Diagnostics.Debug.WriteLine($"name={param.ParameterName}, type={param.SqlDbType}, value={param.Value}");
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader == null)
                {
                    throw new Exception($"ReportsRead failed: reader={reader}\nsql={cmd.CommandText}.");
                }
                else
                {
                    while (reader.Read())
                    {
                        var totalConfirmed = (int)reader["TotalConfirmed"];
                        var totalRecovered = (int)reader["TotalRecovered"];
                        var totalDeaths = (int)reader["TotalDeaths"];
                        var newConfirmed = (int)reader["NewConfirmed"];
                        var newRecovered = (int)reader["NewRecovered"];
                        var newDeaths = (int)reader["NewDeaths"];
                        var totalActive = (int)reader["TotalActive"];
                        var latitude = (double)reader["Latitude"];
                        var longitude = (double)reader["Longitude"];
                        report = new DailyReport(countryRegion, stateProvince, countyDistrict, recordDate, 
                            totalConfirmed, totalRecovered, totalDeaths, newConfirmed, newRecovered, newDeaths,
                            totalActive, latitude, longitude);
                    }
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"ReportExists failed.", ex);
            }

            return report;
        }

        public DailyReport ReportReadPrevious(string countryRegion, string stateProvince, string countyDistrict, DateTime recordDate)
        {
            DailyReport report = null;

            try
            {
                // Find previous DailyReport
                var sql = $"SELECT cr.[Name] as CountryRegion, sp.[Name] as StateProvince, cd.[Name] as CountyDistrict, " +
                    "dr.RecordDate, dr.TotalConfirmed, dr.TotalRecovered, dr.TotalDeaths, " +
                    "dr.NewConfirmed, dr.NewRecovered, dr.NewDeaths, dr.TotalActive, " +
                    "dr.Latitude, dr.Longitude " +
                    $"FROM DailyReport dr " +
                    "JOIN CountryRegion cr ON cr.CountryRegionId=dr.CountryRegionId " +
                    "JOIN StateProvince sp ON sp.StateProvinceId=dr.StateProvinceId " +
                    "JOIN CountyDistrict cd ON cd.CountyDistrictId=dr.CountyDistrictId " +
                    "WHERE cr.[Name]=@countryRegion AND sp.[Name]=@stateProvince " +
                    "AND cd.[Name]=@countyDistrict AND dr.RecordDate<=@recordDate " +
                    "ORDER BY dr.RecordDate DESC";
                var cmd = new SqlCommand(sql, sqlConn);
                cmd.Parameters.AddWithValue("@countryRegion", countryRegion);
                cmd.Parameters.AddWithValue("@stateProvince", stateProvince);
                cmd.Parameters.AddWithValue("@countyDistrict", countyDistrict);
                cmd.Parameters.AddWithValue("@recordDate", recordDate);
                //foreach (SqlParameter param in cmd.Parameters)
                //    System.Diagnostics.Debug.WriteLine($"name={param.ParameterName}, type={param.SqlDbType}, value={param.Value}");
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader == null)
                {
                    throw new Exception($"ReportReadPrevious failed: reader={reader}\nsql={cmd.CommandText}.");
                }
                else
                {
                    while (reader.Read())
                    {
                        var totalConfirmed = (int)reader["TotalConfirmed"];
                        var totalRecovered = (int)reader["TotalRecovered"];
                        var totalDeaths = (int)reader["TotalDeaths"];
                        var newConfirmed = (int)reader["NewConfirmed"];
                        var newRecovered = (int)reader["NewRecovered"];
                        var newDeaths = (int)reader["NewDeaths"];
                        var totalActive = (int)reader["TotalActive"];
                        var latitude = (double)reader["Latitude"];
                        var longitude = (double)reader["Longitude"];
                        report = new DailyReport(countryRegion, stateProvince, countyDistrict, recordDate,
                            totalConfirmed, totalRecovered, totalDeaths, newConfirmed, newRecovered, newDeaths,
                            totalActive, latitude, longitude);
                        break;  // We only care about the first one
                    }
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"ReportReadPrevious failed.", ex);
            }

            return report;
        }


        public void ReportsRead(DailyReports reports)
        {
            try
            {
                // Add DailyReport
                var sql = $"SELECT cr.[Name] as CountryRegion, sp.[Name] as StateProvince, cd.[Name] as CountyDistrict, " +
                    "dr.RecordDate, dr.TotalConfirmed, dr.TotalRecovered, dr.TotalDeaths, dr.TotalActive, dr.Latitude, dr.Longitude, " +
                    "dr.NewConfirmed, dr.NewRecovered, dr.NewDeaths " +
                    $"FROM DailyReport dr " +
                    "JOIN CountryRegion cr ON cr.CountryRegionId=dr.CountryRegionId " +
                    "JOIN StateProvince sp ON sp.StateProvinceId=dr.StateProvinceId " +
                    "JOIN CountyDistrict cd ON cd.CountyDistrictId=dr.CountyDistrictId";
                var cmd = new SqlCommand(sql, sqlConn);
                //foreach (SqlParameter param in cmd.Parameters)
                //    System.Diagnostics.Debug.WriteLine($"name={param.ParameterName}, type={param.SqlDbType}, value={param.Value}");
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader == null)
                {
                    throw new Exception($"ReportsRead failed: reader={reader}\nsql={cmd.CommandText}.");
                }
                else
                {
                    while (reader.Read())
                    {
                        var countryRegion = SqlReadString(reader["CountryRegion"].ToString());
                        var stateProvince = SqlReadString(reader["StateProvince"].ToString());
                        var countyDistrict = SqlReadString(reader["CountyDistrict"].ToString());
                        var recordDate = DateTime.Parse(reader["RecordDate"].ToString());
                        var totalConfirmed = (int)reader["TotalConfirmed"];
                        var totalRecovered = (int)reader["TotalRecovered"];
                        var totalDeaths = (int)reader["TotalDeaths"];
                        var newConfirmed = (int)reader["NewConfirmed"];
                        var newRecovered = (int)reader["NewRecovered"];
                        var newDeaths = (int)reader["NewDeaths"];
                        var totalActive = (int)reader["TotalActive"];
                        var latitude = (double)reader["Latitude"];
                        var longitude = (double)reader["Longitude"];
                        var item = new DailyReport(countryRegion, stateProvince, countyDistrict, recordDate,
                            totalConfirmed, totalRecovered, totalDeaths, newConfirmed, newRecovered, newDeaths,
                            totalActive, latitude, longitude);
                        reports.Add(item);

                        //System.Diagnostics.Debug.WriteLine($"{item.RecordDate},{item.Country},{item.State},{item.County},{item.TotalRecovered}");
                    }
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"ReportsRead failed.", ex);
            }

            //foreach (var item in reports)
            //{
            //    System.Diagnostics.Debug.WriteLine($"{item.RecordDate},{item.Country},{item.State},{item.County},{item.TotalRecovered}");
            //}

        }

        private int CountryRegionManage(string name)
        {
            int id;

            try
            {
                object obj;
                var sql = "SELECT CountryRegionId FROM CountryRegion WHERE [Name]=@name";
                var cmd = new SqlCommand(sql, sqlConn);
                cmd.Parameters.AddWithValue("@name", SqlWriteString(name));
                //foreach (SqlParameter param in cmd.Parameters)
                //    System.Diagnostics.Debug.WriteLine($"name={param.ParameterName}, type={param.SqlDbType}, value={param.Value}");
                obj = cmd.ExecuteScalar();
                if (obj == null)
                {
                    // Doesn't exist; insert the new record
                    cmd.CommandText = "INSERT INTO CountryRegion ([Name]) OUTPUT Inserted.CountryRegionId VALUES (@name)";
                    //foreach (SqlParameter param in cmd.Parameters)
                    //    System.Diagnostics.Debug.WriteLine($"name={param.ParameterName}, type={param.SqlDbType}, value={param.Value}");
                    obj = cmd.ExecuteScalar();
                    if (obj != null)
                    {
                        id = (int)obj;
                    }
                    else
                        throw new Exception($"CountryRegionManage failed: Name={name}\nsql={cmd.CommandText}.");
                }
                else
                {
                    id = (int)obj;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"CountryRegionManage failed: Name={name}.", ex);
            }

            return id;
        }

        public int CountryRegionGetId(string name)
        {
            int id = 0;

            if (!string.IsNullOrEmpty(name))
            {
                try
                {
                    object obj;
                    var sql = "SELECT [Name] FROM CountryReion WHERE [Name]=@name";
                    var cmd = new SqlCommand(sql, sqlConn);
                    cmd.Parameters.AddWithValue("@name", name);
                    //foreach (SqlParameter param in cmd.Parameters)
                    //    System.Diagnostics.Debug.WriteLine($"name={param.ParameterName}, type={param.SqlDbType}, value={param.Value}");
                    obj = cmd.ExecuteScalar();
                    if (obj != null)
                        id = (int)obj;
                }
                catch (Exception ex)
                {
                    throw new Exception($"CountryRegionGetId failed: Name={name}.", ex);
                }
            }

            return id;
        }

        private int StateProvinceManage(string name)
        {
            int id;

            try
            {
                object obj;
                var sql = "SELECT StateProvinceId FROM StateProvince WHERE [Name]=@name";
                var cmd = new SqlCommand(sql, sqlConn);
                cmd.Parameters.AddWithValue("@name", SqlWriteString(name));
                //foreach (SqlParameter param in cmd.Parameters)
                //    System.Diagnostics.Debug.WriteLine($"name={param.ParameterName}, type={param.SqlDbType}, value={param.Value}");
                obj = cmd.ExecuteScalar();
                if (obj == null)
                {
                    // Doesn't exist; insert the new record
                    cmd.CommandText = "INSERT INTO StateProvince ([Name]) OUTPUT Inserted.StateProvinceId VALUES (@name)";
                    //foreach (SqlParameter param in cmd.Parameters)
                    //    System.Diagnostics.Debug.WriteLine($"name={param.ParameterName}, type={param.SqlDbType}, value={param.Value}");
                    obj = cmd.ExecuteScalar();
                    if (obj != null)
                    {
                        id = (int)obj;
                    }
                    else
                        throw new Exception($"StateProvinceManage failed: Name={name}\nsql={cmd.CommandText}.");
                }
                else
                {
                    id = (int)obj;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"StateProvinceManage failed: Name={name}.", ex);
            }

            return id;
        }

        public int StateProvinceGetId(string name)
        {
            int id = 0;

            if (!string.IsNullOrEmpty(name))
            {
                try
                {
                    object obj;
                    var sql = "SELECT [Name] FROM StateProvince WHERE [Name]=@name";
                    var cmd = new SqlCommand(sql, sqlConn);
                    cmd.Parameters.AddWithValue("@name", name);
                    //foreach (SqlParameter param in cmd.Parameters)
                    //    System.Diagnostics.Debug.WriteLine($"name={param.ParameterName}, type={param.SqlDbType}, value={param.Value}");
                    obj = cmd.ExecuteScalar();
                    if (obj != null)
                        id = (int)obj;
                }
                catch (Exception ex)
                {
                    throw new Exception($"StateProvinceGetId failed: Name={name}.", ex);
                }
            }

            return id;
        }

        private int CountyDistrictManage(string name)
        {
            int id;

            try
            {
                object obj;
                var sql = "SELECT CountyDistrictId FROM CountyDistrict WHERE [Name]=@name";
                var cmd = new SqlCommand(sql, sqlConn);
                cmd.Parameters.AddWithValue("@name", SqlWriteString(name));
                //foreach (SqlParameter param in cmd.Parameters)
                //    System.Diagnostics.Debug.WriteLine($"name={param.ParameterName}, type={param.SqlDbType}, value={param.Value}");
                obj = cmd.ExecuteScalar();
                if (obj == null)
                {
                    // Doesn't exist; insert the new record
                    cmd.CommandText = "INSERT INTO CountyDistrict ([Name]) OUTPUT Inserted.CountyDistrictId VALUES (@name)";
                    //foreach (SqlParameter param in cmd.Parameters)
                    //    System.Diagnostics.Debug.WriteLine($"name={param.ParameterName}, type={param.SqlDbType}, value={param.Value}");
                    obj = cmd.ExecuteScalar();
                    if (obj != null)
                    {
                        id = (int)obj;
                    }
                    else
                        throw new Exception($"CountyDistrictManage failed: Name={name}\nsql={cmd.CommandText}.");
                }
                else
                {
                    id = (int)obj;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"CountyDistrictManage failed: Name={name}.", ex);
            }

            return id;
        }

        public int CountyDistrictGetId(string name)
        {
            int id = 0;

            if (!string.IsNullOrEmpty(name))
            {
                try
                {
                    object obj;
                    var sql = "SELECT [Name] FROM CountyDistrict WHERE [Name]=@name";
                    var cmd = new SqlCommand(sql, sqlConn);
                    cmd.Parameters.AddWithValue("@name", name);
                    //foreach (SqlParameter param in cmd.Parameters)
                    //    System.Diagnostics.Debug.WriteLine($"name={param.ParameterName}, type={param.SqlDbType}, value={param.Value}");
                    obj = cmd.ExecuteScalar();
                    if (obj != null)
                        id = (int)obj;
                }
                catch (Exception ex)
                {
                    throw new Exception($"CountyDistrictGetId failed: Name={name}.", ex);
                }
            }

            return id;
        }

        #region Helpers

        private static string SqlWriteString(string text) => string.IsNullOrEmpty(text) ? "" : text.Replace("'", "''");

        private static string SqlReadString(string text) => string.IsNullOrEmpty(text) ? "" : text.Replace("''", "'");

        #endregion
    }
}
