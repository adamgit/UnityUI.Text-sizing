using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// The team at Unity who wrote UI.Text deliberately hide errors, returning 0 size for text, for no obvious reason.
/// Here we re-instate the missing code.
/// </summary>
public static class WorkaroundUnityUIBrokenTextGenerator
{
	public static string FetchLastErrorCode(this TextGenerator tg)
	{
		var infoProp = typeof(TextGenerator).GetField("m_LastValid", BindingFlags.Instance | BindingFlags.NonPublic);
		var tge = infoProp.GetValue(tg);
		return tge.ToString();
	}
}