// Source code is taken from CommonAPI (GPL-3.0) - https://github.com/limoka/CommonAPI
// Source code is indirectly taken from R2API (MIT) - https://github.com/risk-of-thunder/R2API
using Mono.Cecil;

namespace YuanAPI {
    
    public static class CecilExtension {

        internal static bool IsSubTypeOf(this TypeDefinition typeDefinition, string typeFullName) {
            if (typeDefinition.FullName == typeFullName) {
                return true;
            }

            var typeDefBaseType = typeDefinition.BaseType?.Resolve();
            while (typeDefBaseType != null) {
                if (typeDefBaseType.FullName == typeFullName) {
                    return true;
                }

                typeDefBaseType = typeDefBaseType.BaseType?.Resolve();
            }

            return false;
        }
    }
}
