using System.Collections.Generic;

namespace Inventart.Authorization
{
    public static class Permission
    {
        public const string ListDiagnostic = "list:diagnostic";
        public const string ListUserRole = "list:user.role";
        public const string UploadFile = "upload:file";
        public const string EditSelf = "edit:self";
        public const string EditRoles = "edit:roles";

        public static readonly List<string> PermissionList = new List<string>(){
            ListDiagnostic,
            ListUserRole,
            UploadFile,
            EditSelf,
            EditRoles
        };
    }
}