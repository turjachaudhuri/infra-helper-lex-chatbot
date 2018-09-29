using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Chatbot.HelperClasses
{
    /// <summary>
    /// Stub implementation that validates input values. A real implementation would check a datastore.
    /// </summary>
    public static class TypeValidators
    {
        public static readonly ImmutableArray<string> VALID_ACTION_TYPES =
                ImmutableArray.Create<string>(new string[] 
                            { "start", "stop","describe","terminate" });

        public static bool IsValidActionType(string action)
        {
            return VALID_ACTION_TYPES.Contains(action.ToLower());
        }
        public static string ListOfValidActionTypes()
        {
            return string.Join(',', VALID_ACTION_TYPES);
        }
    }
}
