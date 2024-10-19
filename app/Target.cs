using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text.Json.Nodes;

namespace Croupier
{
	public enum TargetType {
		Normal,
		Unique,
	}

	public static partial class TargetTypeMethods {
		public static TargetType FromString(string str) {
			return str.ToLower() switch {
				"normal" => TargetType.Normal,
				"unique" => TargetType.Unique,
				_ => throw new NotImplementedException()
			};
		}
	}

	public enum TargetNameFormat {
		Initials,
		Full,
		Short,
	}

	public static partial class TargetNameFormatMethods {
		public static string ToString(this TargetNameFormat id)
		{
			return id switch {
				TargetNameFormat.Full => "Full",
				TargetNameFormat.Short => "Short",
				_ => "Initials",
			};
		}

		public static TargetNameFormat FromString(string id)
		{
			return id switch {
				"Full" => TargetNameFormat.Full,
				"Short" => TargetNameFormat.Short,
				_ => TargetNameFormat.Initials,
			};
		}
	}

	public class MethodRule(Func<Disguise, KillMethod, Mission, bool> func, List<string> tags)
	{
		public readonly Func<Disguise, KillMethod, Mission, bool> Func = func;
		public readonly List<string> Tags = tags;
	}

	public class Target(string name, string initials, string shortName, string image, TargetType type, bool generic = false, Mission? mission = null) {
		public string Name { get; private set; } = name;
		public string Initials { get; private set; } = initials;
		public string ShortName { get; private set; } = shortName;
		public string Image { get; private set; } = image;
		public TargetType Type { get; private set; } = type;
		public bool Generic { get; private set; } = generic;
		public Mission? Mission { get; private set; } = mission;
		public StringCollection Keywords { get; private set; } = [];

		public Uri ImageUri => new(Path.Combine(Environment.CurrentDirectory, "actors", Image));

		public static Target FromJson(JsonNode json, Mission? mission = null) {
			var name = json["Name"]?.GetValue<string>() ?? throw new Exception("Config error, missing target 'Name' property.");
			var initials = json["Initials"]?.GetValue<string>() ?? throw new Exception("Config error, missing target 'Initials' property.");
			var shortName = json["ShortName"]?.GetValue<string>() ?? throw new Exception("Config error, missing target 'ShortName' property.");
			var image = json["Image"]?.GetValue<string>() ?? throw new Exception("Config error, missing target 'Image' property.");
			var type = TargetType.Normal;

			if (json["Type"] != null) {
				type = TargetTypeMethods.FromString(json["Type"]!.GetValue<string>());
			}

			var generic = json["IsGeneric"]?.GetValue<bool>() ?? false;
			var target = new Target(name, initials, shortName, image, type, generic, mission);

			var keywords = json["Keywords"]?.AsArray();
			foreach (var kw in keywords ?? []) {
				if (kw == null) continue;
				target.Keywords.Add(kw.GetValue<string>());
			}

			return target;
		}
	}
}
