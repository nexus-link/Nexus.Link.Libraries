using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Nexus.Link.Libraries.SqlServer.Logic
{
    public class SqlConstants
    {
        /// <summary>
        /// Constants for <see cref="SqlException.Number"/>
        /// </summary>
        /// <remarks>
        /// The comments have been taken from 
        /// SELECT * FROM master.dbo.sysmessages WHERE msglangid = 1033 AND error = nnn
        /// where nnn is the error number.
        /// </remarks>
        public enum SqlErrorEnum
        {
            /// <summary>
            /// The %ls statement conflicted with the %ls constraint "%.*ls". The conflict occurred in database "%.*ls", table "%.*ls"%ls%.*ls%ls.
            /// </summary>
            ConstraintFailed = 547,
            /// <summary>
            /// The attempted insert or update failed because the target view either specifies WITH CHECK OPTION or spans a
            /// view that specifies WITH CHECK OPTION and one or more rows resulting from the operation did not qualify under
            /// the CHECK OPTION constraint.
            /// </summary>
            CheckConstraintFailed = 550,

            /// <summary>
            /// Cannot insert duplicate key row in object '%.*ls' with unique index '%.*ls'. The duplicate key value is %ls.
            /// </summary>
            DuplicateKey = 2601,

            /// <summary>
            /// Violation of %ls constraint '%.*ls'. Cannot insert duplicate key in object '%.*ls'. The duplicate key value is %ls.
            /// </summary>
            UniqueConstraint = 2627,

        }
    }
}
