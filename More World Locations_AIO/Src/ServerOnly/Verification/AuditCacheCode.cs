using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// The part of MWL's DLL that can reach a verdict, as text: the code of the
/// server-only feature — what opens, converts, extracts and judges a template —
/// and the data the DLL embeds (the stock snapshot, the catalogue's names).
///
/// <para><b>Why not the DLL's bytes.</b> Every MWL release changes the DLL: a
/// version string, a port fix, a trader. Hashing it made each of those re-audit
/// all ~190 templates on the next start, several minutes on a dedicated server,
/// to reach the verdicts it already had. The rest of MWL reaches a verdict only
/// through what the key and the per-template inputs already cover: the
/// definitions (each template's own line), the bundles (each template's own, and
/// the shared ones), MWL's settings, and the stock prefabs it may register or
/// edit (signed one by one). For anything else, <see cref="AuditCacheCanonical.MwlCacheVersion"/>.</para>
///
/// <para><b>Why resolved IL and not the IL's bytes.</b> A method body refers to
/// strings, methods, fields and types by metadata token, and tokens are numbered
/// across the whole assembly: an unrelated change elsewhere in MWL renumbers
/// them, and the bytes of code that did not change would change with it. Every
/// token is written as what it names instead, so the text changes exactly when
/// the code does.</para>
/// </summary>
public static class AuditCacheCode
{
    /// <summary>The namespace of the server-only feature; its sub-namespaces are in it too.</summary>
    public const string ServerOnlyNamespace = "More_World_Locations_AIO.ServerOnly";

    /// <summary>Whether a type is part of the server-only feature. A nested type answers with its outermost type's namespace.</summary>
    public static bool InServerOnly(Type type)
    {
        string ns = type?.Namespace ?? "";
        return ns == ServerOnlyNamespace || ns.StartsWith(ServerOnlyNamespace + ".", StringComparison.Ordinal);
    }

    private const BindingFlags Declared =
        BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private static readonly Dictionary<short, OpCode> OpCodesByValue = BuildOpCodes();

    /// <summary>
    /// The canonical text of every type in <paramref name="assembly"/> that
    /// <paramref name="inScope"/> accepts, and of every resource the assembly
    /// embeds. Types, members and resources are sorted by ordinal name, so the
    /// text does not depend on metadata order.
    /// </summary>
    public static string Canonical(Assembly assembly, Func<Type, bool> inScope)
    {
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));
        if (inScope == null) throw new ArgumentNullException(nameof(inScope));

        Type[] all;
        try
        {
            all = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            // A type that cannot load is a type whose code cannot be vouched for.
            throw new InvalidOperationException("MWL's types could not all be loaded: " +
                                                (ex.LoaderExceptions.Length > 0 ? ex.LoaderExceptions[0]?.Message : ex.Message));
        }

        List<Type> types = new List<Type>();
        foreach (Type type in all)
        {
            if (inScope(type))
                types.Add(type);
        }
        types.Sort((a, b) => string.CompareOrdinal(Name(a), Name(b)));

        StringBuilder text = new StringBuilder();
        foreach (Type type in types)
            AppendType(text, type);

        List<string> resources = new List<string>(assembly.GetManifestResourceNames());
        resources.Sort(StringComparer.Ordinal);
        foreach (string resource in resources)
        {
            if (IsBuildRecord(resource))
                continue;
            using Stream? stream = assembly.GetManifestResourceStream(resource);
            text.Append("resource\t").Append(AuditCacheCanonical.Escape(resource)).Append('\t')
                .Append(stream == null ? "unreadable" : Sha256(stream)).Append('\n');
        }
        return text.ToString();
    }

    /// <summary>
    /// A resource the build writes about itself, not data anything reads:
    /// ILRepack's list of the assemblies it merged, which names MWL's own
    /// version. Hashing it made a release that changed only the version string
    /// re-audit every template (station, 25 Sep 2026).
    /// </summary>
    public static bool IsBuildRecord(string resource) => string.Equals(resource, "ILRepack.List", StringComparison.Ordinal);

    private static void AppendType(StringBuilder text, Type type)
    {
        text.Append("type\t").Append(AuditCacheCanonical.Escape(Name(type)))
            .Append('\t').Append(((int)type.Attributes).ToString(CultureInfo.InvariantCulture))
            .Append('\t').Append(AuditCacheCanonical.Escape(type.BaseType == null ? "" : Name(type.BaseType)));
        List<string> interfaces = new List<string>();
        foreach (Type implemented in type.GetInterfaces())
            interfaces.Add(Name(implemented));
        interfaces.Sort(StringComparer.Ordinal);
        foreach (string implemented in interfaces)
            text.Append('\t').Append(AuditCacheCanonical.Escape(implemented));
        text.Append('\n');

        List<string> fields = new List<string>();
        foreach (FieldInfo field in type.GetFields(Declared))
        {
            string line = "field\t" + AuditCacheCanonical.Escape(field.Name) + "\t" + AuditCacheCanonical.Escape(Name(field.FieldType))
                          + "\t" + ((int)field.Attributes).ToString(CultureInfo.InvariantCulture);
            if (field.IsLiteral)
                line += "\t" + AuditCacheCanonical.Escape(Constant(field));
            fields.Add(line + "\n");
        }
        fields.Sort(StringComparer.Ordinal);
        foreach (string field in fields)
            text.Append(field);

        List<KeyValuePair<string, string>> methods = new List<KeyValuePair<string, string>>();
        foreach (MethodBase method in Methods(type))
            methods.Add(new KeyValuePair<string, string>(Signature(method), Body(method)));
        methods.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
        foreach (KeyValuePair<string, string> method in methods)
            text.Append("method\t").Append(AuditCacheCanonical.Escape(method.Key)).Append('\n').Append(method.Value);
    }

    private static IEnumerable<MethodBase> Methods(Type type)
    {
        foreach (ConstructorInfo constructor in type.GetConstructors(Declared))
            yield return constructor;
        foreach (MethodInfo method in type.GetMethods(Declared))
            yield return method;
    }

    private static string Signature(MethodBase method) =>
        method.ToString() + "\t" + ((int)method.Attributes).ToString(CultureInfo.InvariantCulture);

    /// <summary>A method body as one line of resolved instructions, then its locals and handlers.</summary>
    private static string Body(MethodBase method)
    {
        MethodBody? body;
        try
        {
            body = method.GetMethodBody();
        }
        catch (Exception ex)
        {
            return "body\tunreadable " + AuditCacheCanonical.Escape(ex.GetType().Name) + "\n";
        }
        if (body == null)
            return "body\tnone\n";

        StringBuilder text = new StringBuilder("il\t");
        byte[] il = body.GetILAsByteArray() ?? Array.Empty<byte>();
        Type[]? typeArguments = method.DeclaringType != null && method.DeclaringType.IsGenericType
            ? method.DeclaringType.GetGenericArguments() : null;
        Type[]? methodArguments = method.IsGenericMethod ? method.GetGenericArguments() : null;
        Module module = method.Module;
        int at = 0;
        while (at < il.Length)
        {
            short value = il[at++];
            if (value == 0xFE && at < il.Length)
                value = unchecked((short)(0xFE00 | il[at++]));
            if (!OpCodesByValue.TryGetValue(value, out OpCode op))
            {
                text.Append("?").Append(value.ToString("x", CultureInfo.InvariantCulture)).Append(';');
                continue;
            }
            text.Append(op.Name);
            string operand = Operand(op, il, ref at, module, typeArguments, methodArguments);
            if (operand.Length > 0)
                text.Append(' ').Append(AuditCacheCanonical.Escape(operand));
            text.Append(';');
        }
        text.Append('\n');

        text.Append("locals\t").Append(body.InitLocals ? '1' : '0');
        foreach (LocalVariableInfo local in body.LocalVariables)
            text.Append('\t').Append(AuditCacheCanonical.Escape(Name(local.LocalType))).Append(local.IsPinned ? "*" : "");
        text.Append('\n');
        foreach (ExceptionHandlingClause clause in body.ExceptionHandlingClauses)
        {
            string caught = "";
            if (clause.Flags == ExceptionHandlingClauseOptions.Clause)
            {
                try { caught = clause.CatchType == null ? "" : Name(clause.CatchType); }
                catch { caught = "unresolved"; }
            }
            text.Append("handler\t").Append(clause.Flags.ToString())
                .Append('\t').Append(clause.TryOffset.ToString(CultureInfo.InvariantCulture))
                .Append('\t').Append(clause.TryLength.ToString(CultureInfo.InvariantCulture))
                .Append('\t').Append(clause.HandlerOffset.ToString(CultureInfo.InvariantCulture))
                .Append('\t').Append(clause.HandlerLength.ToString(CultureInfo.InvariantCulture))
                .Append('\t').Append(AuditCacheCanonical.Escape(caught)).Append('\n');
        }
        return text.ToString();
    }

    private static string Operand(OpCode op, byte[] il, ref int at, Module module, Type[]? typeArguments, Type[]? methodArguments)
    {
        switch (op.OperandType)
        {
            case OperandType.InlineNone:
                return "";
            case OperandType.ShortInlineBrTarget:
                return ((sbyte)il[at++]).ToString(CultureInfo.InvariantCulture);
            case OperandType.ShortInlineI:
                return (op.Value == OpCodes.Ldc_I4_S.Value ? ((sbyte)il[at++]).ToString(CultureInfo.InvariantCulture)
                    : il[at++].ToString(CultureInfo.InvariantCulture));
            case OperandType.ShortInlineVar:
                return il[at++].ToString(CultureInfo.InvariantCulture);
            case OperandType.InlineVar:
            {
                ushort v = BitConverter.ToUInt16(il, at);
                at += 2;
                return v.ToString(CultureInfo.InvariantCulture);
            }
            case OperandType.InlineI:
            case OperandType.InlineBrTarget:
            {
                int v = BitConverter.ToInt32(il, at);
                at += 4;
                return v.ToString(CultureInfo.InvariantCulture);
            }
            case OperandType.ShortInlineR:
            {
                float v = BitConverter.ToSingle(il, at);
                at += 4;
                return v.ToString("R", CultureInfo.InvariantCulture);
            }
            case OperandType.InlineI8:
            {
                long v = BitConverter.ToInt64(il, at);
                at += 8;
                return v.ToString(CultureInfo.InvariantCulture);
            }
            case OperandType.InlineR:
            {
                double v = BitConverter.ToDouble(il, at);
                at += 8;
                return v.ToString("R", CultureInfo.InvariantCulture);
            }
            case OperandType.InlineSwitch:
            {
                int count = BitConverter.ToInt32(il, at);
                at += 4;
                StringBuilder targets = new StringBuilder();
                for (int i = 0; i < count; i++)
                {
                    targets.Append(i == 0 ? "" : ",").Append(BitConverter.ToInt32(il, at).ToString(CultureInfo.InvariantCulture));
                    at += 4;
                }
                return targets.ToString();
            }
            default:
            {
                int token = BitConverter.ToInt32(il, at);
                at += 4;
                return Resolve(op.OperandType, token, module, typeArguments, methodArguments);
            }
        }
    }

    /// <summary>What a token names. A token that will not resolve is written as itself: a miss at worst, never a false match.</summary>
    private static string Resolve(OperandType kind, int token, Module module, Type[]? typeArguments, Type[]? methodArguments)
    {
        try
        {
            switch (kind)
            {
                case OperandType.InlineString:
                    return "\"" + module.ResolveString(token) + "\"";
                case OperandType.InlineSig:
                    return "sig:" + BitConverter.ToString(module.ResolveSignature(token));
                default:
                    MemberInfo? member = module.ResolveMember(token, typeArguments, methodArguments);
                    return member switch
                    {
                        null => "null",
                        Type t => Name(t),
                        FieldInfo f => (f.DeclaringType == null ? "" : Name(f.DeclaringType)) + "::" + f.Name + ":" + Name(f.FieldType),
                        MethodBase m => (m.DeclaringType == null ? "" : Name(m.DeclaringType)) + "::" + m,
                        _ => member.ToString() ?? "",
                    };
            }
        }
        catch (Exception)
        {
            return "token:" + token.ToString("x8", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// A type's name without any assembly's: <c>FullName</c> spells a generic
    /// type's arguments with their assembly names and versions, so every
    /// <c>List&lt;SomeMwlType&gt;</c> carried MWL's own version and a release that
    /// changed only that re-audited everything (station, 25 Sep 2026).
    /// </summary>
    private static string Name(Type type) => type.ToString();

    private static string Constant(FieldInfo field)
    {
        try
        {
            object? value = field.GetRawConstantValue();
            return value == null ? "null" : Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
        }
        catch (Exception ex)
        {
            return "unreadable " + ex.GetType().Name;
        }
    }

    private static string Sha256(Stream stream)
    {
        using System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create();
        byte[] hash = sha.ComputeHash(stream);
        StringBuilder text = new StringBuilder(hash.Length * 2);
        foreach (byte b in hash)
            text.Append(b.ToString("x2", CultureInfo.InvariantCulture));
        return text.ToString();
    }

    private static Dictionary<short, OpCode> BuildOpCodes()
    {
        Dictionary<short, OpCode> map = new Dictionary<short, OpCode>();
        foreach (FieldInfo field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.GetValue(null) is OpCode op)
                map[op.Value] = op;
        }
        return map;
    }
}
