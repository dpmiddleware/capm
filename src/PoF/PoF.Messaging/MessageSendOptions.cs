namespace PoF.Messaging
{
    public struct MessageSendOptions
    {
        /// <summary>
        /// The number of seconds the messaging system should wait before the earliest
        /// time that the message can be made available to consumers of the message.
        /// </summary>
        public int? MessageSendDelayInSeconds { get; set; }
        /// <summary>
        /// The number of seconds the messaging system should keep the message before
        /// removing it automatically.
        /// </summary>
        public int? MessageTimeToLiveInSeconds { get; set; }
    }
}