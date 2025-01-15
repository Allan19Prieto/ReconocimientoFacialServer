using System.Data.SQLite;

namespace ReconocimientoFacialServer.Data
{
    public class DatabaseHandler
    {
        private readonly string _connectionString;

        public DatabaseHandler()
        {
            string databasePath = "data.db";
            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);
            }

            _connectionString = $"Data Source={databasePath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string createUsersTableQuery = @"
                CREATE TABLE IF NOT EXISTS Users (
                    UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    RegisteredDate DATETIME DEFAULT CURRENT_TIMESTAMP
                );";

            string createUserImagesTableQuery = @"
                CREATE TABLE IF NOT EXISTS UserImages (
                    ImageId INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    Image BLOB NOT NULL,
                    LightingCondition TEXT NOT NULL,
                    Expression TEXT NOT NULL,
                    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
                );";

            using var usersCommand = new SQLiteCommand(createUsersTableQuery, connection);
            usersCommand.ExecuteNonQuery();

            using var imagesCommand = new SQLiteCommand(createUserImagesTableQuery, connection);
            imagesCommand.ExecuteNonQuery();

        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }
    }
}
