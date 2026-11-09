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
		// Optional: BPC's own preset switch saves the colonists' current settings into the active preset first.
		private static MethodInfo colonistsMethod;
		private static MethodInfo saveCurrentStateMethod;

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
				colonistsMethod = FindStaticMethod(managerType, "Colonists");
				saveCurrentStateMethod = FindStaticMethodWithArity(managerType, "SaveCurrentState", 1);
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

		private static MethodInfo FindStaticMethodWithArity(Type type, string name, int arity)
		{
			for (Type current = type; current != null; current = current.BaseType)
			{
				MethodInfo method = current
					.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
					.FirstOrDefault(m => m.Name == name && m.GetParameters().Length == arity);
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

		/// <summary>Switches Better Pawn Control to the named preset. Returns false when nothing was switched.</summary>
		public static bool SetPawnControlPolicy(string policyName)
		{
			if (!working) return false;

			try
			{
				return SetPawnControlPolicyEx(policyName);
			}
			catch (Exception e)
			{
				Stop(e);
				return false;
			}
		}

		private static bool SetPawnControlPolicyEx(string policyName)
		{
			if (string.IsNullOrWhiteSpace(policyName))
				return false;

			object policy = Policies()
				.FirstOrDefault(p => string.Equals(PolicyLabel(p), policyName, StringComparison.OrdinalIgnoreCase));

			if (policy != null)
			{
				SaveCurrentState();
				loadStateMethod.Invoke(null, new[] { policy });
				return true;
			}

			if (warnedMissingPolicies.Add(policyName))
				Verse.Log.Warning($"Gear Up and Go could not find Better Pawn Control policy '{policyName}'. Gearing will continue without changing policy.");
			return false;
		}

		// Stores the colonists' current settings in the active preset, as BPC's own preset switch does, so changes made
		// since BPC last saved are not lost when Gear & Go later restores that preset.
		private static void SaveCurrentState()
		{
			if (colonistsMethod == null || saveCurrentStateMethod == null)
				return;

			object colonists = colonistsMethod.Invoke(null, null);
			if (colonists == null)
				return;

			Type parameterType = saveCurrentStateMethod.GetParameters()[0].ParameterType;
			object argument = parameterType.IsInstanceOfType(colonists)
				? colonists
				: Activator.CreateInstance(parameterType, colonists);
			saveCurrentStateMethod.Invoke(null, new[] { argument });
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
