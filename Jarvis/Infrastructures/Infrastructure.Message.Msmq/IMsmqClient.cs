namespace Infrastructure.Message.Msmq
{
    public interface IMsmqClient
    {
        /// <summary>
        /// Thêm 1 message vào queue
        /// </summary>
        /// <param name="name"></param>
        /// <param name="message"></param>
        void Send(string name, string message);

        /// <summary>
        /// Xóa toàn bộ message trong queue
        /// </summary>
        /// <param name="name"></param>
        void Purge(string name);
    }
}
