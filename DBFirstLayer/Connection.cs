using MySql.Data.MySqlClient;
using Npgsql;

namespace DBFirstLayer
{
    public class Connection
    {
        //"Host=localhost;Database=soforiste;Username=postgres;Password=123" "Host=159.253.36.245;Database=postgres;Username=postgres;Password=Ali.1453.Arge"
        public static readonly string NpgSqlConnection = "Host=159.253.36.245;Database=postgres;Username=postgres;Password=Ali.1453.Arge";
        private Connection() { }
        private static readonly object Instancelock = new object();
        private static Connection connection = null;

        public static Connection GetConnection
        {
            get
            {
                if (connection == null)
                {
                    lock (Instancelock)
                    {
                        if (connection == null)
                        {
                            connection = new Connection();
                        }
                    }
                }
                return connection;
            }
        }

        public NpgsqlConnection Connect()
        {
            return new NpgsqlConnection(NpgSqlConnection);
        }
    }
}