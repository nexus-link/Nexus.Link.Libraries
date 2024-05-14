using Microsoft.Data.SqlClient;

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

            /// <summary>
            /// Transaction (Process ID %d) was deadlocked on %.*ls resources with another process and has been chosen as the deadlock victim. Rerun the transaction.
            /// </summary>
            Deadlock = 1205,

            /// <summary>
            /// The operating system returned error %ls to SQL Server during a %S_MSG at offset %#016I64x in file '%ls'.
            /// Additional messages in the SQL Server error log and operating system error log may provide more detail.
            /// This is a severe system-level error condition 
            /// </summary>
            SevereSystemError = 823
        }
    }
}
