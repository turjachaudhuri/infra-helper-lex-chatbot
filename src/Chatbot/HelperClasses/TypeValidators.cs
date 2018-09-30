using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace Chatbot.HelperClasses
{
    /// <summary>
    /// Stub implementation that validates input values. A real implementation would check a datastore.
    /// </summary>
    public static class TypeValidators
    {
        public static readonly ImmutableArray<string> VALID_ACTION_TYPES =
                ImmutableArray.Create<string>(new string[] 
                            { "start", "stop","terminate" });

        public static bool IsValidActionType(string action)
        {
            return VALID_ACTION_TYPES.Contains(action.ToLower());
        }
        public static string ListOfValidActionTypes()
        {
            return string.Join(" \n ", VALID_ACTION_TYPES);
        }

        public static readonly ImmutableArray<string> VALID_AMI_TYPES =
                ImmutableArray.Create<string>(new string[]
                            { "ubuntu", "redhat","windows","amazonlinux" });

        public static readonly ImmutableArray<string> VALID_AMI_TYPES_RESPONSE =
                ImmutableArray.Create<string>(new string[]
                            { "Ubuntu", "Red Hat","Windows","Amazon Linux" });

        public static bool IsValidAMIType(string AMIType)
        {
            return VALID_AMI_TYPES.Contains(Regex.Replace(AMIType.ToLower().Trim(), @"\s+", ""));
        }
        public static string ListOfValidAMITypes()
        {
            return string.Join('\n', VALID_AMI_TYPES_RESPONSE);
        }

        public static readonly ImmutableArray<string> VALID_STORAGE_TYPES =
                ImmutableArray.Create<string>(new string[]
                            { "magnetic", "provisioned","generalpurpose"});

        public static readonly ImmutableArray<string> VALID_STORAGE_TYPES_RESPONSE =
                ImmutableArray.Create<string>(new string[]
                            { "Magnetic", "Provisioned","General purpose"});

        public static bool IsValidStorageType(string StorageType)
        {
            return VALID_STORAGE_TYPES.Contains(Regex.Replace(StorageType.ToLower().Trim(), @"\s+", ""));
        }
        public static string ListOfValidStorageTypes()
        {
            return string.Join('\n', VALID_STORAGE_TYPES_RESPONSE);
        }

        public static readonly ImmutableArray<string> VALID_INSTANCE_TYPES =
                ImmutableArray.Create<string>(new string[]
                            { "t2micro", "t2small","t2medium"});

        public static readonly ImmutableArray<string> VALID_INSTANCE_TYPES_RESPONSE =
                ImmutableArray.Create<string>(new string[]
                            { "t2micro", "t2small","t2medium"});

        public static bool IsValidInstanceType(string InstanceType)
        {
            return VALID_INSTANCE_TYPES.Contains(InstanceType.ToLower());
        }
        public static string ListOfValidInstanceTypes()
        {
            return string.Join('\n', VALID_INSTANCE_TYPES_RESPONSE);
        }
    }
}
