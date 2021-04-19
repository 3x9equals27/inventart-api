using System.Collections.Generic;

namespace Inventart.Authorization
{
    public static class Permission
    {
        public const string ListDiagnostic = "list:diagnostic";
        public const string UploadFile = "upload:file";

        public static readonly List<string> PermissionList = new List<string>(){
            ListDiagnostic,
            UploadFile
        };
    }
}
