using System.Collections.Generic;

namespace Inventart.Authorization
{
    public static class Permission
    {
        public const string ListPainting = "list:painting";
        public const string CreatePainting = "create:painting";
        public const string ListUserRole = "list:user.role";
        public const string UploadFile = "upload:file";
        public const string EditSelf = "edit:self";
        public const string EditRoles = "edit:roles";

        public static readonly List<string> PermissionList = new List<string>(){
            ListPainting,
            CreatePainting,
            ListUserRole,
            UploadFile,
            EditSelf,
            EditRoles
        };
    }
}