using System.Data.SqlClient;

namespace DatabaseAccessApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SqlConnection dbConnection = new SqlConnection(@"Server=(localdb)\MSSQLLocalDB; Database=Employees; Trusted_Connection=True;");
            dbConnection.Open();
            using (dbConnection) {

                SqlCommand command = new SqlCommand("SELECT * FROM Employees",dbConnection);
                SqlDataReader reader = command.ExecuteReader();
                using (reader) {
                    while (reader.Read()) {
                        string name = reader["Name"].ToString();
                        string family = reader["Family"].ToString();
                        Console.WriteLine("{0} {1}",name,family);
                    }
                }
            }
        }
    }
}
