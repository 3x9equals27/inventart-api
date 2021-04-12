using System;
using System.Collections.Generic;
using System.Linq;

namespace inventart_api.Authorization
{
    public static class Permission
    {
        public const string ListDiagnostic = "list:diagnostic";
        public const string UploadFile = "upload:file";

        public static readonly List<string> PermissionList = new List<string>(){ 
            ListDiagnostic,
            UploadFile
        };

        public static bool Check(string permission, string role)
        {
            switch (permission)
            {
                case ListDiagnostic:
                    return role.EqualsAny(Role.Curator, Role.Contributor, Role.Visitor);
                case UploadFile:
                    return role.EqualsAny(Role.Curator);
                default:
                    return false;
            }
        }

        public static bool EqualsAny(this string str, params string[] args)
        {
            return args.Any(x => StringComparer.InvariantCultureIgnoreCase.Equals(x, str));
        }
    }
}
