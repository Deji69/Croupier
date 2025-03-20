using System;
using System.Collections.Specialized;
using System.Text.Json.Nodes;

namespace Croupier {
	public class KillMethodVariant(KillMethod method, string name, string? image = null, StringCollection? tags = null)
		: KillMethod($"{name} {method.Name}", image ?? method.Image, method.Category, method.Target, [..method.Tags, ..(tags ?? [])], method.Keywords)
	{
		public KillMethod Method { get; private set; } = method;
		public string VariantName { get; private set; } = name;

		public bool IsSameMethodVariant(KillMethodVariant other) => Name == other.Name;

		public override bool IsSameMethod(KillMethod other) => Method.IsSameMethod(other);

		public override KillMethod GetBasicMethod() => Method;

		public static KillMethodVariant FromJsonVariant(KillMethod method, JsonNode json) {
			var name = (json["Name"]?.GetValue<string>()) ?? throw new Exception("Invalid property 'Name'.");
			var image = method.Image;

			if (json["Image"] != null)
				image = json["Image"]?.GetValue<string>() ?? method.Image;
			
			StringCollection tags = [];

			foreach (var node in json["Tags"]?.AsArray() ?? []) {
				var v = node?.GetValue<string>();
				if (v == null || v.Length == 0)
					continue;
				tags.Add(v);
			}

			return new(method, name, image, tags);
		}
	}
}
