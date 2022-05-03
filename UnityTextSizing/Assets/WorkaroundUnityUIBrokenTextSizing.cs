using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// For latest version of this, go to: github PublisherFork - https://github.com/adamgit/PublishersFork/blob/main/README.md
/// </summary>
public static class WorkaroundUnityUIBrokenTextSizing
{
	/// <summary>
	/// Converts incoming available space with potentially infinite dimensions into an effective finite space.
	///
	/// This is required because Unity implemented their sizing fundamentally wrongly, don't try to handle
	/// the common case of "a scrollable area / text page".
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	private static Vector2 _AvailableSpaceFrom( float? x, float? y )
	{
		return new Vector2( x ?? float.MaxValue, y ?? float.MaxValue );
	}

	public static Vector2 RequiredSizeOfText_UnityBuiltIn( this Text uiText, float? availableWidth, float? availableHeight, bool debug = false )
	{
		return new Vector2( uiText.preferredWidth, uiText.preferredHeight );
	}
	
	public static Vector2 RequiredSizeOfText_UnityTextGenerator( this Text uiText, float? availableWidth, float? availableHeight, bool debug = false )
	{
		var availableSpace = _AvailableSpaceFrom( availableWidth, availableHeight );
		
		/** Setup */
		uiText.cachedTextGenerator.Invalidate();
		TextGenerationSettings tempSettings = uiText.GetGenerationSettings( availableSpace );
		tempSettings.scaleFactor = 1;//dont know why but if I dont set it to 1 it returns a font that is too small. Line above should have made this unnecessary
		
		/** Calculation */
		if (!uiText.cachedTextGenerator.Populate(uiText.text, tempSettings))
		{
			/** Unity UI team did a bad job here: they force us to use Reflection because they hide their errors */
			Debug.LogError("UnityEngine bug: Failed to generate fit size. Error code (secret, from Unity): "+uiText.cachedTextGenerator.FetchLastErrorCode());
			Debug.LogError("Font = "+uiText.font+", TGS.font = "+tempSettings.font);
		}
		
		/** Output */
		var @return = new Vector2( uiText.cachedTextGenerator.GetPreferredWidth( uiText.text, tempSettings ),
			uiText.cachedTextGenerator.GetPreferredHeight( uiText.text, tempSettings ));
		
		return @return;
	}
	
	public static Vector2 RequiredSizeOfText_UnityTextGenerator_ForcedFontSize( this Text uiText, float? availableWidth, float? availableHeight, bool debug = false )
	{
		var availableSpace = _AvailableSpaceFrom( availableWidth, availableHeight );
		
		/** Setup */
		uiText.cachedTextGenerator.Invalidate();
		TextGenerationSettings tempSettings = uiText.GetGenerationSettings( availableSpace );
		tempSettings.scaleFactor = 1;//dont know why but if I dont set it to 1 it returns a font that is too small. Line above should have made this unnecessary
		tempSettings.fontSize = uiText.fontSize; // NOTE: Unity randomly ignored this, or randomly in some cases: manually setting this fixes some bugs in Unity's layout (to do with 'size-to-fit' flag in UI.Text inspector)
		tempSettings.resizeTextForBestFit = false;

		/** Calculation */
		if (!uiText.cachedTextGenerator.Populate(uiText.text, tempSettings))
		{ 
			Debug.LogError("UnityEngine bug: Failed to generate fit size. Error code (secret, from Unity): "+uiText.cachedTextGenerator.FetchLastErrorCode());
			Debug.LogError("Font = "+uiText.font+", TGS.font = "+tempSettings.font);
		}
		
		/** Output */
		var @return = new Vector2( uiText.cachedTextGenerator.GetPreferredWidth( uiText.text, tempSettings ),
			uiText.cachedTextGenerator.GetPreferredHeight( uiText.text, tempSettings ));
		
		return @return;
	}

	/// <summary>
	/// Here we use the CharacterInfo API to manually count glyphs.
	/// </summary>
	/// <param name="uiText"></param>
	/// <param name="availableWidth"></param>
	/// <param name="availableHeight"></param>
	/// <param name="debug"></param>
	/// <returns></returns>
	public static Vector2 RequiredSizeOfText_UnityCharacterInfo_WithWrapping( this Text uiText, float? availableWidth, float? availableHeight, bool debug = false )
	{
		var availableSpace = _AvailableSpaceFrom( availableWidth, availableHeight );

		Font myFont = uiText.font;
		CharacterInfo characterInfo;
		if( debug ) Debug.Log( "Using font size = from font:" + myFont.fontSize + ", from UI.Text:" + uiText.fontSize );

		char[] arr = uiText.text.ToCharArray();
		float currentLineHeight = 0;
		float currentLineLength = 0;
		float longestLineLength = 0;
		float totalLineHeights = 0;
		foreach( char c in arr )
		{
			myFont.GetCharacterInfo( c, out characterInfo, uiText.fontSize );

			if( (currentLineLength > 0 /** if too narrow to fit 1 char, force it to take 1 char, otherwise sizes become ridiculous*/) && currentLineLength + characterInfo.advance > availableSpace.x )
			{
				longestLineLength = Math.Max( longestLineLength, currentLineLength );
				currentLineLength = 0;
				totalLineHeights += currentLineHeight;
				currentLineHeight = 0;
			}

			currentLineLength += characterInfo.advance;
			if( debug ) Debug.Log( "char: " + c + " y: " + characterInfo.minY + "->" + characterInfo.maxY + ", h = " + characterInfo.glyphHeight );
			float heightAdvance = characterInfo.glyphHeight + (characterInfo.minY < 0 ? Math.Abs( characterInfo.minY ) : 0);
			currentLineHeight = Math.Max( currentLineHeight, heightAdvance );
		}

		longestLineLength = Math.Max( longestLineLength, currentLineLength );
		totalLineHeights += currentLineHeight;

		var @return = new Vector2( longestLineLength, totalLineHeights );

		return @return;
	}
	
	/// <summary>
	/// </summary>
	/// <param name="uiText"></param>
	/// <param name="availableWidth"></param>
	/// <param name="availableHeight"></param>
	/// <param name="debug"></param>
	/// <returns></returns>
	public static Vector2 RequiredSizeOfText_UnityCharacterInfo_NoWrap( this Text uiText, float? availableWidth, float? availableHeight, bool debug = false )
	{
		var availableSpace = _AvailableSpaceFrom( availableWidth, availableHeight );

		Font myFont = uiText.font;
		CharacterInfo characterInfo;
		if( debug ) Debug.Log( "Using font size = from font:" + myFont.fontSize + ", from UI.Text:" + uiText.fontSize );

		char[] arr = uiText.text.ToCharArray();
		float currentLineHeight = 0;
		float currentLineLength = 0;
		float longestLineLength = 0;
		float totalLineHeights = 0;
		foreach( char c in arr )
		{
			myFont.GetCharacterInfo( c, out characterInfo, uiText.fontSize );

			currentLineLength += characterInfo.advance;
			if( debug ) Debug.Log( "char: " + c + " y: " + characterInfo.minY + "->" + characterInfo.maxY + ", h = " + characterInfo.glyphHeight );
			float heightAdvance = characterInfo.glyphHeight + (characterInfo.minY < 0 ? Math.Abs( characterInfo.minY ) : 0);
			currentLineHeight = Math.Max( currentLineHeight, heightAdvance );
		}

		longestLineLength = Math.Max( longestLineLength, currentLineLength );
		totalLineHeights += currentLineHeight;

		var @return = new Vector2( longestLineLength, totalLineHeights );

		return @return;
	}
	
}