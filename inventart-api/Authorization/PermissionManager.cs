using System;
using System.Linq;

namespace Inventart.Authorization
{
    public static class PermissionManager
    {
        public static bool Check(string permission, string role)
        {
            switch (permission)
            {
                case Permission.ListDiagnostic:
                    return role.EqualsAny(Role.Curator, Role.Contributor, Role.Visitor, Role.Guest);

                case Permission.UploadFile:
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