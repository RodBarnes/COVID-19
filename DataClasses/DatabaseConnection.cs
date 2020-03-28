using System;
using System.Data.SqlClient;
using System.IO;

namespace DataClasses
{
    public class DatabaseConnection : IDisposable
    {
        private readonly SqlConnection sqlConn;

        public DatabaseConnection()
        {
            var connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB; AttachDbFilename=D:\Source\BitBucket\COVID-19\WpfViewer\COVID-19.mdf; " +
                    "Integrated Security=True; MultipleActiveResultSets=True";
            sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();
        }

        void IDisposable.Dispose() => sqlConn.Close();

        public void ClearDataAll()
        {
            var filePath = @"D:\Source\BitBucket\COVID-19\Clear all data.sql";
            using (var file = new StreamReader(filePath))
            {
                var cmd = new SqlCommand
                {
                    Connection = sqlConn
                };

                while (!file.EndOfStream)
                {
                    var line = file.ReadLine();
                    if (line != "" && line != "GO")
                    {
                        cmd.CommandText = line;
                        _ = cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void ClearDataFromDate(DateTime? lastImportDate)
        {
            int rows;

            var sql = $"DELETE FROM DailyReport WHERE FileDate >= @lastImportDate";
            var cmd = new SqlCommand(sql, sqlConn);
            cmd.Parameters.AddWithValue("@lastImportDate", lastImportDate);
            rows = cmd.ExecuteNonQuery();
            if (rows < 0)
                throw new Exception($"ClearDataFromDate failed: LastImportDate='{lastImportDate}'\nsql={cmd.CommandText}.");
        }

        public void CountryInsert(string country, DateTime fileDate)
        {
            try
            {
                // Check name tables
                var countryRegionId = CountryRegionManage(country);

                // Insert Country-only DailyReport
                int rows;
 
                var sql = "INSERT INTO DailyReport(CountryRegionId, StateProvinceId, CountyDistrictId, FileDate, LastUpdate) " +
                    "VALUES (@countryRegionId, 1, 1, @fileDate, @fileDate)";
                var cmd = new SqlCommand(sql, sqlConn);
                cmd.Parameters.AddWithValue("@countryRegionId", countryRegionId);
                cmd.Parameters.AddWithValue("@fileDate", fileDate);
                //foreach (SqlParameter param in cmd.Parameters)
                //    System.Diagnostics.Debug.WriteLine($"name={param.ParameterName}, type={param.SqlDbType}, value={param.Value}");
                rows = cmd.ExecuteNonQuery();
                if (rows < 0)
                    throw new Exception($"CountryInsert failed: Report='{country}'\nsql={cmd.CommandText}.");
            }
            catch (Exception ex)
            {
                throw new Exception($"CountryInsert failed: Report='{country}'.", ex);
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
                
                int rows;
                var sql = "INSERT INTO DailyReport(CountryRegionId, StateProvinceId, CountyDistrictId, " +
                    "FileDate, lastUpdate, TotalConfirmed, TotalRecovered, TotalDeaths, TotalActive, " +
                    "NewConfirmed, NewRecovered, NewDeaths, NewActive, Latitude, Longitude, FIPS) " +
                    "VALUES (@countryRegionId, @stateProvinceId, @countyDistrictId, @fileDate, @lastUpdate, " +
                    "@totalConfirmed, @totalRecovered, @totalDeaths, @totalActive, " +
                    "@newConfirmed, @newRecovered, @newDeaths, @newActive, @latitude, @longitude, @fips)";
                var cmd = new SqlCommand(sql, sqlConn);
                cmd.Parameters.AddWithValue("@countryRegionId", countryRegionId);
                cmd.Parameters.AddWithValue("@stateProvinceId", stateProvinceId);
                cmd.Parameters.AddWithValue("@countyDistrictId", countyDistrictId);
                cmd.Parameters.AddWithValue("@fileDate", report.FileDate);
                cmd.Parameters.AddWithValue("@lastUpdate", report.LastUpdate);
                cmd.Parameters.AddWithValue("@totalConfirmed", report.TotalConfirmed);
                cmd.Parameters.AddWithValue("@totalRecovered", report.TotalRecovered);
                cmd.Parameters.AddWithValue("@totalDeaths", report.TotalDeaths);
                cmd.Parameters.AddWithValue("@totalActive", report.TotalActive);
                cmd.Parameters.AddWithValue("@newConfirmed", report.NewConfirmed);
                cmd.Parameters.AddWithValue("@newRecovered", report.NewRecovered);
                cmd.Parameters.AddWithValue("@newDeaths", report.NewDeaths);
                cmd.Parameters.AddWithValue("@newActive", report.NewActive);
                cmd.Parameters.AddWithValue("@latitude", report.Latitude);
                cmd.Parameters.AddWithValue("@longitude", report.Longitude);
                cmd.Parameters.AddWithValue("@fips", report.FIPS);
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
                object obj;
                var sql = $"UPDATE DailyReport SET TotalConfirmed=@totalConfirmed, TotalRecovered=@totalRecovered, TotalDeaths=@totalDeaths, " +
                    $"TotalActive=@totalActive, NewConfirmed=@newConfirmed, NewRecovered=@newRecovered, NewDeaths=@newDeaths, NewActive=@newActive, " +
                    "Latitude=@latitude, Longitude=@longitude, FIPS=@fips " +
                    "FROM DailyReport dr " +
                    $"JOIN CountryRegion cr ON cr.[Name] = @countryRegion " +
                    $"JOIN StateProvince sp ON sp.[Name] = @stateProvince " +
                    $"JOIN CountyDistrict cd on cd.[Name] = @countyDistrict";
                var cmd = new SqlCommand(sql, sqlConn);
                cmd.Parameters.AddWithValue("@countryRegion", SqlStringWrite(report.Country));
                cmd.Parameters.AddWithValue("@stateProvince", SqlStringWrite(report.State));
                cmd.Parameters.AddWithValue("@countyDistrict", SqlStringWrite(report.County));
                cmd.Parameters.AddWithValue("@fileDate", report.FileDate);
                cmd.Parameters.AddWithValue("@confirmed", report.TotalConfirmed);
                cmd.Parameters.AddWithValue("@recovered", report.TotalRecovered);
                cmd.Parameters.AddWithValue("@totalDeaths", report.TotalDeaths);
                cmd.Parameters.AddWithValue("@totalActive", report.TotalActive);
                cmd.Parameters.AddWithValue("@newConfirmed", report.NewConfirmed);
                cmd.Parameters.AddWithValue("@newRecovered", report.NewRecovered);
                cmd.Parameters.AddWithValue("@newDeaths", report.NewDeaths);
                cmd.Parameters.AddWithValue("@newActive", report.NewActive);
                cmd.Parameters.AddWithValue("@latitude", report.Latitude);
                cmd.Parameters.AddWithValue("@longitude", report.Longitude);
                cmd.Parameters.AddWithValue("@fips", report.FIPS);
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

        public bool ReportExists(string countryRegion, string stateProvince, string countyDistrict, DateTime fileDate)
        {
            bool exists;

            try
            {
                object obj;

                var sql = $"SELECT COUNT(*) as Count " +
                    $"FROM DailyReport dr " +
                    "JOIN CountryRegion cr ON cr.CountryRegionId = dr.countryRegionId " +
                    "JOIN StateProvince sp ON sp.StateProvinceId = dr.StateProvinceId " +
                    "JOIN CountyDistrict cd ON cd.CountyDistrictId = dr.CountyDistrictId " +
                    "WHERE cr.[Name]=@countryRegion AND sp.[Name]=@stateProvince AND cd.[Name]=@countyDistrict " +
                    "AND dr.FileDate=@fileDate";
                var cmd = new SqlCommand(sql, sqlConn);
                cmd.Parameters.AddWithValue("@countryRegion", SqlStringWrite(countryRegion));
                cmd.Parameters.AddWithValue("@stateProvince", SqlStringWrite(stateProvince));
                cmd.Parameters.AddWithValue("@countyDistrict", SqlStringWrite(countyDistrict));
                cmd.Parameters.AddWithValue("@fileDate", fileDate);
                //foreach (SqlParameter param in cmd.Parameters)
                //    System.Diagnostics.Debug.WriteLine($"name={param.ParameterName}, type={param.SqlDbType}, value={param.Value}");
                obj = cmd.ExecuteScalar();
                exists = (obj != null && ((int)obj) > 0);
            }
            catch (Exception ex)
            {
                throw new Exception($"ReportExists failed.", ex);
            }

            return exists;
        }

        public DailyReport ReportRead(string countryRegion, string stateProvince, string countyDistrict, DateTime fileDate)
        {
            DailyReport report = null;

            try
            {
                var sql = $"SELECT cr.[Name] as CountryRegion, sp.[Name] as StateProvince, cd.[Name] as CountyDistrict, " +
                    "dr.FileDate, dr.LastUpdate, dr.TotalConfirmed, dr.TotalRecovered, dr.TotalDeaths, " +
                    "dr.TotalActive, dr.NewConfirmed, dr.NewRecovered, dr.NewDeaths, dr.NewActive, " +
                    "dr.Latitude, dr.Longitude, dr.FIPS " +
                    $"FROM DailyReport dr " +
                    "JOIN CountryRegion cr ON cr.CountryRegionId=dr.CountryRegionId " +
                    "JOIN StateProvince sp ON sp.StateProvinceId=dr.StateProvinceId " +
                    "JOIN CountyDistrict cd ON cd.CountyDistrictId=dr.CountyDistrictId " +
                    "WHERE cr.[Name]=@countryRegion AND sp.[Name]=@stateProvince " +
                    "AND cd.[Name]=@countyDistrict AND dr.FileDate=@fileDate";
                var cmd = new SqlCommand(sql, sqlConn);
                cmd.Parameters.AddWithValue("@countryRegion", SqlStringWrite(countryRegion));
                cmd.Parameters.AddWithValue("@stateProvince", SqlStringWrite(stateProvince));
                cmd.Parameters.AddWithValue("@countyDistrict", SqlStringWrite(countyDistrict));
                cmd.Parameters.AddWithValue("@fileDate", fileDate);
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
                        var lastUpdate = DateTime.Parse(reader["LastUpdate"].ToString());
                        var totalConfirmed = (int)reader["TotalConfirmed"];
                        var totalRecovered = (int)reader["TotalRecovered"];
                        var totalDeaths = (int)reader["TotalDeaths"];
                        var totalActive = (int)reader["TotalActive"];
                        var newConfirmed = (int)reader["NewConfirmed"];
                        var newRecovered = (int)reader["NewRecovered"];
                        var newDeaths = (int)reader["NewDeaths"];
                        var newActive = (int)reader["NewActive"];
                        var latitude = (double)reader["Latitude"];
                        var longitude = (double)reader["Longitude"];
                        var fips = (int)reader["FIPS"];
                        report = new DailyReport(fileDate, countryRegion, stateProvince, countyDistrict, lastUpdate, 
                            totalConfirmed, totalRecovered, totalDeaths, totalActive, 
                            newConfirmed, newRecovered, newDeaths, newActive, latitude, longitude, fips);
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

        public DailyReport ReportReadPrevious(string countryRegion, string stateProvince, string countyDistrict, DateTime fileDate)
        {
            DailyReport report = null;

            try
            {
                var sql = $"SELECT cr.[Name] as CountryRegion, sp.[Name] as StateProvince, cd.[Name] as CountyDistrict, " +
                    "dr.FileDate, dr.LastUpdate, dr.TotalConfirmed, dr.TotalRecovered, dr.TotalDeaths, " +
                    "dr.TotalActive, dr.NewConfirmed, dr.NewRecovered, dr.NewDeaths, dr.NewActive, " +
                    "dr.Latitude, dr.Longitude, dr.FIPS " +
                    $"FROM DailyReport dr " +
                    "JOIN CountryRegion cr ON cr.CountryRegionId=dr.CountryRegionId " +
                    "JOIN StateProvince sp ON sp.StateProvinceId=dr.StateProvinceId " +
                    "JOIN CountyDistrict cd ON cd.CountyDistrictId=dr.CountyDistrictId " +
                    "WHERE cr.[Name]=@countryRegion AND sp.[Name]=@stateProvince " +
                    "AND cd.[Name]=@countyDistrict AND dr.FileDate<=@fileDate " +
                    "ORDER BY dr.FileDate DESC";
                var cmd = new SqlCommand(sql, sqlConn);
                cmd.Parameters.AddWithValue("@countryRegion", SqlStringWrite(countryRegion));
                cmd.Parameters.AddWithValue("@stateProvince", SqlStringWrite(stateProvince));
                cmd.Parameters.AddWithValue("@countyDistrict", SqlStringWrite(countyDistrict));
                cmd.Parameters.AddWithValue("@fileDate", fileDate);
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
                        var lastUpdate = DateTime.Parse(reader["LastUpdate"].ToString());
                        var totalConfirmed = (int)reader["TotalConfirmed"];
                        var totalRecovered = (int)reader["TotalRecovered"];
                        var totalDeaths = (int)reader["TotalDeaths"];
                        var totalActive = (int)reader["TotalActive"];
                        var newConfirmed = (int)reader["NewConfirmed"];
                        var newRecovered = (int)reader["NewRecovered"];
                        var newDeaths = (int)reader["NewDeaths"];
                        var newActive = (int)reader["NewActive"];
                        var latitude = (double)reader["Latitude"];
                        var longitude = (double)reader["Longitude"];
                        var fips = (int)reader["FIPS"];
                        report = new DailyReport(fileDate, countryRegion, stateProvince, countyDistrict, lastUpdate,
                            totalConfirmed, totalRecovered, totalDeaths, totalActive, 
                            newConfirmed, newRecovered, newDeaths, newActive, latitude, longitude, fips);
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
                var sql = $"SELECT cr.[Name] as CountryRegion, sp.[Name] as StateProvince, cd.[Name] as CountyDistrict, " +
                    "dr.FileDate, dr.LastUpdate, dr.TotalConfirmed, dr.TotalRecovered, dr.TotalDeaths, dr.TotalActive, " +
                    "dr.NewConfirmed, dr.NewRecovered, dr.NewDeaths, dr.NewActive, dr.Latitude, dr.Longitude, dr.fips " +
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
                        var countryRegion = SqlStringRead(reader["CountryRegion"].ToString());
                        var stateProvince = SqlStringRead(reader["StateProvince"].ToString());
                        var countyDistrict = SqlStringRead(reader["CountyDistrict"].ToString());
                        var fileDate = DateTime.Parse(reader["FileDate"].ToString());
                        var lastUpdate = DateTime.Parse(reader["LastUpdate"].ToString());
                        var totalConfirmed = (int)reader["TotalConfirmed"];
                        var totalRecovered = (int)reader["TotalRecovered"];
                        var totalDeaths = (int)reader["TotalDeaths"];
                        var totalActive = (int)reader["TotalActive"];
                        var newConfirmed = (int)reader["NewConfirmed"];
                        var newRecovered = (int)reader["NewRecovered"];
                        var newDeaths = (int)reader["NewDeaths"];
                        var newActive = (int)reader["NewActive"];
                        var latitude = (double)reader["Latitude"];
                        var longitude = (double)reader["Longitude"];
                        var fips = (int)reader["FIPS"];
                        var item = new DailyReport(fileDate, countryRegion, stateProvince, countyDistrict, lastUpdate,
                            totalConfirmed, totalRecovered, totalDeaths, totalActive, 
                            newConfirmed, newRecovered, newDeaths, newActive, latitude, longitude, fips);
                        reports.Add(item);
                    }
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"ReportsRead failed.", ex);
            }
        }

        private int CountryRegionManage(string name)
        {
            int id;

            try
            {
                object obj;
                var sql = "SELECT CountryRegionId FROM CountryRegion WHERE [Name]=@name";
                var cmd = new SqlCommand(sql, sqlConn);
                cmd.Parameters.AddWithValue("@name", SqlStringWrite(name));
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
                    cmd.Parameters.AddWithValue("@name", SqlStringWrite(name));
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
                cmd.Parameters.AddWithValue("@name", SqlStringWrite(name));
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
                    cmd.Parameters.AddWithValue("@name", SqlStringWrite(name));
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
                cmd.Parameters.AddWithValue("@name", SqlStringWrite(name));
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
                    cmd.Parameters.AddWithValue("@name", SqlStringWrite(name));
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

        private static string SqlStringWrite(string text) => string.IsNullOrEmpty(text) ? "" : text.Replace("'", "''");

        private static string SqlStringRead(string text) => string.IsNullOrEmpty(text) ? "" : text.Replace("''", "'");

        #endregion
    }
}
