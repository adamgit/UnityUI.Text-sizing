using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class InterrogateTextSizes : MonoBehaviour
{
	public Canvas c;

	[ContextMenu( "Go - 1x1" )]
	public void go_1_1()
	{
		var texts = c.gameObject.GetComponentsInChildren<Text>();
		_OutputRequiredSizesForTextInAvailableSpace( texts, Vector2.one );
	}
	
	[ContextMenu( "Go - 100x100" )]
	public void go_100_100()
	{
		var texts = c.gameObject.GetComponentsInChildren<Text>();
		_OutputRequiredSizesForTextInAvailableSpace( texts, Vector2.one * 100f );
	}
	
	[ContextMenu( "Go - 16x16" )]
	public void go_16_16()
	{
		var texts = c.gameObject.GetComponentsInChildren<Text>();
		_OutputRequiredSizesForTextInAvailableSpace( texts, Vector2.one * 16f );
	}

	private void _OutputRequiredSizesForTextInAvailableSpace( IEnumerable<Text> texts, Vector2 availableSpace )
	{
		foreach( var t in texts )
		{
			var uPreferred = t.RequiredSizeOfText_UnityBuiltIn( availableSpace.x, availableSpace.y );
			var uTextGen = t.RequiredSizeOfText_UnityTextGenerator( availableSpace.x, availableSpace.y );
			var uTextGen_fontOverride = t.RequiredSizeOfText_UnityTextGenerator_ForcedFontSize( availableSpace.x, availableSpace.y );
			var uCharInfoWrap = t.RequiredSizeOfText_UnityCharacterInfo_WithWrapping( availableSpace.x, availableSpace.y );
			var uCharInfoNoWrap = t.RequiredSizeOfText_UnityCharacterInfo_NoWrap( availableSpace.x, availableSpace.y );
			
			var sb = new StringBuilder();
			sb.AppendLine( "   'Text.preferred' requires "+uPreferred+" to fit into space: "+availableSpace+" -- font size = "+_SizeOfFont( t ) );
			sb.AppendLine( "   'TextGenerator' requires "+uTextGen+" to fit into space: "+availableSpace+" -- font size = "+_SizeOfFont( t ) );
			sb.AppendLine( "   'TextGenerator + fontsize' requires "+uTextGen_fontOverride+" to fit into space: "+availableSpace+" -- font size = "+_SizeOfFont( t ) );
			sb.AppendLine( "   'CharacterInfo + no-wrap' requires "+uCharInfoNoWrap+" to fit into space: "+availableSpace+" -- font size = "+_SizeOfFont( t ) );
			sb.AppendLine( "   'CharacterInfo + wrap' requires "+uCharInfoWrap+" to fit into space: "+availableSpace+" -- font size = "+_SizeOfFont( t ) );
			
			Debug.Log( "Text \""+t.text+"\":\n"+sb.ToString() );
		}
	}

	private string _SizeOfFont( Text text )
	{
		return text.fontSize + (text.resizeTextForBestFit ? " (" + text.resizeTextMinSize + " <= size <= " + text.resizeTextMaxSize + ")" : "");
	}
}