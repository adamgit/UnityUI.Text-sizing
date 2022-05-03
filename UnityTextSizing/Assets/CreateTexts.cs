using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateTexts : MonoBehaviour
{
	public List<string> texts;
	public Canvas c;

	public void Start()
	{
		foreach( var t in texts )
		{
			var go = DefaultControls.CreateText( new DefaultControls.Resources() );
			go.transform.SetParent( c.transform, false );
			var text = go.GetComponent<Text>();
			text.text = t;
		}
	}
}