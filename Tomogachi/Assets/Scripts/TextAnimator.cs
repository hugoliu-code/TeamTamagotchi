using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/* 
Written by Hugo Liu for the Lost Kids Game Developers, 2022

This script enables custom richtexts that can apply specific effects.
Works alongside native TextMeshPro richtext.
Add this script to an object containing a TextMeshPro text component
Drag TMP reference to the public "textComponent" variable in the editor.

Surround the desired text to be effected with the appropriate rich text
Ex: "<w>wave<w>", "<r>rainbow<r>"
*/





public class TextAnimator : MonoBehaviour
{
	
	[SerializeField] TMP_Text textComponent;
	
	//Effects =============================================================================================
	//The various effects, and their parameters and relevant variables
	[Header("Waving")]
	[SerializeField] float WaveSize = 0.03f; //The size of the wave vertically
	[SerializeField] float WaveTime = 4f; //The speed of the wave
	[SerializeField] float XWaveWeight = 30f; //How many waves in 1/10ths
	private int[] waveArray; //Array with same size as the non-richtext characters. 1 for apply effect, 0 by default.
	
	
	[Header("Rainbow")]
	[SerializeField] float XColorWeight = 0.5f; //The concentration of colors (how wide the color bands will be)
	[SerializeField] float TimeWeight = 1.3f; //how fast the colors will travel
	[SerializeField] Color referenceColor; //A color used for reference in the effect function. Preferably an obscure color.
	private int[] rainbowArray; //Array with same size as the non-richtext characters. 1 for apply effect, 0 by default.
	
	
	[Header("Shaking")]
	[SerializeField] float MaxShake = 15f; //How intense the shake will be
	[SerializeField] int ShakeSpeed = 1; //How many frames between each shake
	private int shakeFrame = 0;
	private int[] shakeArray;//Array with same size as the non-richtext characters. 1 for apply effect, 0 by default.
	
	[Header("Time Wait")]
	private float[] waitArray; //Array of times to wait at each character; Used in the coroutine
	
	private string realText; //the text with no rich text
	private int[] revealArray;
	private Color[] originalColors;



	//Rich Text ============================================================================================
	//https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/manual/RichTextSupportedTags.html
	string[] richText = new string[] //All native unity richtexts
	{
		"color",
		"/color",
		"alpha",
		"/alpha",
		"align",
		"allcaps",
		"b",
		"br",
		"cspace",
		"font",
		"font-weight",
		"gradient",
		"i",
		"indent",
		"line-height",
		"line-indent",
		"link",
		"lowercase",
		"margin",
		"mark",
		"mspace",
		"nobr",
		"noparse",
		"page",
		"pos",
		"rotate",
		"s",
		"/s",
		"size",
		"smallcaps",
		"space",
		"sprite",
		"strikethrough",
		"style",
		"sub",
		"sup",
		"u",
		"uppercase",
		"voffset",
		"width"
		
	};
	
	string[] customRichText = new string[]{ //richtexts supported in this script
		"k", //shake
		"w", //wave
		"r", //rainbow
		"t" //time
	};
	
	
	
	
	
	
	
	//Native Functions =============================================================================================
	
	void Start(){

		ReScan();
		textComponent.ForceMeshUpdate();
		StartCoroutine(RevealCharacterCoroutine());

	}
	
    void Update()
	{
		if(Input.GetKeyDown(KeyCode.Space)){
			textComponent.text = "hello <k> there <k>";
			ReScan();
		}
		
		
		//Make sure the mesh is the most recently updated one
	    textComponent.ForceMeshUpdate();
		var textInfo = textComponent.textInfo;
		
			
			
			

		RevealCharacters(textInfo);
			
			
			
		//All Effect Functions
		EffectShake(textInfo);
		EffectWave(textInfo);
		EffectRainbow(textInfo);
		
	    
		//For specific functions, this will update the position of the mesh (shake and wave, which affects verticies)
	    for (int i =0; i<textInfo.meshInfo.Length;++i){
	    	var meshInfo = textInfo.meshInfo[i];
	    	meshInfo.mesh.vertices = meshInfo.vertices;
	    	textComponent.UpdateGeometry(meshInfo.mesh,i);
	    }
	}
	
	
	//Custom Functions ==============================================================================================
	private void ReScan(){ //Should be called when any new text is updated. Updates the arrays that determines which characters get effects applied on them
		
		
		//Creates "original text": the text with all native richtext removed; <color>, <alpha>, etc...
		string originalText = textComponent.text;
		char[] originalTextArray = originalText.ToCharArray();
		string scrubbedText = "";
		for(int i = 0; i<originalText.Length;i++){
			bool foundRichText = false;
			foreach(string n in richText){
				if(i+1+n.Length <= originalText.Length && originalText.Substring(i,1+n.Length).Equals("<" + n)){
					i = i + FindNext(originalText.Substring(i+1+n.Length),">") + 1+n.Length;
					foundRichText = true;
					break;
				}
			}
			
			if(!foundRichText)
				scrubbedText += originalTextArray[i];
		}
		
		//Creates "only characters" which has no custom or native richtext. Used to set length of the effect arrays.
		string onlyCharacters = "";
		for(int i = 0; i<scrubbedText.Length;i++){
			bool foundRichText = false;
			foreach(string n in customRichText){
				if(i+1+n.Length <= scrubbedText.Length && scrubbedText.Substring(i,1+n.Length).Equals("<" + n)){
					i = i + FindNext(scrubbedText.Substring(i+1+n.Length),">") + 1+n.Length;
					foundRichText = true;
					break;
				}
			}
			
			if(!foundRichText)
				onlyCharacters += scrubbedText.ToCharArray()[i];
		}
		// Set global reference for reveal coroutine
		realText = onlyCharacters;
		
		//Only has unity native richtext. Creates the final text that will be read by textmeshpro (TMP needs the native richtext, but should ignore the custom, which is removed)
		string finalText = "";
		for(int i = 0; i<originalText.Length;i++){
			bool foundRichText = false;
			foreach(string n in customRichText){
				if(i+1+n.Length <= originalText.Length && originalText.Substring(i,1+n.Length).Equals("<" + n)){
					i = i + FindNext(originalText.Substring(i+1+n.Length),">") + 1+n.Length;
					foundRichText = true;
					break;
				}
			}
			
			if(!foundRichText)
				finalText += originalTextArray[i];
		}
		
		//Sets textMeshPro text
		textComponent.text = finalText;
		
		
		//Need to find special rich texts, and update the effect arrays
		//Each effect will have its own separate handling hardcoded, as they have different needs, especially with parameters
		
		
		//Variables used by multiple effect parsers
		int charIndex;
		bool foundOutsideDelimiter;
		
		//Time to Wait Effect Parser <t> ================================================================================
		waitArray = new float[onlyCharacters.Length];
		charIndex = 0;
		foundOutsideDelimiter = false;
		
		for(int i = 0;i<scrubbedText.Length;i++){
			if(i+2 <= scrubbedText.Length && scrubbedText.Substring(i,2).Equals("<t")){
				int end = FindNext(scrubbedText.Substring(i+2), ">") + 3 + i; //the end of the <t=...>
				float parameter = float.Parse(scrubbedText.Substring(i+3,end-i-4)); //The parameter enclosed, determining how long to wait before revealing a character
				waitArray[charIndex] = parameter; //Set time for character at charIndex to wait

				i = end;
			}
			
			
			
			if(scrubbedText.ToCharArray()[i].Equals('>') || scrubbedText.ToCharArray()[i].Equals('<')){
				foundOutsideDelimiter = !foundOutsideDelimiter;
				continue;
			}
			if(foundOutsideDelimiter)
				continue;
		
			charIndex++;
		}
		
		
		//Shake Effect Parser <k> ======================================================================================
		shakeArray = new int[onlyCharacters.Length];
		charIndex = 0; //the "true" location of the characters; How many valid(non-richtext) characters have been parsed
		foundOutsideDelimiter = false;
		
		for(int i = 0; i<scrubbedText.Length;i++){
			if(i+2 <= scrubbedText.Length && scrubbedText.Substring(i,2).Equals("<k")){
				int start = FindNext(scrubbedText.Substring(i+2), ">") + 3 + i;
				int end = FindNext(scrubbedText.Substring(i+2), "<k>") + 1 + i;
				bool foundInsideDelimiter = false;
				for(int a = start; a<=end;a++){
					//If statement checks for extra "< ... >" inside the current effect's delimiters
					if(scrubbedText.ToCharArray()[a].Equals('>') || scrubbedText.ToCharArray()[a].Equals('<')){
						foundInsideDelimiter = !foundInsideDelimiter;
						continue;
					}
					if(foundInsideDelimiter) //If we are within a "<" and  ">", meaning we should ignore until valid characters
						continue;
					shakeArray[charIndex] = 1;
					charIndex++;
					
				}
				i = end+3;
				continue;
			}
			//Checks for extra "< ... >" OUTSIDE the current effect's delimiters, which shouldn't be counted as valid characters
			if(scrubbedText.ToCharArray()[i].Equals('>') || scrubbedText.ToCharArray()[i].Equals('<')){
				foundOutsideDelimiter = !foundOutsideDelimiter;
				continue;
			}
			if(foundOutsideDelimiter)
				continue;
		
			charIndex++;
		}
		
		
		//Wave Effect Parser <w> ========================================================================================
		waveArray = new int[onlyCharacters.Length];
		charIndex = 0; //the "true" location of the characters
		foundOutsideDelimiter = false;
		
		for(int i = 0; i<scrubbedText.Length;i++){
			if(i+2 <= scrubbedText.Length && scrubbedText.Substring(i,2).Equals("<w")){
				int start = FindNext(scrubbedText.Substring(i+2), ">") + 3 + i;
				int end = FindNext(scrubbedText.Substring(i+2), "<w>") + 1 + i;
				bool foundInsideDelimiter = false;
				for(int a = start; a<=end;a++){
					if(scrubbedText.ToCharArray()[a].Equals('>') || scrubbedText.ToCharArray()[a].Equals('<')){
						foundInsideDelimiter = !foundInsideDelimiter;
						continue;
					}
					if(foundInsideDelimiter)
						continue;
					waveArray[charIndex] = 1;
					charIndex++;
				}
				i = end+3;
				continue;
			}
			if(scrubbedText.ToCharArray()[i].Equals('>') || scrubbedText.ToCharArray()[i].Equals('<')){
				foundOutsideDelimiter = !foundOutsideDelimiter;
				continue;
			}
			if(foundOutsideDelimiter)
				continue;
			charIndex++;
		}


		//Rainbow Effect Parser <r> ==================================================================================
		rainbowArray = new int[onlyCharacters.Length];
		charIndex = 0; //the "true" location of the characters
		foundOutsideDelimiter = false;
		
		for(int i = 0; i<scrubbedText.Length;i++){
			if(i+2 <= scrubbedText.Length && scrubbedText.Substring(i,2).Equals("<r")){
				int start = FindNext(scrubbedText.Substring(i+2), ">") + 3 + i;
				int end = FindNext(scrubbedText.Substring(i+2), "<r>") + 1 + i;
				bool foundInsideDelimiter = false;
				for(int a = start; a<=end;a++){
					if(scrubbedText.ToCharArray()[a].Equals('>') || scrubbedText.ToCharArray()[a].Equals('<')){
						foundInsideDelimiter = !foundInsideDelimiter;
						continue;
					}
					if(foundInsideDelimiter)
						continue;
					if(char.IsWhiteSpace(scrubbedText.ToCharArray()[a]))
						rainbowArray[charIndex] = -1;
					else
						rainbowArray[charIndex] = 1;
					charIndex++;
				}
				i = end+3;
				continue;
			}
			if(scrubbedText.ToCharArray()[i].Equals('>') || scrubbedText.ToCharArray()[i].Equals('<')){
				foundOutsideDelimiter = !foundOutsideDelimiter;
				continue;
			}
			if(foundOutsideDelimiter)
				continue;
			charIndex++;
		}
		
	}
	
	
	//ReScan Helper Functions =======================================================
	
	private int FindNext(string n, string x){
		return n.IndexOf(x);
	}
	
	//Reveal Function ===============================================================
	IEnumerator RevealCharacterCoroutine(){
		Vector3[] vertices = textComponent.mesh.vertices;
		originalColors = textComponent.mesh.colors;
		revealArray = new int[vertices.Length];
		int currentVertex = 0;
		for(int charIndex = 0;charIndex<realText.Length;charIndex++){
			if(waitArray[charIndex] == 0){
				yield return new WaitForSeconds(0.04f); //If there was no <t>, wait for this normal time
			}
			else{
				yield return new WaitForSeconds(waitArray[charIndex]); //If there was <t>, wait for the specified time
			}
			
			//Play one shot
			
			if(char.IsWhiteSpace(realText.ToCharArray()[charIndex])){
				continue;	
			}
			for(int a = 0; a < 4; a++){
				revealArray[currentVertex] = 1;
				currentVertex++;
			}
		}
	}
	
	private void RevealCharacters(TMP_TextInfo textInfo){
		Vector3[] vertices = textComponent.mesh.vertices;
		Color[] setColors = new Color[vertices.Length];
		for(int i = 0; i < vertices.Length; i++){
			if(revealArray[i] == 0){
				setColors[i] = Color.clear;
			}
			else{
				setColors[i] = originalColors[i];
			}
		}
		textComponent.mesh.colors = setColors;
	}
	
	
	//Effect Functions ===============================================================
	
	private void EffectWave(TMP_TextInfo textInfo){
		//Uses sin function to displace y location of vertices
		for(int i = 0; i<textInfo.characterCount; ++i){
			var charInfo = textInfo.characterInfo[i];
			if(!charInfo.isVisible){
				continue;
			}
			if(waveArray[i] == 0){
				continue;
			}	
			var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
			for(int j = 0; j<4; ++j){	    	
				var orig = verts[charInfo.vertexIndex + j];
				verts[charInfo.vertexIndex + j] = orig + new Vector3(0, Mathf.Sin(Time.time*WaveTime + orig.x*XWaveWeight*0.1f)*(1/WaveSize*textComponent.fontSize),0);
			}
		}
	}
	
	
	private void EffectShake(TMP_TextInfo textInfo){
		//Uses randomly generated numbers to displace location of verticies
		
		//Check if this frame should shake
		shakeFrame++;
		if(shakeFrame == ShakeSpeed){
			shakeFrame = 0;
		}
		else{
			return;
		}
		
		
		for(int i = 0; i<textInfo.characterCount; ++i){
			var charInfo = textInfo.characterInfo[i];
			if(!charInfo.isVisible){
				continue;
			}
			if(shakeArray[i] == 0){
				continue;
			}
			var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
			for(int j = 0; j<4; ++j){
				Vector3 shakeVector = new Vector3(Random.Range(0,MaxShake)*textComponent.fontSize*0.001f,Random.Range(0,MaxShake)*textComponent.fontSize*0.001f,0);
				var orig = verts[charInfo.vertexIndex + j];
				verts[charInfo.vertexIndex + j] = orig + shakeVector;
			}
		}
	}
	
	
	private void EffectRainbow(TMP_TextInfo textInfo){
		//Uses sin function to set Hue value of HSV color along verticies
		Vector3[] vertices = textComponent.mesh.vertices;
		Color[] colors = new Color[vertices.Length];
		
		for(int i = 0;i <colors.Length;i++){
			colors[i] = referenceColor;
		}
		int vertIndex = 0;
		for(int i = 0; i<textInfo.characterCount; ++i){
			var charInfo = textInfo.characterInfo[i];
			if(!charInfo.isVisible){
				continue;
			}
			if(rainbowArray[i] == 0){
				vertIndex+=4;
				continue;
			}		
			if(revealArray[vertIndex] == 0){ //Has the coroutine revealed this yet
				continue;
			}
			var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
			for(int j = 0; j<4; ++j){
				float hue = Mathf.Lerp(0.03f,0.97f,0.5f*(Mathf.Sin(Time.time*TimeWeight + vertices[vertIndex].x*XColorWeight)+1));
				colors[vertIndex] = Color.HSVToRGB(hue, 1,1);
				vertIndex++;
			}
		}
		
		for (int i = 0; i < vertices.Length; i++){
			if(colors[i].Equals(referenceColor)){
				colors[i] = textComponent.mesh.colors[i];
			}
		}
		
		textComponent.mesh.colors = colors;
	}
	

	
	//UNUSED FUNCTIONS =================================================================================
	private void EffectRainbowLegacy(){
		Vector3[] vertices = textComponent.mesh.vertices;
		Color[] colors = new Color[vertices.Length];
		int currentChar = 0;
		for (int i = 0; i < vertices.Length; i++){
			Debug.Log(currentChar);
			if(i %4 == 0 && i != 0 )
				currentChar++;
			if(rainbowArray[currentChar] != 1){
				continue;
			}

			float hue = Mathf.Lerp(0,1,0.5f*(Mathf.Sin(Time.time*TimeWeight + vertices[i].x*XColorWeight)+1));
			colors[i] = Color.HSVToRGB(hue, 1,1);
			
		}
		textComponent.mesh.colors = colors;
	}

}
