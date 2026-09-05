using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace GearUpAndGo
{
	[StaticConstructorOnStartup]
	static class SetBetterPawnControl
	{
		private static readonly string[] packageIds =
		{
			"KyleMHB.BetterPawnControlForked",
			"VouLT.BetterPawnControl"
		};

		private static readonly string[] namespaces =
		{
			"BetterPawnControlForked",
			"BetterPawnControl"
		};

		public static bool working;
		private static readonly HashSet<string> warnedMissingPolicies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private static FieldInfo policiesField;
		private static PropertyInfo policiesProperty;
		private static MethodInfo loadStateMethod;
		private static MethodInfo getActivePolicyMethod;
		private static FieldInfo policyLabelField;

		public static bool Active()
		{
			return ModsConfig.ActiveModsInLoadOrder.Any(m =>
				packageIds.Any(packageId =>
					string.Equals(m.PackageIdPlayerFacing, packageId, StringComparison.OrdinalIgnoreCase)));
		}

		static SetBetterPawnControl()
		{
			if (Active())
			{
				working = TryBind();
			}
		}

		private static bool TryBind()
		{
			foreach (string ns in namespaces)
			{
				Type policyType = FindType(ns + ".Policy");
				Type managerType = FindType(ns + ".AssignManager");
				if (policyType == null || managerType == null)
					continue;

				FieldInfo policies = FindStaticField(managerType, "policies");
				PropertyInfo policiesPropertyInfo = policies == null ? FindStaticProperty(managerType, "policies") : null;
				MethodInfo loadState = FindStaticMethod(managerType, "LoadState", policyType);
				MethodInfo getActivePolicy = FindStaticMethod(managerType, "GetActivePolicy");
				FieldInfo label = policyType.GetField("label", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

				if ((policies == null && policiesPropertyInfo == null) || loadState == null || getActivePolicy == null || label == null)
					continue;

				policiesField = policies;
				policiesProperty = policiesPropertyInfo;
				loadStateMethod = loadState;
				getActivePolicyMethod = getActivePolicy;
				policyLabelField = label;
				return true;
			}

			Verse.Log.Error("Gear Up and Go could not bind to Better Pawn Control or Better Pawn Control Forked. Policy switching will be disabled.");
			return false;
		}

		private static Type FindType(string fullName)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type type = assembly.GetType(fullName, false);
				if (type != null)
					return type;
			}

			return null;
		}

		private static PropertyInfo FindStaticProperty(Type type, string name)
		{
			for (Type current = type; current != null; current = current.BaseType)
			{
				PropertyInfo property = current.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (property?.GetGetMethod(true) != null)
					return property;
			}

			return null;
		}

		private static FieldInfo FindStaticField(Type type, string name)
		{
			for (Type current = type; current != null; current = current.BaseType)
			{
				FieldInfo field = current.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null)
					return field;
			}

			return null;
		}

		private static MethodInfo FindStaticMethod(Type type, string name, params Type[] parameterTypes)
		{
			for (Type current = type; current != null; current = current.BaseType)
			{
				MethodInfo method = current.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, parameterTypes, null);
				if (method != null)
					return method;
			}

			return null;
		}

		public static void Stop(Exception e)
		{
			working = false;

			Verse.Log.Error($"Hey, dang, Gear Up and Go failed to use Better Pawn Control:\n\n{e}");
		}

		public static void SetPawnControlPolicy(string policyName)
		{
			if (!working) return;

			try
			{
				SetPawnControlPolicyEx(policyName);
			}
			catch (Exception e)
			{
				Stop(e);
			}
		}

		private static void SetPawnControlPolicyEx(string policyName)
		{
			if (string.IsNullOrWhiteSpace(policyName))
				return;

			object policy = Policies()
				.FirstOrDefault(p => string.Equals(PolicyLabel(p), policyName, StringComparison.OrdinalIgnoreCase));

			if (policy != null)
			{
				loadStateMethod.Invoke(null, new[] { policy });
				return;
			}

			if (warnedMissingPolicies.Add(policyName))
				Verse.Log.Warning($"Gear Up and Go could not find Better Pawn Control policy '{policyName}'. Gearing will continue without changing policy.");
		}

		public static string CurrentPolicy()
		{
			if (!working) return "";

			try
			{
				return CurrentPolicyEx();
			}
			catch (Exception e)
			{
				Stop(e);
				return "";
			}
		}

		private static string CurrentPolicyEx() =>
			PolicyLabel(getActivePolicyMethod.Invoke(null, null));

		public static List<string> PolicyList()
		{
			if (!working) return null;

			try
			{
				return PolicyListEx().ToList();
			}
			catch (Exception e)
			{
				Stop(e);
				return null;
			}
		}

		private static List<string> PolicyListEx()
		{
			List<string> result = new();
			foreach (object policy in Policies())
			{
				string label = PolicyLabel(policy);
				if (!string.IsNullOrWhiteSpace(label))
					result.Add(label);
			}

			return result;
		}

		private static IEnumerable<object> Policies()
		{
			object value = policiesField != null
				? policiesField.GetValue(null)
				: policiesProperty.GetValue(null, null);

			return value is System.Collections.IEnumerable policies
				? policies.Cast<object>()
				: Enumerable.Empty<object>();
		}

		private static string PolicyLabel(object policy)
		{
			return policy == null ? "" : policyLabelField.GetValue(policy) as string ?? "";
		}
	}
}
