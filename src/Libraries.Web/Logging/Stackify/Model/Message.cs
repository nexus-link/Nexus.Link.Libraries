namespace Nexus.Link.Libraries.Web.Logging.Stackify.Model
{
    internal class Message
    {
        #region Mandatory properties
        public string Msg { get; set; }
        // ReSharper disable once InconsistentNaming
        public double EpochMS { get; set; }
        #endregion
        public string Level { get; set; }
        public string Data { get; set; }
        public string SourceMethod { get; set; }
        public Exception Ex { get; set; }
        public string TransId { get; set; }
    }
}