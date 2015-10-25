using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

// Source: UIToolkit -- https://github.com/prime31/UIToolkit/blob/master/Assets/Plugins/MiniJSON.cs

// Based on the JSON parser from 
// http://techblog.procurios.nl/k/618/news/view/14605/14863/How-do-I-write-my-own-parser-for-JSON.html

/// <summary>
/// This class encodes and decodes JSON strings.
/// Spec. details, see http://www.json.org/
/// 
/// JSON uses Arrays and Objects. These correspond here to the datatypes ArrayList and Hashtable.
/// All numbers are parsed to doubles.
/// </summary>

public class FlareJson
{
	private const int TOKEN_NONE = 0;
	private const int TOKEN_CURLY_OPEN = 1;
	private const int TOKEN_CURLY_CLOSE = 2;
	private const int TOKEN_SQUARED_OPEN = 3;
	private const int TOKEN_SQUARED_CLOSE = 4;
	private const int TOKEN_COLON = 5;
	private const int TOKEN_COMMA = 6;
	private const int TOKEN_STRING = 7;
	private const int TOKEN_NUMBER = 8;
	private const int TOKEN_TRUE = 9;
	private const int TOKEN_FALSE = 10;
	private const int TOKEN_NULL = 11;
	private const int BUILDER_CAPACITY = 2000;

	/// <summary>
	/// On decoding, this value holds the position at which the parse failed (-1 = no error).
	/// </summary>
	protected static int lastErrorIndex = -1;
	protected static string lastDecode = "";
	
	public static Color decodeColor(Hashtable table){
		
		float r = float.Parse(table["r"].ToString());
		float g = float.Parse(table["g"].ToString());
		float b = float.Parse(table["b"].ToString());
		float a = float.Parse(table["a"].ToString());
		
		return new Color(r,g,b,a);
	}
	
	public static Vector4 decodeVector4(Hashtable table){
		
		float r = float.Parse(table["r"].ToString());
		float g = float.Parse(table["g"].ToString());
		float b = float.Parse(table["b"].ToString());
		float a = float.Parse(table["a"].ToString());
		
		return new Vector4(r,g,b,a);
	}
	
	public static Vector3 decodeVector3(Hashtable table){
		
		float r = float.Parse(table["r"].ToString());
		float g = float.Parse(table["g"].ToString());
		float b = float.Parse(table["b"].ToString());
		
		return new Vector4(r,g,b);
	}
	
	public static Vector2 decodeVector2(Hashtable table){
		
		float x = float.Parse(table["x"].ToString());
		float y = float.Parse(table["y"].ToString());
		
		return new Vector2(x,y);
	}
	
	
	public static AnimationCurve decodeAnimCurve(Hashtable table){

		AnimationCurve curve = new AnimationCurve ();

		
		foreach (System.Collections.DictionaryEntry key in table) {


			Hashtable keyTable = (Hashtable)table [key.Key];


			float time = float.Parse(keyTable["time"].ToString());
			float value = float.Parse(keyTable["value"].ToString());
			float _in = float.Parse(keyTable["in"].ToString());
			float _out = float.Parse(keyTable["out"].ToString());
//			float tangentMode = float.Parse(keyTable["tangentMode"].ToString());

			Keyframe newKey = new Keyframe(time,value,_in,_out);
			//			newKey.tangentMode = tangentMode;

			curve.AddKey(newKey);

		}

		return curve;
	}

	public static bool decodeBool(string str){

		int num =  int.Parse(str);
		if(num == 1)
			return true;
		else
			return false;
	}

	public static void LoadFlareData (ProFlare flare, TextAsset asset)
	{
		
		string jsonString = asset.text;

//		Debug.Log ("LoadFlareData");

		Hashtable decodedHash = jsonDecode (jsonString) as Hashtable;
		
		if (decodedHash == null) {
			Debug.LogWarning ("Unable to parse Json file: " + asset.name);
			return;
		} 

		Hashtable meta = (Hashtable)decodedHash["meta"];
			
		flare.GlobalScale = float.Parse(meta["GlobalScale"].ToString());

		flare.GlobalBrightness = float.Parse(meta["GlobalBrightness"].ToString());

		flare.GlobalTintColor = FlareJson.decodeColor((Hashtable)meta["GlobalTintColor"]);

		flare.MultiplyScaleByTransformScale = FlareJson.decodeBool(meta["MultiplyScaleByTransformScale"].ToString());

		//Distance Fall off
		flare.useMaxDistance = FlareJson.decodeBool(meta["useMaxDistance"].ToString());

		flare.useDistanceScale = FlareJson.decodeBool(meta["useDistanceScale"].ToString());

		flare.useDistanceFade = FlareJson.decodeBool(meta["useDistanceFade"].ToString());

		flare.GlobalMaxDistance = float.Parse(meta["GlobalMaxDistance"].ToString());

					
		//Angle Culling Properties
		flare.UseAngleLimit = FlareJson.decodeBool(meta["UseAngleLimit"].ToString());

		flare.maxAngle = float.Parse(meta["maxAngle"].ToString());

		flare.UseAngleScale = FlareJson.decodeBool(meta["UseAngleScale"].ToString());

		flare.UseAngleBrightness = FlareJson.decodeBool(meta["UseAngleBrightness"].ToString());

		flare.UseAngleCurve = FlareJson.decodeBool(meta["UseAngleCurve"].ToString());

		flare.AngleCurve = FlareJson.decodeAnimCurve ((Hashtable)meta ["AngleCurve"]);

		//			public LayerMask mask = 1;

		flare.RaycastPhysics = FlareJson.decodeBool(meta["RaycastPhysics"].ToString());

		flare.OffScreenFadeDist = float.Parse(meta["OffScreenFadeDist"].ToString());

		flare.useDynamicEdgeBoost = FlareJson.decodeBool(meta["useDynamicEdgeBoost"].ToString());

		flare.DynamicEdgeBoost = float.Parse(meta["DynamicEdgeBoost"].ToString());

		flare.DynamicEdgeBrightness = float.Parse(meta["DynamicEdgeBrightness"].ToString());

		flare.DynamicEdgeRange = float.Parse(meta["DynamicEdgeRange"].ToString());

		flare.DynamicEdgeBias = float.Parse(meta["DynamicEdgeBias"].ToString());

		flare.DynamicEdgeCurve = FlareJson.decodeAnimCurve ((Hashtable)meta ["DynamicEdgeCurve"]);

		flare.useDynamicCenterBoost = FlareJson.decodeBool(meta["useDynamicCenterBoost"].ToString());

		flare.DynamicCenterBoost = float.Parse(meta["DynamicCenterBoost"].ToString());

		flare.DynamicCenterBrightness = float.Parse(meta["DynamicCenterBrightness"].ToString());

		flare.DynamicCenterRange = float.Parse(meta["DynamicCenterRange"].ToString());

		flare.DynamicCenterBias = float.Parse(meta["DynamicCenterBias"].ToString());

		flare.neverCull = FlareJson.decodeBool(meta["neverCull"].ToString());

		flare.Elements.Clear ();

		Hashtable elements = (Hashtable)meta["Elements"];

		foreach (System.Collections.DictionaryEntry item in elements) {

			Hashtable element = (Hashtable)elements[item.Key];

			ProFlareElement elementNew = new ProFlareElement();
			
			elementNew.Editing = FlareJson.decodeBool(element["Editing"].ToString());

			elementNew.Visible = FlareJson.decodeBool(element["Visible"].ToString());

			elementNew.SpriteName = element["SpriteName"].ToString();

			elementNew.flare = flare;

			elementNew.flareAtlas = flare._Atlas;

			elementNew.Brightness = float.Parse(element["Brightness"].ToString());

			elementNew.Scale = float.Parse(element["Scale"].ToString());

			elementNew.ScaleRandom = float.Parse(element["ScaleRandom"].ToString());

			elementNew.ScaleFinal = float.Parse(element["ScaleFinal"].ToString());

			elementNew.RandomColorAmount = FlareJson.decodeVector4((Hashtable)element["RandomColorAmount"]);

//			//Element OffSet Properties
			elementNew.position = float.Parse(element["position"].ToString());

			elementNew.useRangeOffset = FlareJson.decodeBool(element["useRangeOffset"].ToString());

			elementNew.SubElementPositionRange_Min = float.Parse(element["SubElementPositionRange_Min"].ToString());

			elementNew.SubElementPositionRange_Max = float.Parse(element["SubElementPositionRange_Max"].ToString());

			elementNew.SubElementAngleRange_Min = float.Parse(element["SubElementAngleRange_Min"].ToString());

			elementNew.SubElementAngleRange_Max = float.Parse(element["SubElementAngleRange_Max"].ToString());

			elementNew.OffsetPosition = FlareJson.decodeVector3((Hashtable)element["OffsetPosition"]);

			elementNew.Anamorphic = FlareJson.decodeVector3((Hashtable)element["Anamorphic"]);

			elementNew.OffsetPostion = FlareJson.decodeVector3((Hashtable)element["OffsetPostion"]);

//			//Element Rotation Properties
			elementNew.angle = float.Parse(element["angle"].ToString());

			elementNew.useRandomAngle = FlareJson.decodeBool(element["useRandomAngle"].ToString());

			elementNew.useStarRotation = FlareJson.decodeBool(element["useStarRotation"].ToString());

			elementNew.AngleRandom_Min = float.Parse(element["AngleRandom_Min"].ToString());

			elementNew.AngleRandom_Max = float.Parse(element["AngleRandom_Max"].ToString());

			elementNew.OrientToSource = FlareJson.decodeBool(element["OrientToSource"].ToString());

			elementNew.rotateToFlare = FlareJson.decodeBool(element["rotateToFlare"].ToString());

			elementNew.rotationSpeed = float.Parse(element["rotationSpeed"].ToString());

			elementNew.rotationOverTime = float.Parse(element["rotationOverTime"].ToString());

//			//Colour Properties,
			elementNew.useColorRange = FlareJson.decodeBool(element["useColorRange"].ToString());

			elementNew.OffsetPosition = FlareJson.decodeVector3((Hashtable)element["OffsetPosition"]);

			elementNew.ElementTint = FlareJson.decodeColor((Hashtable)element["ElementTint"]);

			elementNew.SubElementColor_Start = FlareJson.decodeColor((Hashtable)element["SubElementColor_Start"]);

			elementNew.SubElementColor_End = FlareJson.decodeColor((Hashtable)element["SubElementColor_End"]);

//			//Scale Curve
			elementNew.useScaleCurve = FlareJson.decodeBool(element["useScaleCurve"].ToString());

			elementNew.ScaleCurve = FlareJson.decodeAnimCurve ((Hashtable)element ["ScaleCurve"]);

//			//Override Properties
			elementNew.OverrideDynamicEdgeBoost = FlareJson.decodeBool(element["OverrideDynamicEdgeBoost"].ToString());

			elementNew.DynamicEdgeBoostOverride = float.Parse(element["DynamicEdgeBoostOverride"].ToString());

			elementNew.OverrideDynamicCenterBoost = FlareJson.decodeBool(element["OverrideDynamicCenterBoost"].ToString());

			elementNew.DynamicCenterBoostOverride = float.Parse(element["DynamicCenterBoostOverride"].ToString());

			elementNew.OverrideDynamicEdgeBrightness = FlareJson.decodeBool(element["OverrideDynamicEdgeBrightness"].ToString());

			elementNew.DynamicEdgeBrightnessOverride = float.Parse(element["DynamicEdgeBrightnessOverride"].ToString());

			elementNew.OverrideDynamicCenterBrightness = FlareJson.decodeBool(element["OverrideDynamicCenterBrightness"].ToString());

			elementNew.DynamicCenterBrightnessOverride = float.Parse(element["DynamicCenterBrightnessOverride"].ToString());

			elementNew.type = (ProFlareElement.Type)(int.Parse(element["type"].ToString()));


			elementNew.size = FlareJson.decodeVector2((Hashtable)element["size"]);

			Hashtable subElements = (Hashtable)element["subElements"];

			if(subElements != null)
			foreach (System.Collections.DictionaryEntry subItem in subElements) {

				Hashtable subElement = (Hashtable)subElements[subItem.Key];

				SubElement subElementNew = new SubElement();

				subElementNew.color = FlareJson.decodeColor((Hashtable)subElement["color"]);

				subElementNew.position = float.Parse(subElement["position"].ToString());

				subElementNew.offset = FlareJson.decodeVector3((Hashtable)subElement["offset"]);

				subElementNew.angle = float.Parse(subElement["angle"].ToString());

				subElementNew.scale = float.Parse(subElement["scale"].ToString());

				subElementNew.random = float.Parse(subElement["random"].ToString());

				subElementNew.random2 = float.Parse(subElement["random2"].ToString());

				subElementNew.RandomScaleSeed = float.Parse(subElement["RandomScaleSeed"].ToString());

				subElementNew.RandomColorSeedR = float.Parse(subElement["RandomColorSeedR"].ToString());

				subElementNew.RandomColorSeedG = float.Parse(subElement["RandomColorSeedG"].ToString());

				subElementNew.RandomColorSeedB = float.Parse(subElement["RandomColorSeedB"].ToString());

				subElementNew.RandomColorSeedA = float.Parse(subElement["RandomColorSeedA"].ToString());

				elementNew.subElements.Add(subElementNew);
			}

			bool Found = false;

			for(int i2 = 0; i2 < flare._Atlas.elementsList.Count; i2++){
				if(elementNew.SpriteName == flare._Atlas.elementsList[i2].name){
					Found = true;
					elementNew.elementTextureID = i2;
				}
			}

			if(Found)
				flare.Elements.Add(elementNew);
			else
				Debug.LogWarning("ProFlares - Flare Element Missing From Atlas Not Adding - "+elementNew.SpriteName);

		}

		foreach (ProFlareBatch batch in flare.FlareBatches) {
			batch.dirty = true;
		}
	}
	/// <summary>
	/// Parse the specified JSon file, loading sprite information for the specified atlas.
	/// </summary>

	public static void LoadSpriteData (ProFlareAtlas atlas, TextAsset asset)
	{
		if (asset == null || atlas == null) return;

		string jsonString = asset.text;
		
		Hashtable decodedHash = jsonDecode(jsonString) as Hashtable;
		
		if (decodedHash == null)
		{
			Debug.LogWarning("Unable to parse Json file: " + asset.name);
			return;
		} 
		List<ProFlareAtlas.Element> oldElements = atlas.elementsList;
		
		atlas.elementsList = new List<ProFlareAtlas.Element>();
		
		Vector2 TextureScale = Vector2.one;
		
		//Find Texture Size
		Hashtable meta = (Hashtable)decodedHash["meta"];
		foreach (System.Collections.DictionaryEntry item in meta)
		{
			if(item.Key.ToString() == "size"){
				Hashtable sizeTable = (Hashtable)item.Value;
				
				TextureScale.x = int.Parse(sizeTable["w"].ToString());
				TextureScale.y = int.Parse(sizeTable["h"].ToString());
			}
		}
		
		//Debug.LogError(TextureScale);
		
		Hashtable frames = (Hashtable)decodedHash["frames"];
		foreach (System.Collections.DictionaryEntry item in frames)
		{
			ProFlareAtlas.Element newElement = new ProFlareAtlas.Element();
			newElement.name = item.Key.ToString();

			bool exists = false;

			// Check to see if this sprite exists
			foreach (ProFlareAtlas.Element oldSprite in oldElements)
			{
				if (oldSprite.name.Equals(newElement.name, StringComparison.OrdinalIgnoreCase))
				{
					exists = true;
					break;
				}
			}
 
			if (!exists)
			{
				newElement.name = newElement.name.Replace(".png", "");
				newElement.name = newElement.name.Replace(".tga", "");
				newElement.name = newElement.name.Replace(".psd", "");
				newElement.name = newElement.name.Replace(".PSD", "");
			}
 
			Hashtable table = (Hashtable)item.Value;
			Hashtable frame = (Hashtable)table["frame"];

			int frameX = int.Parse(frame["x"].ToString());
			int frameY = int.Parse(frame["y"].ToString());
			int frameW = int.Parse(frame["w"].ToString());
			int frameH = int.Parse(frame["h"].ToString());
 
			Rect finalUVs = new Rect(frameX, frameY, frameW, frameH);
			
			Rect rect = new Rect(frameX, frameY, frameW, frameH);
			
			float width = TextureScale.x;
			float height = TextureScale.y;
			
			if (width != 0f && height != 0f)
			{
				finalUVs.xMin = rect.xMin / width;
				finalUVs.xMax = rect.xMax / width;
				finalUVs.yMin = 1f - rect.yMax / height;
				finalUVs.yMax = 1f - rect.yMin / height;
			}
			
			newElement.UV = finalUVs;
			newElement.Imported = true;
			 
 
			
			atlas.elementsList.Add(newElement);
		}
		
		foreach (ProFlareAtlas.Element oldSprite in oldElements)
		{
			if (!oldSprite.Imported)
			{
				atlas.elementsList.Add(oldSprite);
			}
		}

		// Sort imported sprites alphabetically
		
		atlas.elementsList.Sort(CompareSprites);
		
		Debug.Log("PROFLARES - Imported " + atlas.elementsList.Count + " Elements");
		
		// Unload the asset
		asset = null;
		Resources.UnloadUnusedAssets();
	}

	/// <summary>
	/// Sprite comparison function for sorting.
	/// </summary>
	
	static int CompareSprites (ProFlareAtlas.Element a, ProFlareAtlas.Element b) { return a.name.CompareTo(b.name); }

	/// <summary>
	/// Copy the inner rectangle from one sprite to another.
	/// </summary>
	/*
	static void CopyInnerRect (ProFlareAtlas.Element oldSprite, ProFlareAtlas.Element newElement)
	{
		float offsetX = oldSprite.inner.xMin - oldSprite.outer.xMin;
		float offsetY = oldSprite.inner.yMin - oldSprite.outer.yMin;
		float sizeX = oldSprite.inner.width;
		float sizeY = oldSprite.inner.height;

		if (Mathf.Approximately(newElement.outer.width, oldSprite.outer.width))
		{
			// The sprite has not been rotated or it's a square
			newElement.inner = new Rect(newElement.outer.xMin + offsetX, newElement.outer.yMin + offsetY, sizeX, sizeY);
		}
		else if (Mathf.Approximately(newElement.outer.width, oldSprite.outer.height))
		{
			// The sprite was rotated since the last time it was imported
			newElement.inner = new Rect(newElement.outer.xMin + offsetY, newElement.outer.yMin + offsetX, sizeY, sizeX);
		}
	}
	 */
	/// <summary>
	/// Parses the string json into a value
	/// </summary>
	/// <param name="json">A JSON string.</param>
	/// <returns>An ArrayList, a Hashtable, a double, a string, null, true, or false</returns>
	public static object jsonDecode( string json )
	{
		// save the string for debug information
		FlareJson.lastDecode = json;

		if( json != null )
		{
			char[] charArray = json.ToCharArray();
			int index = 0;
			bool success = true;
			object value = FlareJson.parseValue( charArray, ref index, ref success );

			if( success ){
				Debug.Log("jsonDecode success"); 
				FlareJson.lastErrorIndex = -1;
			}
			else{

				FlareJson.lastErrorIndex = index;
			}
			return value;
		}
		else
		{
			return null;
		}
	}


	/// <summary>
	/// Converts a Hashtable / ArrayList / Dictionary(string,string) object into a JSON string
	/// </summary>
	/// <param name="json">A Hashtable / ArrayList</param>
	/// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
	public static string jsonEncode( object json )
	{
		var builder = new StringBuilder( BUILDER_CAPACITY );
		var success = FlareJson.serializeValue( json, builder );
		
		return ( success ? builder.ToString() : null );
	}


	/// <summary>
	/// On decoding, this function returns the position at which the parse failed (-1 = no error).
	/// </summary>
	/// <returns></returns>
	public static bool lastDecodeSuccessful()
	{
		return ( FlareJson.lastErrorIndex == -1 );
	}


	/// <summary>
	/// On decoding, this function returns the position at which the parse failed (-1 = no error).
	/// </summary>
	/// <returns></returns>
	public static int getLastErrorIndex()
	{
		return FlareJson.lastErrorIndex;
	}


	/// <summary>
	/// If a decoding error occurred, this function returns a piece of the JSON string 
	/// at which the error took place. To ease debugging.
	/// </summary>
	/// <returns></returns>
	public static string getLastErrorSnippet()
	{
		if( FlareJson.lastErrorIndex == -1 )
		{
			return "";
		}
		else
		{
			int startIndex = FlareJson.lastErrorIndex - 5;
			int endIndex = FlareJson.lastErrorIndex + 15;
			if( startIndex < 0 )
				startIndex = 0;

			if( endIndex >= FlareJson.lastDecode.Length )
				endIndex = FlareJson.lastDecode.Length - 1;

			return FlareJson.lastDecode.Substring( startIndex, endIndex - startIndex + 1 );
		}
	}

	
	#region Parsing
	
	protected static Hashtable parseObject( char[] json, ref int index )
	{
		Hashtable table = new Hashtable();
		int token;

		// {
		nextToken( json, ref index );

		bool done = false;
		while( !done )
		{
			token = lookAhead( json, index );
			if( token == FlareJson.TOKEN_NONE )
			{
				return null;
			}
			else if( token == FlareJson.TOKEN_COMMA )
			{
				nextToken( json, ref index );
			}
			else if( token == FlareJson.TOKEN_CURLY_CLOSE )
			{
				nextToken( json, ref index );
				return table;
			}
			else
			{
				// name
				string name = parseString( json, ref index );
				if( name == null )
				{
					return null;
				}

				// :
				token = nextToken( json, ref index );
				if( token != FlareJson.TOKEN_COLON )
					return null;

				// value
				bool success = true;
				object value = parseValue( json, ref index, ref success );
				if( !success )
					return null;

				table[name] = value;
			}
		}

		return table;
	}

	
	protected static ArrayList parseArray( char[] json, ref int index )
	{
		ArrayList array = new ArrayList();

		// [
		nextToken( json, ref index );

		bool done = false;
		while( !done )
		{
			int token = lookAhead( json, index );
			if( token == FlareJson.TOKEN_NONE )
			{
				return null;
			}
			else if( token == FlareJson.TOKEN_COMMA )
			{
				nextToken( json, ref index );
			}
			else if( token == FlareJson.TOKEN_SQUARED_CLOSE )
			{
				nextToken( json, ref index );
				break;
			}
			else
			{
				bool success = true;
				object value = parseValue( json, ref index, ref success );
				if( !success )
					return null;

				array.Add( value );
			}
		}

		return array;
	}

	
	protected static object parseValue( char[] json, ref int index, ref bool success )
	{
		switch( lookAhead( json, index ) )
		{
			case FlareJson.TOKEN_STRING:
				return parseString( json, ref index );
			case FlareJson.TOKEN_NUMBER:
				return parseNumber( json, ref index );
			case FlareJson.TOKEN_CURLY_OPEN:
				return parseObject( json, ref index );
			case FlareJson.TOKEN_SQUARED_OPEN:
				return parseArray( json, ref index );
			case FlareJson.TOKEN_TRUE:
				nextToken( json, ref index );
				return Boolean.Parse( "TRUE" );
			case FlareJson.TOKEN_FALSE:
				nextToken( json, ref index );
				return Boolean.Parse( "FALSE" );
			case FlareJson.TOKEN_NULL:
				nextToken( json, ref index );
				return null;
			case FlareJson.TOKEN_NONE:
				break;
		}

		success = false;
		return null;
	}

	
	protected static string parseString( char[] json, ref int index )
	{
		string s = "";
		char c;

		eatWhitespace( json, ref index );
		
		// "
		c = json[index++];

		bool complete = false;
		while( !complete )
		{
			if( index == json.Length )
				break;

			c = json[index++];
			if( c == '"' )
			{
				complete = true;
				break;
			}
			else if( c == '\\' )
			{
				if( index == json.Length )
					break;

				c = json[index++];
				if( c == '"' )
				{
					s += '"';
				}
				else if( c == '\\' )
				{
					s += '\\';
				}
				else if( c == '/' )
				{
					s += '/';
				}
				else if( c == 'b' )
				{
					s += '\b';
				}
				else if( c == 'f' )
				{
					s += '\f';
				}
				else if( c == 'n' )
				{
					s += '\n';
				}
				else if( c == 'r' )
				{
					s += '\r';
				}
				else if( c == 't' )
				{
					s += '\t';
				}
				else if( c == 'u' )
				{
					int remainingLength = json.Length - index;
					if( remainingLength >= 4 )
					{
						char[] unicodeCharArray = new char[4];
						Array.Copy( json, index, unicodeCharArray, 0, 4 );

						// Drop in the HTML markup for the unicode character
						s += "&#x" + new string( unicodeCharArray ) + ";";

						/*
uint codePoint = UInt32.Parse(new string(unicodeCharArray), NumberStyles.HexNumber);
// convert the integer codepoint to a unicode char and add to string
s += Char.ConvertFromUtf32((int)codePoint);
*/

						// skip 4 chars
						index += 4;
					}
					else
					{
						break;
					}

				}
			}
			else
			{
				s += c;
			}

		}

		if( !complete )
			return null;

		return s;
	}
	
	
	protected static double parseNumber( char[] json, ref int index )
	{
		eatWhitespace( json, ref index );

		int lastIndex = getLastIndexOfNumber( json, index );
		int charLength = ( lastIndex - index ) + 1;
		char[] numberCharArray = new char[charLength];

		Array.Copy( json, index, numberCharArray, 0, charLength );
		index = lastIndex + 1;
		return Double.Parse( new string( numberCharArray ) ); // , CultureInfo.InvariantCulture);
	}
	
	
	protected static int getLastIndexOfNumber( char[] json, int index )
	{
		int lastIndex;
		for( lastIndex = index; lastIndex < json.Length; lastIndex++ )
			if( "0123456789+-.eE".IndexOf( json[lastIndex] ) == -1 )
			{
				break;
			}
		return lastIndex - 1;
	}
	
	
	protected static void eatWhitespace( char[] json, ref int index )
	{
		for( ; index < json.Length; index++ )
			if( " \t\n\r".IndexOf( json[index] ) == -1 )
			{
				break;
			}
	}
	
	
	protected static int lookAhead( char[] json, int index )
	{
		int saveIndex = index;
		return nextToken( json, ref saveIndex );
	}

	
	protected static int nextToken( char[] json, ref int index )
	{
		eatWhitespace( json, ref index );

		if( index == json.Length )
		{
			return FlareJson.TOKEN_NONE;
		}
		
		char c = json[index];
		index++;
		switch( c )
		{
			case '{':
				return FlareJson.TOKEN_CURLY_OPEN;
			case '}':
				return FlareJson.TOKEN_CURLY_CLOSE;
			case '[':
				return FlareJson.TOKEN_SQUARED_OPEN;
			case ']':
				return FlareJson.TOKEN_SQUARED_CLOSE;
			case ',':
				return FlareJson.TOKEN_COMMA;
			case '"':
				return FlareJson.TOKEN_STRING;
			case '0':
			case '1':
			case '2':
			case '3':
			case '4': 
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
			case '-': 
				return FlareJson.TOKEN_NUMBER;
			case ':':
				return FlareJson.TOKEN_COLON;
		}
		index--;

		int remainingLength = json.Length - index;

		// false
		if( remainingLength >= 5 )
		{
			if( json[index] == 'f' &&
				json[index + 1] == 'a' &&
				json[index + 2] == 'l' &&
				json[index + 3] == 's' &&
				json[index + 4] == 'e' )
			{
				index += 5;
				return FlareJson.TOKEN_FALSE;
			}
		}

		// true
		if( remainingLength >= 4 )
		{
			if( json[index] == 't' &&
				json[index + 1] == 'r' &&
				json[index + 2] == 'u' &&
				json[index + 3] == 'e' )
			{
				index += 4;
				return FlareJson.TOKEN_TRUE;
			}
		}

		// null
		if( remainingLength >= 4 )
		{
			if( json[index] == 'n' &&
				json[index + 1] == 'u' &&
				json[index + 2] == 'l' &&
				json[index + 3] == 'l' )
			{
				index += 4;
				return FlareJson.TOKEN_NULL;
			}
		}

		return FlareJson.TOKEN_NONE;
	}

	#endregion
	
	
	#region Serialization
	
	protected static bool serializeObjectOrArray( object objectOrArray, StringBuilder builder )
	{
		if( objectOrArray is Hashtable )
		{
			return serializeObject( (Hashtable)objectOrArray, builder );
		}
		else if( objectOrArray is ArrayList )
			{
				return serializeArray( (ArrayList)objectOrArray, builder );
			}
			else
			{
				return false;
			}
	}

	
	protected static bool serializeObject( Hashtable anObject, StringBuilder builder )
	{
		builder.Append( "{" );

		IDictionaryEnumerator e = anObject.GetEnumerator();
		bool first = true;
		while( e.MoveNext() )
		{
			string key = e.Key.ToString();
			object value = e.Value;

			if( !first )
			{
				builder.Append( ", " );
			}

			serializeString( key, builder );
			builder.Append( ":" );
			if( !serializeValue( value, builder ) )
			{
				return false;
			}

			first = false;
		}

		builder.Append( "}" );
		return true;
	}
	
	
	protected static bool serializeDictionary( Dictionary<string,string> dict, StringBuilder builder )
	{
		builder.Append( "{" );
		
		bool first = true;
		foreach( var kv in dict )
		{
			if( !first )
				builder.Append( ", " );
			
			serializeString( kv.Key, builder );
			builder.Append( ":" );
			serializeString( kv.Value, builder );

			first = false;
		}

		builder.Append( "}" );
		return true;
	}
	
	
	protected static bool serializeArray( ArrayList anArray, StringBuilder builder )
	{
		builder.Append( "[" );

		bool first = true;
		for( int i = 0; i < anArray.Count; i++ )
		{
			object value = anArray[i];

			if( !first )
			{
				builder.Append( ", " );
			}

			if( !serializeValue( value, builder ) )
			{
				return false;
			}

			first = false;
		}

		builder.Append( "]" );
		return true;
	}

	
	protected static bool serializeValue( object value, StringBuilder builder )
	{
		// Type t = value.GetType();
		// Debug.Log("type: " + t.ToString() + " isArray: " + t.IsArray);

		if( value == null )
		{
			builder.Append( "null" );
		}
		else if( value.GetType().IsArray )
		{
			serializeArray( new ArrayList( (ICollection)value ), builder );
		}
		else if( value is string )
		{
			serializeString( (string)value, builder );
		}
		else if( value is Char )
		{
			serializeString( Convert.ToString( (char)value ), builder );
		}
		else if( value is Hashtable )
		{
			serializeObject( (Hashtable)value, builder );
		}
		else if( value is Dictionary<string,string> )
		{
			serializeDictionary( (Dictionary<string,string>)value, builder );
		}
		else if( value is ArrayList )
		{
			serializeArray( (ArrayList)value, builder );
		}
		else if( ( value is Boolean ) && ( (Boolean)value == true ) )
		{
			builder.Append( "true" );
		}
		else if( ( value is Boolean ) && ( (Boolean)value == false ) )
		{
			builder.Append( "false" );
		}
		else if( value.GetType().IsPrimitive )
		{
			serializeNumber( Convert.ToDouble( value ), builder );
		}
		else
		{
			return false;
		}

		return true;
	}

	
	protected static void serializeString( string aString, StringBuilder builder )
	{
		builder.Append( "\"" );

		char[] charArray = aString.ToCharArray();
		for( int i = 0; i < charArray.Length; i++ )
		{
			char c = charArray[i];
			if( c == '"' )
			{
				builder.Append( "\\\"" );
			}
			else if( c == '\\' )
			{
				builder.Append( "\\\\" );
			}
			else if( c == '\b' )
			{
				builder.Append( "\\b" );
			}
			else if( c == '\f' )
			{
				builder.Append( "\\f" );
			}
			else if( c == '\n' )
			{
				builder.Append( "\\n" );
			}
			else if( c == '\r' )
			{
				builder.Append( "\\r" );
			}
			else if( c == '\t' )
			{
				builder.Append( "\\t" );
			}
			else
			{
				int codepoint = Convert.ToInt32( c );
				if( ( codepoint >= 32 ) && ( codepoint <= 126 ) )
				{
					builder.Append( c );
				}
				else
				{
					builder.Append( "\\u" + Convert.ToString( codepoint, 16 ).PadLeft( 4, '0' ) );
				}
			}
		}

		builder.Append( "\"" );
	}

	
	protected static void serializeNumber( double number, StringBuilder builder )
	{
		builder.Append( Convert.ToString( number ) ); // , CultureInfo.InvariantCulture));
	}
	
	#endregion
	
}
