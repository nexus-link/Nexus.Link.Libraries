using System.Collections.Generic;

namespace Nexus.Link.Libraries.Web.Logging.Stackify.Model
{
    internal class Envelope
    {
        #region Mandatory properties
        public string Env { get; set; }
        public string ServerName { get; set; }
        public string AppName { get; set; }
        public string Logger { get; set; }
        #endregion

        // ReSharper disable once IdentifierTypo
        public List<Message> Msgs { get; set; }
        public string Platform { get; set; }
    }
}