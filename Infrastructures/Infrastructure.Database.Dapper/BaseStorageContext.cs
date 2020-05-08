namespace Infrastructure.Database.Dapper
{
    public class BaseStorageContext
    {
        //private readonly IDbTransaction _dbTransaction;
        private readonly string _strConnection;

        public BaseStorageContext(string strConnection)
        {
            _strConnection = strConnection;
        }

        public string GetConnectionString()
        {
            return _strConnection;
        }
    }
}
