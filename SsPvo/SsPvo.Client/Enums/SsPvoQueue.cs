namespace SsPvo.Client.Enums
{
    public enum SsPvoQueue
    {
        /// <summary>
        /// Очередь, содержащая результаты обработки сообщений (данных), загруженных в сервис (ЕПГУ) приема ИС ООВО
        /// </summary>
        Service,
        /// <summary>
        /// Очередь сообщения из ЕПГУ для ИС ООВО
        /// </summary>
        Epgu
    }
}
